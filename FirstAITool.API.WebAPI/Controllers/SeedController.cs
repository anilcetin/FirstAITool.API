using FirstAITool.API.Application.Interfaces;
using FirstAITool.API.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace FirstAITool.API.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeedController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserRepository _userRepository;

        public SeedController(IAuthService authService, IUserRepository userRepository)
        {
            _authService = authService;
            _userRepository = userRepository;
        }

        [HttpPost("create-admin")]
        public async Task<IActionResult> CreateAdminUser()
        {
            if (await _userRepository.UsernameExistsAsync("admin"))
            {
                return BadRequest("Admin user already exists");
            }

            _authService.CreatePasswordHash("Admin123!", out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User
            {
                Username = "admin",
                Email = "admin@admin.com",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.CreateAsync(user);

            return Ok("Admin user created successfully. Password: Admin123!");
        }
    }
} 