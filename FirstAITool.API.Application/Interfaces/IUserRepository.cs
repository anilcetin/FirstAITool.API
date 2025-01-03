using FirstAITool.API.Domain.Entities;

namespace FirstAITool.API.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(int id);
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<bool> UsernameExistsAsync(string username);
        Task<bool> EmailExistsAsync(string email);
        Task<User> CreateAsync(User user);
        Task UpdateLastLoginAsync(int userId);
        Task SavePasswordResetTokenAsync(int userId, string token, DateTime expiryDate);
        Task<PasswordResetToken?> GetPasswordResetTokenAsync(int userId);
        Task UpdatePasswordAsync(int userId, byte[] passwordHash, byte[] passwordSalt);
        Task ClearPasswordResetTokenAsync(int userId);
        
        // Refresh token methods
        Task SaveRefreshTokenAsync(int userId, RefreshToken refreshToken);
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        Task UpdateRefreshTokenAsync(RefreshToken refreshToken);
    }
} 