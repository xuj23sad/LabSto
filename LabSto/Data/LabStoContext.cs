using Microsoft.EntityFrameworkCore;
using LabSto.Models.Entities;

namespace LabSto.Data
{
    public class LabStoContext(DbContextOptions<LabStoContext> options) : DbContext(options)
    {
        public DbSet<Reagent> Reagent { get; set; } = default!;
        public DbSet<Cabinet> Cabinet { get; set; } = default!;
        public DbSet<CabinetSlot> CabinetSlot { get; set; } = default!;
        public DbSet<StockRecord> StockRecord { get; set; } = default!;
        public DbSet<User> User { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CabinetSlot>(entity =>
            {
                entity.HasOne(s => s.Cabinet)
                    .WithMany(c => c.Slots)
                    .HasForeignKey(s => s.CabinetId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(s => s.Reagent)
                    .WithMany(r => r.Slots)
                    .HasForeignKey(s => s.ReagentId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<StockRecord>(entity =>
            {
                entity.HasOne(s => s.Reagent)
                    .WithMany(r => r.StockRecords)
                    .HasForeignKey(s => s.ReagentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<User>().HasIndex(u => u.UserName).IsUnique();
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        }
    }
}
