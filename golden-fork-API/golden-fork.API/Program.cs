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
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Reflection;
using golden_fork.Core.IServices.Kitchen;
using golden_fork.Core.Services.Kitchen;

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
            builder.Services.AddAutoMapper(typeof(core.MappingProfiles.ProfileMapper).Assembly);

            builder.Services.AddScoped<IAppUserService, AppUserService>();
            builder.Services.AddScoped<IMenuService, MenuService>();

            builder.Services.infrastructureConfiguration(builder.Configuration);

            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Golden Fork API",
                    Version = "v1",
                    Description = "API for Golden Fork Restaurant Management"
                });

                // Add JWT Authentication definition to Swagger
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header using the Bearer scheme. 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      Example: 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
            });
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