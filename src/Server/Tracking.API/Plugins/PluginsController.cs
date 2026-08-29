using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tracking.PluginManager.Services;

namespace Tracking.API.Plugins;

[ApiController]
[Route("api/plugins")]
[Authorize]
public sealed class PluginsController : ControllerBase
{
    private readonly ProtocolPluginManager _pluginManager;
    private readonly PluginRepositoryService _repository;
    private readonly InstalledPluginStore _installed;

    public PluginsController(
        ProtocolPluginManager pluginManager,
        PluginRepositoryService repository,
        InstalledPluginStore installed)
    {
        _pluginManager = pluginManager;
        _repository = repository;
        _installed = installed;
    }

    [HttpGet("repository")]
    public IActionResult GetRepository()
    {
        return Ok(
            _repository
                .GetPackages());
    }

    [HttpGet]
    public IActionResult GetPlugins()
    {
        var installed =
            _installed
                .GetAll()
                .ToDictionary(
                    x => x.Id,
                    StringComparer.OrdinalIgnoreCase);

        var result =
            _pluginManager
                .GetStates()
                .Select(state =>
                {
                    installed.TryGetValue(
                        state.Id,
                        out var package);

                    return new
                    {
                        id = state.Id,
                        name = state.Name,
                        version = state.Version,
                        enabled = state.Enabled,
                        installed = package is not null
                    };
                })
                .OrderBy(x => x.name);

        return Ok(result);
    }

    [HttpGet("installed")]
    public IActionResult GetInstalled()
    {
        return Ok(
            _installed.GetAll());
    }

    [HttpPost("{pluginId}/install")]
    public IActionResult InstallPlugin(
        string pluginId)
    {
        var package =
            _repository.GetPackages()
                .FirstOrDefault(x =>
                    string.Equals(
                        x.PackageId,
                        pluginId,
                        StringComparison.OrdinalIgnoreCase));

        if (package is null)
        {
            return NotFound(new
            {
                message =
                    $"Plugin package '{pluginId}' was not found in repository."
            });
        }

        var source =
            _repository.GetPackagePayloadDirectory(
                pluginId);

        if (source is null)
        {
            return NotFound(new
            {
                message =
                    $"Plugin package directory '{pluginId}' was not found."
            });
        }

        try
        {
            var existing = _installed.Get(package.PackageId);

            _installed.InstallFromDirectory(
                source,
                package.PackageId);

            // A reinstall/upgrade must not silently change the
            // administrator's previous enabled state.
            if (existing is not null && !existing.Enabled)
            {
                _installed.SetEnabled(
                    package.PackageId,
                    false);
            }

            return Ok(new
            {
                id = package.PackageId,
                installed = true,
                enabled = existing?.Enabled ?? true,
                version = package.Version
            });
        }
        catch (Exception ex)
        {
            return Problem(
                title: "Plugin installation failed.",
                detail: ex.Message,
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost("{pluginId}/uninstall")]
    public IActionResult UninstallPlugin(
        string pluginId)
    {
        if (!_installed.Remove(pluginId))
        {
            return NotFound(new
            {
                message =
                    $"Plugin '{pluginId}' is not installed."
            });
        }

        return Ok(new
        {
            id = pluginId,
            installed = false
        });
    }

    [HttpPost("{pluginId}/enable")]
    public IActionResult EnablePlugin(
        string pluginId)
    {
        if (!_installed.SetEnabled(
                pluginId,
                true))
        {
            return NotFound(new
            {
                message =
                    $"Plugin '{pluginId}' is not installed."
            });
        }

        if (!_pluginManager.Enable(pluginId))
        {
            return Conflict(new
            {
                message =
                    $"Plugin '{pluginId}' is installed but not loaded by the runtime. Restart the server after installation."
            });
        }

        return Ok(new
        {
            id = pluginId,
            enabled = true
        });
    }

    [HttpPost("{pluginId}/disable")]
    public IActionResult DisablePlugin(
        string pluginId)
    {
        if (!_installed.SetEnabled(
                pluginId,
                false))
        {
            return NotFound(new
            {
                message =
                    $"Plugin '{pluginId}' is not installed."
            });
        }

        if (!_pluginManager.Disable(pluginId))
        {
            return Conflict(new
            {
                message =
                    $"Plugin '{pluginId}' is installed but not currently loaded."
            });
        }

        return Ok(new
        {
            id = pluginId,
            enabled = false
        });
    }
}
