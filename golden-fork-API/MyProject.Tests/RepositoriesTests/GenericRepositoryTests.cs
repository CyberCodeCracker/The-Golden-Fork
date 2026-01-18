using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FluentAssertions;
using golden_fork.core.Entities.AppUser;
using golden_fork.Infrastructure.Data;
using golden_fork.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace golden_fork.Tests.Infrastructure.Repositories
{
    public class GenericRepositoryTests : IDisposable
    {
        private readonly AppDbContext _dbContext;
        private readonly GenericRepository<User> _repository;
        private readonly Mock<AppDbContext> _mockContext;
        private readonly Mock<DbSet<User>> _mockSet;

        public GenericRepositoryTests()
        {
            // Configuration de la base InMemory
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
                .Options;

            _dbContext = new AppDbContext(options);
            _repository = new GenericRepository<User>(_dbContext);

            // Setup pour les tests avec Moq
            _mockContext = new Mock<AppDbContext>();
            _mockSet = new Mock<DbSet<User>>();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllEntities()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 1, Username = "user1", Email = "user1@test.com", Password = "pass1" },
                new User { Id = 2, Username = "user2", Email = "user2@test.com", Password = "pass2" }
            };

            await _dbContext.Users.AddRangeAsync(users);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(u => u.Username == "user1");
            result.Should().Contain(u => u.Username == "user2");
        }

        [Fact]
        public async Task GetByIdAsync_WithValidId_ShouldReturnEntity()
        {
            // Arrange
            var user = new User { Id = 1, Username = "test", Email = "test@test.com", Password = "pass" };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetByIdAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.Username.Should().Be("test");
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
        {
            // Act
            var result = await _repository.GetByIdAsync(999);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetAsync_WithPredicate_ShouldReturnMatchingEntity()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 1, Username = "john", Email = "john@test.com", Password = "pass" },
                new User { Id = 2, Username = "jane", Email = "jane@test.com", Password = "pass" }
            };

            await _dbContext.Users.AddRangeAsync(users);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetAsync(u => u.Username == "jane");

            // Assert
            result.Should().NotBeNull();
            result.Username.Should().Be("jane");
        }

        [Fact]
        public async Task AddAsync_ShouldAddEntityToDatabase()
        {
            // Arrange
            var user = new User { Username = "newuser", Email = "new@test.com", Password = "pass" };

            // Act
            await _repository.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            // Assert
            var userInDb = await _dbContext.Users.FindAsync(user.Id);
            userInDb.Should().NotBeNull();
            userInDb.Username.Should().Be("newuser");
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateEntity()
        {
            // Arrange
            var user = new User { Id = 1, Username = "oldname", Email = "old@test.com", Password = "pass" };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            // Détacher l'entité
            _dbContext.Entry(user).State = EntityState.Detached;

            // Act
            user.Username = "newname";
            await _repository.UpdateAsync(user);
            await _dbContext.SaveChangesAsync();

            // Assert
            var updatedUser = await _dbContext.Users.FindAsync(1);
            updatedUser.Username.Should().Be("newname");
        }

        [Fact]
        public async Task DeleteByIdAsync_ShouldRemoveEntity()
        {
            // Arrange
            var user = new User { Id = 1, Username = "todelete", Email = "delete@test.com", Password = "pass" };
            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            // Act
            await _repository.DeleteByIdAsync(1);
            await _dbContext.SaveChangesAsync();

            // Assert
            var deletedUser = await _dbContext.Users.FindAsync(1);
            deletedUser.Should().BeNull();
        }

        [Fact]
        public async Task DeleteByIdAsync_WithNonExistentId_ShouldThrowException()
        {
            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _repository.DeleteByIdAsync(999));
        }

        [Fact]
        public async Task GetAllAsyncWithFilter_WithFilter_ShouldReturnFilteredResults()
        {
            // Arrange
            var users = new List<User>
    {
        new User { Id = 1, Username = "activeuser", Email = "active@test.com", Password = "pass" },
        new User { Id = 2, Username = "inactiveuser", Email = "inactive@test.com", Password = "pass" },
        new User { Id = 3, Username = "john", Email = "john@test.com", Password = "pass" } // Ajoute un user sans "active"
    };

            await _dbContext.Users.AddRangeAsync(users);
            await _dbContext.SaveChangesAsync();

            // Act - Utilise StartsWith au lieu de Contains
            var result = await _repository.GetAllAsyncWithFilter(
                filter: u => u.Username.StartsWith("active")); // <-- CORRECTION ICI

            // Assert
            var filteredList = await result.ToListAsync();
            filteredList.Should().HaveCount(1);
            filteredList.First().Username.Should().Be("activeuser");
        }

        [Fact]
        public async Task GetAllAsyncWithFilter_WithOrderBy_ShouldReturnOrderedResults()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 1, Username = "zebra", Email = "z@test.com", Password = "pass" },
                new User { Id = 2, Username = "apple", Email = "a@test.com", Password = "pass" },
                new User { Id = 3, Username = "banana", Email = "b@test.com", Password = "pass" }
            };

            await _dbContext.Users.AddRangeAsync(users);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsyncWithFilter(
                orderBy: q => q.OrderBy(u => u.Username));

            // Assert
            var orderedList = await result.ToListAsync();
            orderedList[0].Username.Should().Be("apple");
            orderedList[1].Username.Should().Be("banana");
            orderedList[2].Username.Should().Be("zebra");
        }

        [Fact]
        public async Task GetFirstOrDefaultAsync_WithInclude_ShouldReturnEntityWithRelatedData()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "test",
                Email = "test@test.com",
                Password = "pass",
                Cart = new core.Entities.AppCart.Cart { Id = 1, UserId = 1 }
            };

            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetFirstOrDefaultAsync(
                predicate: u => u.Id == 1,
                include: q => q.Include(u => u.Cart));

            // Assert
            result.Should().NotBeNull();
            result.Cart.Should().NotBeNull();
            result.Cart.UserId.Should().Be(1);
        }

        [Fact]
        public async Task CreateAndLogAsync_ShouldReturnLogMessage()
        {
            // Arrange
            var user = new User
            {
                Username = "newuser",
                Email = "new@test.com",
                Password = "password123"
            };

            // Act
            var logMessage = await _repository.CreateAndLogAsync(user);

            // Assert
            logMessage.Should().Contain("Created User");
            logMessage.Should().Contain("Username=newuser");
            logMessage.Should().Contain("Email=new@test.com");
            logMessage.Should().NotContain("ConfirmPassword"); // NotMapped property
        }

        [Fact]
        public async Task UpdateGeneral_ShouldUpdateSpecifiedFields()
        {
            // Arrange
            var user = new User
            {
                Id = 1,
                Username = "oldname",
                Email = "old@test.com",
                Password = "pass",
                PhoneNumber = "1234567890"
            };

            await _dbContext.Users.AddAsync(user);
            await _dbContext.SaveChangesAsync();

            var updatedUser = new User
            {
                Username = "newname",
                Email = "new@test.com",
                PhoneNumber = null // On ne change pas ce champ
            };

            // Act
            var changes = await _repository.UpdateGeneral(user, updatedUser,
                new List<string> { "Username", "Email" });

            // Assert
            changes.Should().Contain("Changed Username");
            changes.Should().Contain("Changed Email");
            changes.Should().NotContain("PhoneNumber");

            var userInDb = await _dbContext.Users.FindAsync(1);
            userInDb.Username.Should().Be("newname");
            userInDb.Email.Should().Be("new@test.com");
            userInDb.PhoneNumber.Should().Be("1234567890"); // Inchangé
        }

        [Fact]
        public async Task GetByLastAsync_ShouldReturnLastEntityByStringProperty()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 1, Username = "aaa", Email = "a@test.com", Password = "pass" },
                new User { Id = 2, Username = "bbb", Email = "b@test.com", Password = "pass" },
                new User { Id = 3, Username = "ccc", Email = "c@test.com", Password = "pass" }
            };

            await _dbContext.Users.AddRangeAsync(users);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _repository.GetBylastAsync(u => u.Username);

            // Assert
            result.Should().NotBeNull();
            result.Username.Should().Be("ccc"); // Dernier par ordre alphabétique
        }

        [Fact]
        public async Task AddListAsync_ShouldAddMultipleEntities()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Username = "user1", Email = "u1@test.com", Password = "pass" },
                new User { Username = "user2", Email = "u2@test.com", Password = "pass" },
                new User { Username = "user3", Email = "u3@test.com", Password = "pass" }
            };

            // Act
            await _repository.AddListAsync(users);
            await _dbContext.SaveChangesAsync();

            // Assert
            var usersInDb = await _dbContext.Users.ToListAsync();
            usersInDb.Should().HaveCount(3);
        }

        [Fact]
        public async Task DeleteListAsync_ShouldRemoveMultipleEntities()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = 1, Username = "user1", Email = "u1@test.com", Password = "pass" },
                new User { Id = 2, Username = "user2", Email = "u2@test.com", Password = "pass" },
                new User { Id = 3, Username = "user3", Email = "u3@test.com", Password = "pass" }
            };

            await _dbContext.Users.AddRangeAsync(users);
            await _dbContext.SaveChangesAsync();

            // Act
            await _repository.DeleteListAsync(users);
            await _dbContext.SaveChangesAsync();

            // Assert
            var remainingUsers = await _dbContext.Users.ToListAsync();
            remainingUsers.Should().BeEmpty();
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
        }
    }
}