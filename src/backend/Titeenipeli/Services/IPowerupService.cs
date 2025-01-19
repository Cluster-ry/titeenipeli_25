using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Enums;
using Titeenipeli.SpecialEffects;

namespace Titeenipeli.Services;

public interface IPowerupService
{
    ISpecialEffect? GetByEnum(PowerUps powerUp);
    ISpecialEffect? GetByDb(PowerUp powerUp);
}