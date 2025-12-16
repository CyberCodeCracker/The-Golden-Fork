// golden_fork.Core/Services/AppUserService.cs
using golden_fork.core.DTOs;
using golden_fork.core.Entities.AppUser;
using golden_fork.Core.IServices;
using golden_fork.Infrastructure.IRepositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BCrypt.Net; 
using System.Security.Cryptography;
using System.Text;

namespace golden_fork.Core.Services
{
    public class AppUserService : IAppUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;

        public AppUserService(IUnitOfWork unitOfWork, IConfiguration config)
        {
            _unitOfWork = unitOfWork;
            _config = config;
        }

        public string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role?.Name ?? "Customer"),
                new("RoleId", user.RoleId.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                Environment.GetEnvironmentVariable("GOLDENFORK_JWT_SECRET")
                ?? throw new InvalidOperationException("JWT secret is missing")));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"] ?? "GoldenForkAPI",
                audience: _config["Jwt:Audience"] ?? "GoldenForkClients",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<(bool success, string message, User? user)> LoginAsync(LoginRequest dto)
        {
            if (dto == null)
                return (false, "Request cannot be null", null);

            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return (false, "Email and password are required", null);

            var user = await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(
                u => u.Email == dto.Email,
                include: q => q.Include(u => u.Role));

            if (user == null)
                return (false, "Invalid email", null);

            // Hash input password and compare with stored hashed password
            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
                return (false, "Invalid password", null);

            return (true, "Login successful", user);
        }

        public async Task<(bool success, string message, int? userId)> RegisterAsync(RegistrationRequest dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.Username))
                return (false, "Username is required", null);

            if (string.IsNullOrWhiteSpace(dto.Email))
                return (false, "Email is required", null);

            if (string.IsNullOrWhiteSpace(dto.Password))
                return (false, "Password is required", null);

            if (string.IsNullOrWhiteSpace(dto.PhoneNumber))
                return (false, "Phone number is required", null);

            if (dto.Password != dto.ConfirmPassword)
                return (false, "Passwords do not match", null);

            // Check duplicates
            var existing = await _unitOfWork.UserRepository.GetFirstOrDefaultAsync(
                u => u.Username == dto.Username || u.Email == dto.Email);

            if (existing != null)
            {
                if (existing.Email == dto.Email)
                    return (false, "Email already registered", null);
            }

            // Hash password with BCrypt (recommended over HMACSHA512 for passwords)
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                Password = hashedPassword,           // ← stored as BCrypt hash
                PhoneNumber = dto.PhoneNumber,
                RoleId = 2,            // 2 = Customer
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.UserRepository.AddAsync(user);
            await _unitOfWork.UserRepository.SaveChangesAsync();

            return (true, "User registered successfully", user.Id);
        }
    }
}