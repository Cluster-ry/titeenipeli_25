using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Titeenipeli.Schema;

namespace Titeenipeli.Context;

public class ApiDbContext : DbContext
{
    public ApiDbContext(DbContextOptions options) : base(options)
    {
    }

    public required DbSet<CtfFlag> CtfFlags { get; init; }
    public required DbSet<Guild> Guilds { get; init; }
    public required DbSet<User> Users { get; init; }
    public required DbSet<Pixel> Map { get; init; }
    public required DbSet<PowerUp> PowerUps { get; init; }
    public required DbSet<GuildPowerUp> GuildPowerUps { get; init; }
    public required DbSet<GameEvent> GameEvents { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }

    public override int SaveChanges()
    {
        AddTimestamps();
        return base.SaveChanges();
    }

    private void AddTimestamps()
    {
        IEnumerable<EntityEntry> entities = ChangeTracker.Entries()
            .Where(x => x is { Entity: Entity, State: EntityState.Added or EntityState.Modified });

        foreach (EntityEntry entity in entities)
        {
            DateTime now = DateTime.UtcNow; // current datetime

            if (entity.State == EntityState.Added)
            {
                ((Entity)entity.Entity).CreatedDateTime = now;
            }

            ((Entity)entity.Entity).UpdatedDateTime = now;
        }
    }
}