using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace golden_fork.API
{
    public enum UserRole
    {
        Admin = 1,
        Chef = 2,
        Client = 3,
    }

    // Custom attribute
    public class AuthorizeRolesAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly UserRole[] _allowedRoles;

        public AuthorizeRolesAttribute(params UserRole[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Debug: Log the user claims
            var user = context.HttpContext.User;
            Console.WriteLine($"User authenticated: {user.Identity?.IsAuthenticated}");

            foreach (var claim in user.Claims)
            {
                Console.WriteLine($"Claim: {claim.Type} = {claim.Value}");
            }

            // Allow anonymous
            var allowAnonymous = context.ActionDescriptor.EndpointMetadata
                .Any(em => em.GetType() == typeof(AllowAnonymousAttribute));
            if (allowAnonymous) return;

            // Check authentication
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Get role ID
            var roleIdClaim = user.FindFirst("RoleId") ?? user.FindFirst(ClaimTypes.Role);
            Console.WriteLine($"RoleIdClaim found: {roleIdClaim?.Type} = {roleIdClaim?.Value}");

            if (roleIdClaim == null || !int.TryParse(roleIdClaim.Value, out var userRoleId))
            {
                Console.WriteLine("No valid RoleId claim found");
                context.Result = new ForbidResult();
                return;
            }

            var userRole = (UserRole)userRoleId;
            Console.WriteLine($"User role: {userRole} (ID: {userRoleId})");
            Console.WriteLine($"Allowed roles: {string.Join(", ", _allowedRoles)}");

            if (!_allowedRoles.Contains(userRole))
            {
                Console.WriteLine("Role not allowed");
                context.Result = new ForbidResult();
                return;
            }

            Console.WriteLine("Authorization successful");
        }
    }
}
