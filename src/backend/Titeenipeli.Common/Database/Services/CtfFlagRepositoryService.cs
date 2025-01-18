using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Database.Services.Interfaces;

namespace Titeenipeli.Common.Database.Services;

public class CtfFlagRepositoryService(ApiDbContext dbContext)
    : EntityRepositoryService(dbContext), ICtfFlagRepositoryService
{
    public CtfFlag? GetById(int id)
    {
        return DbContext.CtfFlags.Find(id);
    }

    public List<CtfFlag> GetAll()
    {
        return DbContext.CtfFlags.ToList();
    }

    public void Add(CtfFlag ctfFlag)
    {
        DbContext.CtfFlags.Add(ctfFlag);
    }

    public CtfFlag? GetByToken(string token)
    {
        return DbContext.CtfFlags.FirstOrDefault(flag => flag.Token == token);
    }
}