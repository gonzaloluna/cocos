using CocosTradingAPI.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using CocosTradingAPI.Domain.Enums;

namespace CocosTradingAPI.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Instrument> Instruments { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<MarketData> MarketData { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var stringConverter = new EnumToStringConverter<OrderStatus>();
            modelBuilder.Entity<Order>()
                .Property(o => o.Status)
                .HasConversion(stringConverter);

            modelBuilder.Entity<Order>()
                .Property(o => o.Side)
                .HasConversion(new EnumToStringConverter<OrderSide>());

            modelBuilder.Entity<Instrument>()
                .Property(i => i.Type)
                .HasConversion(new EnumToStringConverter<InstrumentType>());

            modelBuilder.Entity<Order>()
                .Property(o => o.Type)
                .HasConversion<string>()
                .HasMaxLength(10)
                .HasColumnName("type");

        }
    }
}
