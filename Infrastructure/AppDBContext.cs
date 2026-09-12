using Core.Models;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class AppDBContext : IdentityDbContext<IdentityUser>, IDataProtectionKeyContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Products>(entity =>
            {
                entity.Property(p => p.OldPrice)
                    .HasColumnType("decimal(18,2)");

                entity.Property(p => p.DiscountPercentage)
                    .HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.Property(o => o.TotalPrice)
                    .HasColumnType("decimal(18,2)");

                entity.OwnsMany(o => o.OrderLines, line =>
                {
                    line.Property(l => l.Price)
                        .HasColumnType("decimal(18,2)");
                });

                entity.OwnsMany(o => o.MissingOrderLines, line =>
                {
                    line.Property(l => l.Price)
                        .HasColumnType("decimal(18,2)");
                });
            });
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Products> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;
    }
}
