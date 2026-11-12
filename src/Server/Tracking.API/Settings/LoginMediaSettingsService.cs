using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;

namespace Tracking.API.Settings;

public interface ILoginMediaSettingsService
{
    Task<LoginMediaSetting> GetAsync(CancellationToken cancellationToken);
    Task<LoginMediaSetting> SetUrlAsync(string type, string url, CancellationToken cancellationToken);
    Task<LoginMediaSetting> SetUploadedFileAsync(string type, string relativeUrl, CancellationToken cancellationToken);
    Task ResetAsync(CancellationToken cancellationToken);
}

public sealed record LoginMediaSetting(string? Type, string? Url);

public sealed class LoginMediaSettingsService : ILoginMediaSettingsService
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public LoginMediaSettingsService(IWebHostEnvironment env)
    {
        var directory = Path.Combine(env.ContentRootPath, "App_Data");
        Directory.CreateDirectory(directory);
        _filePath = Path.Combine(directory, "login-media.json");
    }

    public async Task<LoginMediaSetting> GetAsync(CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            return await ReadAsync(cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    public Task<LoginMediaSetting> SetUrlAsync(
        string type, string url, CancellationToken cancellationToken)
        => WriteAsync(new LoginMediaSetting(type, url), cancellationToken);

    public Task<LoginMediaSetting> SetUploadedFileAsync(
        string type, string relativeUrl, CancellationToken cancellationToken)
        => WriteAsync(new LoginMediaSetting(type, relativeUrl), cancellationToken);

    public async Task ResetAsync(CancellationToken cancellationToken)
    {
        await WriteAsync(new LoginMediaSetting(null, null), cancellationToken);
    }

    private async Task<LoginMediaSetting> ReadAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_filePath))
        {
            return new LoginMediaSetting(null, null);
        }

        await using var stream = File.OpenRead(_filePath);

        var setting = await JsonSerializer.DeserializeAsync<LoginMediaSetting>(
            stream, cancellationToken: cancellationToken);

        return setting ?? new LoginMediaSetting(null, null);
    }

    private async Task<LoginMediaSetting> WriteAsync(
        LoginMediaSetting setting, CancellationToken cancellationToken)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            await using var stream = File.Create(_filePath);

            await JsonSerializer.SerializeAsync(
                stream,
                setting,
                new JsonSerializerOptions { WriteIndented = true },
                cancellationToken);

            return setting;
        }
        finally
        {
            _lock.Release();
        }
    }
}
