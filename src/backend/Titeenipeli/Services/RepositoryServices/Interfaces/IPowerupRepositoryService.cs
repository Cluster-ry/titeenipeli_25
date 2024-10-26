using Titeenipeli.Schema;

namespace Titeenipeli.Services.RepositoryServices.Interfaces;

public interface IPowerupRepositoryService : IEntityRepositoryService<PowerUp>
{
    public IReadOnlyCollection<PowerUp>? UserPowers(int userid);

}