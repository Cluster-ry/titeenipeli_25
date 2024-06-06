using Microsoft.EntityFrameworkCore;
using Titeenipeli.Models;

namespace Titeenipeli.Context;

public class ApiDbContext : DbContext
{
    public ApiDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<CtfFlag>? Flags { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CtfFlag>().ToTable("CtfFlags");
    }
}