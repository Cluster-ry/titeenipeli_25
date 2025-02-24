using FluentAssertions;
using NUnit.Framework;
using Titeenipeli.Common.Enums;
using Titeenipeli.GameLogic;
using Titeenipeli.Options;

namespace Titeenipeli.Tests.GameLogic;

[TestFixture]
[TestOf(typeof(SpawnGenerator))]
public class SpawnGeneratorTest
{
    [TestCase(TestName = "Should return spawn point in given constraints (1000 iterations)")]
    public void GetSpawnPointTest()
    {
        var options = new GameOptions
        {
            SpawnAreaDistanceFromCenter = 10,
            SpawnAreaRadius = 5,
            SpawnAreasPerGuild = 2
        };

        var spawnGenerator = new SpawnGenerator(options);

        for (int i = 0; i < 1000; i++)
        {
            var result = spawnGenerator.GetSpawnPoint(GuildName.Tietokilta);

            result.X.Should().Match(x => (x >= 5 && x <= 15) || (x <= -5 && x >= -15));
            result.Y.Should().BeInRange(-5, 5);
        }
    }
}