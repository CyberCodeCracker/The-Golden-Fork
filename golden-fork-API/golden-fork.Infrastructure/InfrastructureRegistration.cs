using golden_fork.Infrastructure.Data;
using golden_fork.Infrastructure.IRepositorie;
using golden_fork.Infrastructure.IRepositories;
using golden_fork.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.Infrastructure
{
    public static class InfrastructureRegistration
    {
        // Transient pas d'enregistrement a la BD. Comme envoi email confirmation
        // Singleton un seul service pour toute l'application. Comme le service de cache en mémoire
        // Scoped une fois par requête HTTP.
        public static IServiceCollection infrastructureConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            // Appliquez Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            // Appliquez DbContext
            services.AddDbContext<AppDbContext>(op =>
            {
                op.UseSqlServer(configuration.GetConnectionString("GoldenForkDatabase"));
            });
            return services;
        }   
    }
}
