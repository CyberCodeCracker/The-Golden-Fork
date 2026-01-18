using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using golden_fork.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace golden_fork.Tests.Infrastructure.Integration
{
    public class EntityRelationshipsTests : IDisposable
    {
        private readonly AppDbContext _dbContext;

        public EntityRelationshipsTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_Relations_{Guid.NewGuid()}")
                .Options;

            _dbContext = new AppDbContext(options);
            SeedTestData();
        }

        private void SeedTestData()
        {
            // Créer un scénario complet avec relations
            var user = new golden_fork.core.Entities.AppUser.User
            {
                Username = "testuser",
                Email = "test@test.com",
                Password = "password",
                Role = new golden_fork.core.Entities.AppUser.UserRole
                {
                    Name = "Customer"
                },
                Cart = new golden_fork.core.Entities.AppCart.Cart()
            };

            var category = new golden_fork.core.Entities.Menu.Category
            {
                Name = "Main Course",
                Description = "Main dishes"
            };

            // Dans EntityRelationshipsTests.cs, ligne ~81
            var item = new golden_fork.core.Entities.Menu.Item
            {
                Name = "Burger",
                Description = "Delicious burger",
                Price = 9.99m,
                ImageUrl = "/images/burger.jpg", 
                Category = category
            };

            // Créer une commande complète
            var order = new golden_fork.core.Entities.Purchase.Order
            {
                User = user,
                TotalPrice = 19.98m,
                OrderItems = new List<golden_fork.core.Entities.Purchase.OrderItem>
                {
                    new()
                    {
                        Item = item,
                        Quantity = 2,
                        UnitPrice = 9.99m
                    }
                },
                Payment = new golden_fork.core.Entities.Purchase.Payment
                {
                    Amount = 19.98m,
                    Method = "Credit Card",
                    Status = "Completed"
                }
            };

            _dbContext.Users.Add(user);
            _dbContext.Categories.Add(category);
            _dbContext.Items.Add(item);
            _dbContext.Orders.Add(order);

            _dbContext.SaveChanges();
        }

        [Fact]
        public async Task User_Cart_Relationship_ShouldWork()
        {
            // Act
            var user = await _dbContext.Users
                .Include(u => u.Cart)
                .FirstOrDefaultAsync(u => u.Username == "testuser");

            // Assert
            user.Should().NotBeNull();
            user.Cart.Should().NotBeNull();
            user.Cart.UserId.Should().Be(user.Id);
        }

        [Fact]
        public async Task Order_User_Relationship_ShouldWork()
        {
            // Act
            var order = await _dbContext.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync();

            // Assert
            order.Should().NotBeNull();
            order.User.Should().NotBeNull();
            order.User.Username.Should().Be("testuser");
        }

        [Fact]
        public async Task Order_OrderItems_Relationship_ShouldWork()
        {
            // Act
            var order = await _dbContext.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Item)
                .FirstOrDefaultAsync();

            // Assert
            order.Should().NotBeNull();
            order.OrderItems.Should().HaveCount(1);
            order.OrderItems.First().Item.Name.Should().Be("Burger");
            order.OrderItems.First().TotalPrice.Should().Be(19.98m);
        }

        [Fact]
        public async Task Item_Category_Relationship_ShouldWork()
        {
            // Act
            var item = await _dbContext.Items
                .Include(i => i.Category)
                .FirstOrDefaultAsync(i => i.Name == "Burger");

            // Assert
            item.Should().NotBeNull();
            item.Category.Should().NotBeNull();
            item.Category.Name.Should().Be("Main Course");
        }

        [Fact]
        public async Task User_Role_Relationship_ShouldWork()
        {
            // Act
            var user = await _dbContext.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == "testuser");

            // Assert
            user.Should().NotBeNull();
            user.Role.Should().NotBeNull();
            user.Role.Name.Should().Be("Customer");
        }

        [Fact]
        public async Task Order_Payment_Relationship_ShouldWork()
        {
            // Act
            var order = await _dbContext.Orders
                .Include(o => o.Payment)
                .FirstOrDefaultAsync();

            // Assert
            order.Should().NotBeNull();
            order.Payment.Should().NotBeNull();
            order.Payment.Status.Should().Be("Completed");
        }

        [Fact]
        public async Task CascadingDelete_UserDeleted_ShouldDeleteRelatedEntities()
        {
            // Arrange
            var user = await _dbContext.Users
                .Include(u => u.Cart)
                .Include(u => u.Tokens)
                .FirstOrDefaultAsync(u => u.Username == "testuser");

            // Act
            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();

            // Assert
            var deletedUser = await _dbContext.Users.FindAsync(user.Id);
            deletedUser.Should().BeNull();

            // Vérifier que les entités liées avec cascade sont supprimées
            var userCart = await _dbContext.Carts.FindAsync(user.Cart.Id);
            userCart.Should().BeNull();
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
        }
    }
}