using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tracking.Security.Password;
using Tracking.Storage.Data;

namespace Tracking.API.Auth;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public sealed class AuthController : ControllerBase
{
    private readonly TrackingDbContext _db;
    private readonly PasswordHasher _passwordHasher;
    private readonly JwtTokenService _jwtTokenService;

    public AuthController(
        TrackingDbContext db,
        PasswordHasher passwordHasher,
        JwtTokenService jwtTokenService)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _db.Users
            .SingleOrDefaultAsync(
                x => x.Username == request.Username,
                cancellationToken);

        if (user is null ||
            !user.IsActive ||
            !_passwordHasher.Verify(
                request.Password,
                user.PasswordHash))
        {
            return Unauthorized(new
            {
                message = "Invalid username or password."
            });
        }

        var token = _jwtTokenService.CreateToken(user);

        return Ok(new
        {
            accessToken = token,
            tokenType = "Bearer",
            expiresIn = 3600,
            user = new
            {
                id = user.Id,
                username = user.Username,
                displayName = user.DisplayName,
                role = user.Role
            }
        });
    }
}

public sealed record LoginRequest(
    string Username,
    string Password);
