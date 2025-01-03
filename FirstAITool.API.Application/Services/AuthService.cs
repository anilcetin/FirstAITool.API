using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FirstAITool.API.Application.DTOs;
using FirstAITool.API.Application.Interfaces;
using FirstAITool.API.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace FirstAITool.API.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            
            if (user == null)
                throw new Exception("User not found");

            if (!VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
                throw new Exception("Invalid password");

            await _userRepository.UpdateLastLoginAsync(user.Id);

            var token = CreateToken(user.Username);
            var refreshToken = GenerateRefreshToken();
            
            // Save refresh token
            user.RefreshTokens.Add(refreshToken);
            await _userRepository.SaveRefreshTokenAsync(user.Id, refreshToken);
            
            return new LoginResponse
            {
                Username = user.Username,
                Token = token,
                RefreshToken = refreshToken.Token,
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            };
        }

        public async Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var refreshToken = await _userRepository.GetRefreshTokenAsync(request.RefreshToken);
            if (refreshToken == null)
                throw new Exception("Invalid refresh token");

            var user = await _userRepository.GetByIdAsync(refreshToken.UserId);
            if (user == null)
                throw new Exception("User not found");

            if (!refreshToken.IsActive)
                throw new Exception("Inactive refresh token");

            // Generate new tokens
            var newToken = CreateToken(user.Username);
            var newRefreshToken = GenerateRefreshToken();

            // Revoke the old refresh token
            refreshToken.RevokedAt = DateTime.UtcNow;
            refreshToken.ReplacedByToken = newRefreshToken.Token;
            refreshToken.RevokedReason = "Replaced by new token";

            // Save the new refresh token
            user.RefreshTokens.Add(newRefreshToken);
            await _userRepository.SaveRefreshTokenAsync(user.Id, newRefreshToken);

            return new RefreshTokenResponse
            {
                Token = newToken,
                RefreshToken = newRefreshToken.Token,
                ExpiresAt = DateTime.UtcNow.AddDays(1)
            };
        }

        public async Task<bool> RevokeTokenAsync(string token)
        {
            var refreshToken = await _userRepository.GetRefreshTokenAsync(token);
            if (refreshToken == null)
                return false;

            if (!refreshToken.IsActive)
                return false;

            // Revoke token
            refreshToken.RevokedAt = DateTime.UtcNow;
            refreshToken.RevokedReason = "Revoked by user";

            await _userRepository.UpdateRefreshTokenAsync(refreshToken);
            return true;
        }

        public RefreshToken GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            
            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomBytes),
                ExpiresAt = DateTime.UtcNow.AddDays(7), // Refresh tokens last 7 days
                CreatedAt = DateTime.UtcNow
            };
        }

        public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512(passwordSalt);
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            return computedHash.SequenceEqual(passwordHash);
        }

        public string CreateToken(string username)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration.GetSection("AppSettings:Token").Value!));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            if (await _userRepository.UsernameExistsAsync(request.Username))
                throw new Exception("Username already exists");

            if (await _userRepository.EmailExistsAsync(request.Email))
                throw new Exception("Email already exists");

            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.CreateAsync(user);

            var token = CreateToken(user.Username);

            return new RegisterResponse
            {
                Username = user.Username,
                Email = user.Email,
                Token = token
            };
        }

        public async Task ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
                return; // Don't reveal if email exists

            var resetToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var tokenExpiry = DateTime.UtcNow.AddHours(1);

            await _userRepository.SavePasswordResetTokenAsync(user.Id, resetToken, tokenExpiry);

            // TODO: Send email with reset token
            // In a real application, you would integrate with an email service here
        }

        public async Task ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
                throw new Exception("Invalid reset request");

            var resetToken = await _userRepository.GetPasswordResetTokenAsync(user.Id);
            if (resetToken == null || resetToken.Token != request.ResetToken || resetToken.ExpiresAt < DateTime.UtcNow)
                throw new Exception("Invalid or expired reset token");

            CreatePasswordHash(request.NewPassword, out byte[] passwordHash, out byte[] passwordSalt);
            await _userRepository.UpdatePasswordAsync(user.Id, passwordHash, passwordSalt);
            await _userRepository.ClearPasswordResetTokenAsync(user.Id);
        }
    }
} 