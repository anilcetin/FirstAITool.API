using FirstAITool.API.Domain.Entities;

namespace FirstAITool.API.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<bool> UsernameExistsAsync(string username);
        Task<User> CreateAsync(User user);
        Task UpdateLastLoginAsync(int userId);
        Task<User?> GetByEmailAsync(string email);
    }
} 