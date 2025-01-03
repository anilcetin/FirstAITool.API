using FirstAITool.API.Application.Interfaces;
using FirstAITool.API.Domain.Entities;
using FirstAITool.API.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FirstAITool.API.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _context.Users
                .AnyAsync(u => u.Username == username);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users
                .AnyAsync(u => u.Email == email);
        }

        public async Task<User> CreateAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task UpdateLastLoginAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task SavePasswordResetTokenAsync(int userId, string token, DateTime expiryDate)
        {
            var existingToken = await _context.PasswordResetTokens
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (existingToken != null)
            {
                existingToken.Token = token;
                existingToken.ExpiresAt = expiryDate;
                existingToken.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                await _context.PasswordResetTokens.AddAsync(new PasswordResetToken
                {
                    UserId = userId,
                    Token = token,
                    ExpiresAt = expiryDate,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task<PasswordResetToken?> GetPasswordResetTokenAsync(int userId)
        {
            return await _context.PasswordResetTokens
                .FirstOrDefaultAsync(t => t.UserId == userId);
        }

        public async Task UpdatePasswordAsync(int userId, byte[] passwordHash, byte[] passwordSalt)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
                await _context.SaveChangesAsync();
            }
        }

        public async Task ClearPasswordResetTokenAsync(int userId)
        {
            var token = await _context.PasswordResetTokens
                .FirstOrDefaultAsync(t => t.UserId == userId);

            if (token != null)
            {
                _context.PasswordResetTokens.Remove(token);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SaveRefreshTokenAsync(int userId, RefreshToken refreshToken)
        {
            refreshToken.UserId = userId;
            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Token == token);
        }

        public async Task UpdateRefreshTokenAsync(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Update(refreshToken);
            await _context.SaveChangesAsync();
        }
    }
} 