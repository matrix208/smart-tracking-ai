using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Tracking.API.Settings;

namespace Tracking.API.Controllers;

[ApiController]
[Route("api/settings")]
[Authorize(Roles = "Administrator")]
public sealed class SettingsController : ControllerBase
{
    private const long MaxFileSizeBytes = 25 * 1024 * 1024; // 25 MB

    private static readonly string[] AllowedImageExtensions =
        { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

    private static readonly string[] AllowedVideoExtensions =
        { ".mp4", ".webm", ".mov" };

    private readonly ILoginMediaSettingsService _settingsService;
    private readonly IWebHostEnvironment _env;

    public SettingsController(
        ILoginMediaSettingsService settingsService,
        IWebHostEnvironment env)
    {
        _settingsService = settingsService;
        _env = env;
    }

    [HttpGet("login-media")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLoginMedia(CancellationToken cancellationToken)
    {
        var setting = await _settingsService.GetAsync(cancellationToken);
        return Ok(new { type = setting.Type, url = setting.Url });
    }

    [HttpPost("login-media/url")]
    public async Task<IActionResult> SetLoginMediaUrl(
        SetLoginMediaUrlRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Url) ||
            !Uri.TryCreate(request.Url, UriKind.Absolute, out _))
        {
            return BadRequest(new { message = "A valid absolute URL is required." });
        }

        if (request.Type != "image" && request.Type != "video")
        {
            return BadRequest(new { message = "Type must be 'image' or 'video'." });
        }

        var setting = await _settingsService.SetUrlAsync(
            request.Type, request.Url, cancellationToken);

        return Ok(new { type = setting.Type, url = setting.Url });
    }

    [HttpPost("login-media/upload")]
    [RequestSizeLimit(MaxFileSizeBytes)]
    public async Task<IActionResult> UploadLoginMedia(
        IFormFile? file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest(new { message = "No file was uploaded." });
        }

        if (file.Length > MaxFileSizeBytes)
        {
            return BadRequest(new { message = "File exceeds the 25 MB limit." });
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        string type;
        if (AllowedImageExtensions.Contains(extension))
        {
            type = "image";
        }
        else if (AllowedVideoExtensions.Contains(extension))
        {
            type = "video";
        }
        else
        {
            return BadRequest(new { message = "Unsupported file type." });
        }

        var uploadsRoot = Path.Combine(_env.ContentRootPath, "uploads", "login-media");
        Directory.CreateDirectory(uploadsRoot);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(uploadsRoot, fileName);

        await using (var stream = System.IO.File.Create(filePath))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var relativeUrl = $"/uploads/login-media/{fileName}";

        var setting = await _settingsService.SetUploadedFileAsync(
            type, relativeUrl, cancellationToken);

        return Ok(new { type = setting.Type, url = setting.Url });
    }

    [HttpDelete("login-media")]
    public async Task<IActionResult> ResetLoginMedia(CancellationToken cancellationToken)
    {
        await _settingsService.ResetAsync(cancellationToken);
        return NoContent();
    }
}

public sealed record SetLoginMediaUrlRequest(string Type, string Url);
