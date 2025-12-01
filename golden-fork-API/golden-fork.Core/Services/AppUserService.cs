using golden_fork.core.DTOs;
using golden_fork.core.Entities.AppUser;
using golden_fork.Core.IServices;
using golden_fork.Infrastructure.IRepositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

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
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                Environment.GetEnvironmentVariable("GOLDENFORK_JWT_SECRET")
                ?? throw new InvalidOperationException("JWT secret missing")));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"] ?? "GoldenForkAPI",
                audience: _config["Jwt:Audience"] ?? "GoldenForkClients",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public Task<(bool success, string message, User? user)> LoginAsync(LoginRequest dto)
        {
            throw new NotImplementedException();
        }

        public async Task<(bool success, string message, int? userId)> RegisterAsync(RegistrationRequest entity)
        {
            // Check if user already exists
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));    
            }
            if (string.IsNullOrWhiteSpace(entity.Username))
                throw new ArgumentException("Username est requis.", nameof(entity.Username));
            if (string.IsNullOrWhiteSpace(entity.Email))
                throw new ArgumentException("Mail est requis.", nameof(entity.Email));
            if (string.IsNullOrWhiteSpace(entity.Password))
                throw new ArgumentException("Mot de passe est requis.", nameof(entity.Password));
            if (string.IsNullOrWhiteSpace(entity.PhoneNumber))
                throw new ArgumentException("Téléphone est requis.", nameof(entity.PhoneNumber));

            if (await )
        }
    }
}
