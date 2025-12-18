using golden_fork.core.Entities.AppCart;
using golden_fork.core.Entities.AppUser;
using golden_fork.core.Entities.Kitchen;
using golden_fork.core.Entities.Menu;
using golden_fork.core.Entities.Purchase;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace golden_fork.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // 👉 Add ALL DbSets here
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserRole> UserRoles { get; set; }
        public virtual DbSet<Token> Tokens { get; set; }

        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Item> Items { get; set; }

        public virtual DbSet<Menu> Menus { get; set; }
        public virtual DbSet<MenuItem> MenuItems { get; set; }

        public virtual DbSet<Cart> Carts { get; set; }
        public virtual DbSet<CartItem> CartItems { get; set; }

        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderItem> OrderItems { get; set; }

        public virtual DbSet<Payment> Payments { get; set; }


        // Gestion tes relations entre les entites
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ============================
            // USER → ROLE  (Many-to-One)
            // ============================
            modelBuilder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============================
            // USER → TOKENS (One-to-Many)
            // ============================
            modelBuilder.Entity<Token>()
                .HasOne(t => t.User)
                .WithMany(u => u.Tokens)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ============================
            // CATEGORY → ITEMS (One-to-Many)
            // ============================
            modelBuilder.Entity<Item>()
                .HasOne(i => i.Category)
                .WithMany(c => c.Items)
                .HasForeignKey(i => i.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============================
            // ItemImage → ITEMS (Many-to-One)
            // ============================
            modelBuilder.Entity<ItemImage>()
                .HasOne(II => II.Item)
                .WithMany(i => i.ItemImages)
                .HasForeignKey(II => II.ItemId)
                .OnDelete(DeleteBehavior.Cascade);
            // ============================
            // MENU ↔ ITEM (Many-to-Many)
            // ============================
            modelBuilder.Entity<MenuItem>()
                .HasKey(mi => new { mi.MenuId, mi.ItemId });

            modelBuilder.Entity<MenuItem>()
                .HasOne(mi => mi.Menu)
                .WithMany(m => m.MenuItems)
                .HasForeignKey(mi => mi.MenuId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MenuItem>()
                .HasOne(mi => mi.Item)
                .WithMany(i => i.MenuItems)
                .HasForeignKey(mi => mi.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            // ============================
            // USER → CART (One-to-One)
            // ============================
            modelBuilder.Entity<User>()
                .HasOne(u => u.Cart)
                .WithOne(c => c.User)
                .HasForeignKey<Cart>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ============================
            // CART → CART ITEMS (One-to-Many)
            // ============================
            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.Item)
                .WithMany()
                .HasForeignKey(ci => ci.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============================
            // ORDER → ORDER ITEMS (One-to-Many)
            // ============================
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Item)
                .WithMany()
                .HasForeignKey(oi => oi.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            // ============================
            // ORDER → PAYMENT (One-to-One)
            // ============================
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Payment)
                .WithOne(p => p.Order)
                .HasForeignKey<Payment>(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRole>().ToTable("UserRoles");
            modelBuilder.Entity<Category>().ToTable("Categories");
            modelBuilder.Entity<Menu>().ToTable("Menus");
            modelBuilder.Entity<Item>().ToTable("Items");           
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Cart>().ToTable("Carts");
            modelBuilder.Entity<CartItem>().ToTable("CartItems");
            modelBuilder.Entity<Order>().ToTable("Orders");
            modelBuilder.Entity<OrderItem>().ToTable("OrderItems");
            modelBuilder.Entity<Payment>().ToTable("Payments");

            // ========================
            // STRING CONSTRAINTS (Required + MaxLength)
            // ========================
            modelBuilder.Entity<User>(e =>
            {
                e.Property(u => u.Username).IsRequired().HasMaxLength(50);
                e.Property(u => u.Email).IsRequired().HasMaxLength(100);
                e.Property(u => u.Password).IsRequired().HasMaxLength(255);
                e.Property(u => u.PhoneNumber).HasMaxLength(20);
                e.HasIndex(u => u.Email).IsUnique();
            });
            modelBuilder.Entity<UserRole>(e =>
            {
                e.Property(r => r.Name).IsRequired().HasMaxLength(30);
                e.HasIndex(r => r.Name).IsUnique();
            });

            modelBuilder.Entity<Category>(e =>
            {
                e.Property(c => c.Name).IsRequired().HasMaxLength(50);
                e.HasIndex(c => c.Name).IsUnique();
            });

            modelBuilder.Entity<Item>(e =>
            {
                e.Property(i => i.Name).IsRequired().HasMaxLength(100);
                e.Property(i => i.Description).HasMaxLength(500);
                e.Property(i => i.ImageUrl).HasMaxLength(500);
                e.Property(i => i.Price).IsRequired().HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<Menu>(e =>
            {
                e.Property(m => m.Name).IsRequired().HasMaxLength(50);
            });

            modelBuilder.Entity<Payment>(e =>
            {
                e.Property(p => p.Method).IsRequired().HasMaxLength(30); // "Card", "Cash", etc.
                e.Property(p => p.Status).IsRequired().HasMaxLength(20);        // "Pending", "Completed", "Failed"
                e.Property(p => p.Amount).IsRequired().HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<Order>(e =>
            {
                e.Property(o => o.TotalPrice).IsRequired().HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<OrderItem>(e =>
            {
                e.Property(oi => oi.UnitPrice).IsRequired().HasColumnType("decimal(18,2)");
                e.Property(oi => oi.Quantity).IsRequired();
            });

            // ========================
            // VALUE RANGE CONSTRAINTS (optional but nice)
            // ========================
            modelBuilder.Entity<Item>()
                .Property(i => i.Price)
                .HasColumnType("decimal(18,2)")
                .HasConversion<decimal>() // makes sure > 0
                .IsRequired();

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.Quantity)
                .IsRequired()
                .HasDefaultValue(1);

            // Prevent negative prices (EF Core 7+ way)
            modelBuilder.Entity<Item>()
                .Property(i => i.Price)
                .HasConversion<decimal>()
                .IsRequired()
                .HasAnnotation("MinValue", 0.01m);

            // ========================
            // DEFAULT VALUES (very useful)
            // ========================
            modelBuilder.Entity<Order>()
                .Property(o => o.OrderDate)
                .HasDefaultValueSql("GETUTCDATE()");

            modelBuilder.Entity<Payment>()
                .Property(p => p.Status)
                .HasDefaultValue("Pending");

            modelBuilder.Entity<Cart>()
                .Property(c => c.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // ========================
            // SEED DATA 
            // ========================

            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasData(
                    new UserRole { Id = 1, Name = "Admin" },
                    new UserRole { Id = 2, Name = "Customer" },
                    new UserRole { Id = 3, Name = "Kitchen" },
                    new UserRole { Id = 4, Name = "Delivery" }
                );
            });

            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasData(
                    new Category { Id = 1, Name = "Appetizers" },
                    new Category { Id = 2, Name = "Main Courses" },
                    new Category { Id = 3, Name = "Desserts" },
                    new Category { Id = 4, Name = "Beverages" }
                );
            });

            modelBuilder.Entity<Menu>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasData(
                    new Menu { Id = 1, Name = "Lunch Menu" },
                    new Menu { Id = 2, Name = "Dinner Menu" }
                );
            });

            modelBuilder.Entity<Item>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasData(
                    new Item { Id = 1, CategoryId = 1, Name = "Bruschetta", Price = 8.99m, Description = "Tomato & basil", ImageUrl = "/img/bruschetta.jpg" },
                    new Item { Id = 2, CategoryId = 2, Name = "Grilled Salmon", Price = 24.99m, Description = "With lemon butter", ImageUrl = "/img/salmon.jpg" },
                    new Item { Id = 3, CategoryId = 3, Name = "Tiramisu", Price = 7.99m, Description = "Classic Italian", ImageUrl = "/img/tiramisu.jpg" }
                );
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasData(
                    new User
                    {
                        Id = 1,
                        Username = "admin",
                        Email = "admin@goldenfork.com",
                        Password = "$2a$11$j3K8vP9mN5xL2rT6yU0iOcVfGaNd2z3fZ8k9pL5mN2xR7vQ1w/.kJtH",
                        RoleId = 1,
                        PhoneNumber = "555-0001"
                    }
                );
            });


        }

    }
}
