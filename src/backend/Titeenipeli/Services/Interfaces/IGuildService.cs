using Titeenipeli.Schema;

namespace Titeenipeli.Services.Interfaces;

public interface IGuildService : IEntityService<Guild>
{
    public Guild? GetByColor(int color);
}