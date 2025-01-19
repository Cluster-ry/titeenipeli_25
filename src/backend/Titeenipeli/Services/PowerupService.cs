using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Enums;
using Titeenipeli.Options;
using Titeenipeli.SpecialEffects;

namespace Titeenipeli.Services;

public sealed class PowerupService(GameOptions gameOptions) : IPowerupService
{
    public ISpecialEffect? GetByEnum(PowerUps powerUp) => SelectSpecialEffect(powerUp);
    public ISpecialEffect? GetByPowerId(int powerId) => SelectSpecialEffect((PowerUps)powerId);
    public ISpecialEffect? GetByDb(PowerUp powerUp) => SelectSpecialEffect((PowerUps)powerUp.PowerId);


    private ISpecialEffect? SelectSpecialEffect(PowerUps? powerUp) =>
    powerUp switch
    {
        PowerUps.TestEffect => new TestEffect(),
        PowerUps.Titeenikirves => new TiteenikirvesEffect(gameOptions.Height, gameOptions.Width),
        _ => null
    };


}