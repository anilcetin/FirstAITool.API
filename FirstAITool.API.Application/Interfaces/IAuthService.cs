using FirstAITool.API.Application.DTOs;
using FirstAITool.API.Domain.Entities;

namespace FirstAITool.API.Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<RegisterResponse> RegisterAsync(RegisterRequest request);
        Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request);
        Task<bool> RevokeTokenAsync(string token);
        Task ForgotPasswordAsync(ForgotPasswordRequest request);
        Task ResetPasswordAsync(ResetPasswordRequest request);
        void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt);
        bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt);
        string CreateToken(string username);
        RefreshToken GenerateRefreshToken();
    }
} 