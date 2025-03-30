using AppDomainEntityFramework.Entities;
using Microsoft.EntityFrameworkCore;
using System;

namespace AppDomainEntityFramework
{
    public class ApplicationDbContext : DbContext
    {
        public required DbSet<Product> Products { get; set; }
        public required DbSet<ProductReadOnly> ProductsReadOnly { get; set; }
        public required DbSet<UserAccount> UserAccounts { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured)
            {
                return;
            }

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserAccount>(entity =>
            {
                // Explicitly set table name
                entity.ToTable("UserAccounts");

                // Define primary key
                entity.HasKey(p => p.UserAccountId);

                entity.Property(p => p.UserAccountId)
                    .HasColumnType("guid")
                    .HasDefaultValueSql("(newid())")
                    .IsRequired();

                entity.Property(p => p.FirstName)
                    .HasColumnType("varchar")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(p => p.LastName)
                    .HasColumnType("varchar")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(p => p.Login)
                    .HasColumnType("varchar")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(p => p.Password)
                    .HasColumnType("varchar")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(p => p.RefreshToken)
                    .HasColumnType("varchar")
                    .HasMaxLength(100)
                    .IsRequired(false);

                entity.Property(p => p.RefreshTokenExpiresAt)
                    .HasColumnType("datetime2(7)")
                    .IsRequired(false);
            });

            modelBuilder.Entity<Product>(entity =>
            {
                // Explicitly set table name
                entity.ToTable("Products");

                // Define primary key
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Name)
                    .HasColumnType("varchar")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(p => p.Price)
                    .HasColumnType("DECIMAL(10,2)");

                // Enforce RowVersion for concurrency checks
                entity.Property(p => p.RowVersion)
                    .IsRowVersion();
            });

            modelBuilder.Entity<ProductReadOnly>(entity =>
            {
                // Explicitly set table name
                entity.ToTable("ProductsReadOnly");

                // Define primary key
                entity.HasKey(p => p.Id);

                entity.Property(p => p.Name)
                    .HasColumnType("varchar")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(p => p.Price)
                    .HasColumnType("DECIMAL(10,2)");
            });


            modelBuilder.Entity<UserAccount>().HasData(
                new UserAccount {
                    UserAccountId = Guid.Parse("4EC76740-6895-40F4-ABB8-3FBAB440FFF1"),
                    FirstName = "JWT",
                    LastName = "Issuer",
                    Login = "JwtIssuer",
                    Password = "5AfOmFg6TwOudeFBqxwkFQ==.FpN5yzKjshmyvOuDxS/khiFcwdTXXUGhhy1ixnsq6m4="
                });

            modelBuilder.Entity<UserAccount>().HasData(
                new UserAccount
                {
                    UserAccountId = Guid.Parse("9B2E1B59-56B0-41E4-B1AA-52418591E40C"),
                    FirstName = "QA",
                    LastName = "TestUser",
                    Login = "QATestUser",
                    Password = "0blgA5Hpn+WErqfeZe9IJg==.Q/mx5MQRlEmZHHKUiYnH5jWrxHKSTK5InQUi3tZQXqk="
                });

            base.OnModelCreating(modelBuilder);

        }
    }
}
