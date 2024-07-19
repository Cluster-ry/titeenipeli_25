using Titeenipeli.Context;
using Titeenipeli.Schema;
using Titeenipeli.Services.Interfaces;

namespace Titeenipeli.Services;

public class CtfFlagService : ICtfFlagService
{
    private readonly ApiDbContext _dbContext;

    public CtfFlagService(ApiDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public CtfFlag? GetCtfFlag(int id)
    {
        return _dbContext.CtfFlags.Find(id);
    }

    public CtfFlag? GetCtfFlagByToken(string token)
    {
        return _dbContext.CtfFlags.FirstOrDefault(flag => flag.Token == token);
    }
}