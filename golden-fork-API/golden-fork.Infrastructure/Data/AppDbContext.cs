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
        }


    }


}
