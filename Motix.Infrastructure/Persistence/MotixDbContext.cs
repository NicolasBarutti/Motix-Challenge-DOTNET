using Microsoft.EntityFrameworkCore;
using Motix.Domain.Entities;

namespace Motix.Infrastructure.Persistence;

public class MotixDbContext : DbContext
{
    public MotixDbContext(DbContextOptions<MotixDbContext> options) : base(options) { }

    public DbSet<Sector> Sectors => Set<Sector>();
    public DbSet<Motorcycle> Motorcycles => Set<Motorcycle>();
    public DbSet<Movement> Movements => Set<Movement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Sector>().ToTable("SECTORS");
        modelBuilder.Entity<Motorcycle>().ToTable("MOTORCYCLES");
        modelBuilder.Entity<Movement>().ToTable("MOVEMENTS");

        // FKs mínimas (sem navegação nas entidades)
        modelBuilder.Entity<Motorcycle>().HasOne<Sector>().WithMany().HasForeignKey(m => m.SectorId);
        modelBuilder.Entity<Movement>().HasOne<Motorcycle>().WithMany().HasForeignKey(mv => mv.MotorcycleId);
        modelBuilder.Entity<Movement>().HasOne<Sector>().WithMany().HasForeignKey(mv => mv.SectorId);
    }
}