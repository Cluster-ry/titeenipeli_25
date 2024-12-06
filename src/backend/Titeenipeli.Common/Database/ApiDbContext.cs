using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Titeenipeli.Common.Database.Schema;

namespace Titeenipeli.Common.Database;

public class ApiDbContext : DbContext
{
    public ApiDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<CtfFlag> CtfFlags { get; init; } = null!;
    public DbSet<Guild> Guilds { get; init; } = null!;
    public DbSet<User> Users { get; init; } = null!;
    public DbSet<Pixel> Map { get; init; } = null!;
    public DbSet<PowerUp> PowerUps { get; init; } = null!;
    public DbSet<GuildPowerUp> GuildPowerUps { get; init; } = null!;
    public DbSet<GameEvent> GameEvents { get; init; } = null!;


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
                                                         .Where(x => x is
                                                         {
                                                             Entity: Entity,
                                                             State: EntityState.Added or EntityState.Modified
                                                         });

        foreach (var entity in entities)
        {
            var now = DateTime.UtcNow; // current datetime

            if (entity.State == EntityState.Added)
            {
                ((Entity)entity.Entity).CreatedDateTime = now;
            }

            ((Entity)entity.Entity).UpdatedDateTime = now;
        }
    }
}