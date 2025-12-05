using golden_fork.core.Entities.AppUser;
using golden_fork.Core.IServices;
using golden_fork.Core.Services;
using golden_fork.Infrastructure;
using golden_fork.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace golden_fork.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ========================
            // 1. Services
            // ========================
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddAutoMapper(typeof(core.MappingProfiles.ProfileMapper).Assembly);

            builder.Services.AddScoped<IAppUserService, AppUserService>();

            builder.Services.infrastructureConfiguration(builder.Configuration);

            // DbContext
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")
                    ?? "Server=(localdb)\\mssqllocaldb;Database=GoldenForkDb;Trusted_Connection=true;MultipleActiveResultSets=true"));

            // ========================
            // 2. JWT — SECRET FROM ENV ONLY (NEVER IN JSON)
            // ========================

            var jwtSecret = builder.Configuration["JWT:Key"];

            if (string.IsNullOrWhiteSpace(jwtSecret))
            {
                throw new InvalidOperationException(
                    "ERROR: GOLDENFORK_JWT_SECRET environment variable is not set!\n" +
                    "Run this in your terminal before starting the API:\n\n" +
                    "    $env:GOLDENFORK_JWT_SECRET = \"your-very-long-random-key-here==\"\n\n" +
                    "Or set it permanently in Windows → Environment Variables.");
            }

            var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "GoldenForkAPI";
            var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "GoldenForkClients";

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };

                // Read JWT from HttpOnly cookie (not from header
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Cookies["GoldenForkAuth"];
                        if (!string.IsNullOrEmpty(token))
                        {
                            context.Token = token;
                        }
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddAuthorization();

            // ========================
            // 3. App pipeline
            // ========================
            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();  // before UseAuthorization
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}