namespace Titeenipeli.Common.Database.Services;

abstract public class RepositoryService(ApiDbContext dbContext)
{
    protected readonly ApiDbContext _dbContext = dbContext;

    public int SaveChanges()
    {
        return _dbContext.SaveChanges();
    }

    public Task<int> SaveChangesAsync()
    {
        return _dbContext.SaveChangesAsync();
    }
}