using golden_fork.core;
using golden_fork.core.DTOs;
using golden_fork.core.Entities.AppUser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.Core.IServices
{
    public interface IAppUserService
    {
        Task<(bool success, string message, int? userId)> RegisterAsync(RegistrationRequest dto);
        Task<(bool success, string message, User? user)> LoginAsync(LoginRequest dto);
        string GenerateJwtToken(User user);
        Task<int> GetUserCountAsync();
        Task<List<UserResponseDto>> GetAllUsersAsync();
        Task<(bool success, string message)> UpdateUserRoleAsync(string userId, string newRole);
        Task<(bool success, string message)> DeleteUserAsync(string userId);
    }

}

