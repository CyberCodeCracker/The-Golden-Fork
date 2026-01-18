using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using golden_fork.Infrastructure.Data;
using golden_fork.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace golden_fork.Tests.Infrastructure.Repositories
{
    public class MenuRepositoryTests : IDisposable
    {
        private readonly AppDbContext _dbContext;
        private readonly MenuRepository _repository;

        public MenuRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_Menu_{Guid.NewGuid()}")
                .Options;

            _dbContext = new AppDbContext(options);
            _repository = new MenuRepository(_dbContext);

            SeedData();
        }

        private void SeedData()
        {
            // Créer des données de test
            var menu = new golden_fork.core.Entities.Kitchen.Menu
            {
                Id = 1,
                Name = "Test Menu",
                Description = "Test Description"
            };

            _dbContext.Menus.Add(menu);
            _dbContext.SaveChanges();
        }

        [Fact]
        public async Task MenuRepository_ShouldInheritFromGenericRepository()
        {
            // Arrange & Act
            var menu = await _repository.GetByIdAsync(1);

            // Assert
            menu.Should().NotBeNull();
            menu.Name.Should().Be("Test Menu");
        }

        [Fact]
        public async Task MenuRepository_CanUseAllGenericMethods()
        {
            // Test que toutes les méthodes du GenericRepository fonctionnent
            var newMenu = new golden_fork.core.Entities.Kitchen.Menu
            {
                Name = "New Menu",
                Description = "New Description"
            };

            // AddAsync
            await _repository.AddAsync(newMenu);
            await _dbContext.SaveChangesAsync();

            // GetAllAsync
            var allMenus = await _repository.GetAllAsync();
            allMenus.Should().HaveCount(2);

            // GetAsync avec prédicat
            var foundMenu = await _repository.GetAsync(m => m.Name == "New Menu");
            foundMenu.Should().NotBeNull();

            // UpdateAsync
            foundMenu.Description = "Updated Description";
            await _repository.UpdateAsync(foundMenu);
            await _dbContext.SaveChangesAsync();

            // DeleteByIdAsync
            await _repository.DeleteByIdAsync(foundMenu.Id);
            await _dbContext.SaveChangesAsync();

            var remainingMenus = await _repository.GetAllAsync();
            remainingMenus.Should().HaveCount(1);
        }

        [Fact]
        public async Task MenuRepository_GetAllAsyncWithFilter_WorksForMenu()
        {
            // Arrange - Ajouter plus de menus
            var menus = new List<golden_fork.core.Entities.Kitchen.Menu>
            {
                new() { Name = "Breakfast Menu", Description = "Morning meals" },
                new() { Name = "Lunch Menu", Description = "Midday meals" },
                new() { Name = "Dinner Menu", Description = "Evening meals" }
            };

            await _repository.AddListAsync(menus);
            await _dbContext.SaveChangesAsync();

            // Act - Filtrer par nom contenant "Menu"
            var query = await _repository.GetAllAsyncWithFilter(
                filter: m => m.Name.Contains("Menu"));

            // Assert
            var result = await query.ToListAsync();
            result.Should().HaveCount(4); // 3 nouveaux + 1 existant
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
        }
    }
}