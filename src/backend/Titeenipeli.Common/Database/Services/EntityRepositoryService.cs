namespace Titeenipeli.Common.Database.Services;

public abstract class EntityRepositoryService(ApiDbContext dbContext)
{
    protected readonly ApiDbContext DbContext = dbContext;

    public int SaveChanges()
    {
        return DbContext.SaveChanges();
    }

    public Task<int> SaveChangesAsync()
    {
        return DbContext.SaveChangesAsync();
    }
}