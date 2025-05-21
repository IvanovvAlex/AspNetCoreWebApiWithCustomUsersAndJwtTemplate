using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Template.Common.Requests.Auth;
using Template.Common.Responses.Auth;
using Template.Core.Exceptions;
using Template.Core.StaticClasses;
using Template.Data;
using Template.Data.Entities;
using Template.Data.Interfaces;
using Template.Domain.Interfaces;

namespace Template.Domain.Services;

public class AuthService(AppDbContext context, IUserRepository userRepository, IHttpContextAccessor httpContextAccessor) : IAuthService
{
    public async Task<RegisterUserResponse?> RegisterAsync(RegisterUserRequest request)
    {
        if (await userRepository.IsEmailAlreadyUsed(request.Email))
        {
            throw new AppException("Email is already in use.").SetStatusCode(409);
        }

        User user = new()
        {
            Email = request.Email,
            PasswordHash = "temporaryPasswordHash",
            Names = request.Names,
            Phone = request.Phone,
            Role = Roles.Admin
        };

        string hashedPassword = new PasswordHasher<User>()
            .HashPassword(user, request.Password);

        user.Email = request.Email;
        user.PasswordHash = hashedPassword;

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return new()
        {
            Id = user.Id,
        };
    }

    public async Task<TokenResponse?> LoginAsync(LoginUserRequest request)
    {
        User? user = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
        if (user is null)
        {
            return null;
        }
        if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password)
            == PasswordVerificationResult.Failed)
        {
            return null;
        }

        return await CreateTokenResponse(user);
    }

    public async Task<TokenResponse?> RefreshTokensAsync(RefreshTokenRequest request)
    {
        User? user = await ValidateRefreshTokenAsync(request.UserId, request.RefreshToken);
        if (user is null)
        {
            return null;
        }

        return await CreateTokenResponse(user);

    }

    public async Task<bool> LogoutAsync()
    {
        Guid currentUserId = Guid.Parse(await GetCurrentUserId());
        User? user = await context.Users.FirstOrDefaultAsync(u => u.Id == currentUserId) ?? throw new AppException("User not found.").SetStatusCode(404);

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        await context.SaveChangesAsync();

        return true;
    }

    private async Task<User?> ValidateRefreshTokenAsync(Guid userId, string refreshToken)
    {
        User? user = await context.Users.FindAsync(userId);
        if (user is null || user.RefreshToken != refreshToken
                         || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            return null;
        }

        return user;
    }

    private async Task<TokenResponse> CreateTokenResponse(User? user)
    {
        return new()
        {
            AccessToken = CreateToken(user),
            RefreshToken = await GenerateAndSaveRefreshTokenAsync(user)
        };
    }

    private string GenerateRefreshToken()
    {
        byte[] randomNumber = new byte[32];
        using RandomNumberGenerator rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private async Task<string> GenerateAndSaveRefreshTokenAsync(User user)
    {
        string refreshToken = GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(int.Parse(Environment.GetEnvironmentVariable("REFRESH_TOKEN_EXPIRY_DAYS")!));
        await context.SaveChangesAsync();
        return refreshToken;
    }

    private string CreateToken(User user)
    {
        List<Claim> claims =
        [
            new(ClaimTypes.Name, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Role, user.Role.ToString())
        ];

        SymmetricSecurityKey key = new(
            Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT_TOKEN_SECRET")!));

        SigningCredentials creds = new(key, SecurityAlgorithms.HmacSha512);

        JwtSecurityToken tokenDescriptor = new(
            issuer: Environment.GetEnvironmentVariable("JWT_ISSUER"),
            audience: Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(Environment.GetEnvironmentVariable("JWT_TOKEN_EXPIRY_MINUTES")!)),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }

    public async Task<string?> GetCurrentUserId()
    {
        return await GetClaimValue(ClaimTypes.NameIdentifier);
    }

    public async Task<string?> GetCurrentUserEmail()
    {
        return await GetClaimValue(ClaimTypes.Name);
    }

    public async Task<string?> GetCurrentUserRole()
    {
        return await GetClaimValue(ClaimTypes.Role);
    }
    private async Task<string?> GetClaimValue(string claimType)
    {
        return httpContextAccessor.HttpContext?.User.FindFirst(claimType)?.Value;
    }
}
