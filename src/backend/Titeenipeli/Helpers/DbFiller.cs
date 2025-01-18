using Titeenipeli.Common.Database;
using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Enums;
using Titeenipeli.Controllers;
using Titeenipeli.Options;

namespace Titeenipeli.Helpers;

public static class DbFiller
{
    public static void Initialize(ApiDbContext dbContext, GameOptions gameOptions)
    {
        if (!dbContext.CtfFlags.Any())
        {
            dbContext.CtfFlags.Add(new CtfFlag
            {
                Token = "#TEST_FLAG",
            });

            dbContext.CtfFlags.Add(new CtfFlag
            {
                Token = "#TITEENIKIRVES",
                Powerup = new PowerUp
                {
                    PowerId = (int)PowerUps.Titeenikirves,
                    Name = PowerUps.Titeenikirves.ToString(),
                    Description = PowerController.GetDescription(PowerUps.Titeenikirves)
                },
            });


            dbContext.SaveChanges();
        }

        if (!dbContext.Guilds.Any())
        {
            List<Guild> guilds = [];
            var guildNames = Enum.GetValues(typeof(GuildName)).Cast<GuildName>().ToList();
            // Skip Nobody.
            guildNames = guildNames.Skip(1).ToList();
            guilds.AddRange(from GuildName name in guildNames select new Guild { Name = name, ActiveCtfFlags = new() });

            dbContext.Guilds.AddRange(guilds);

            dbContext.SaveChanges();
        }

        var testUser = new User
        {
            Code = "test",
            Guild = dbContext.Guilds.FirstOrDefault() ?? throw new InvalidOperationException(),
            SpawnX = 5,
            SpawnY = 5,
            PowerUps = [],
            TelegramId = "0",
            FirstName = "",
            LastName = "",
            Username = "",
        };

        var testOpponent = new User
        {
            Code = "opponent",
            Guild = dbContext.Guilds.FirstOrDefault(guild => guild.Name == GuildName.TietoTeekkarikilta) ??
                    throw new InvalidOperationException(),
            SpawnX = 3,
            SpawnY = 2,
            PowerUps = [],
            TelegramId = "1",
            FirstName = "",
            LastName = "",
            Username = ""
        };


        if (!dbContext.Users.Any())
        {
            dbContext.Users.Add(testUser);
            dbContext.Users.Add(testOpponent);

            dbContext.SaveChanges();
        }


        if (!dbContext.PowerUps.Any())
        {
            foreach (var powerUp in Enum.GetValues<PowerUps>())
            {
                dbContext.PowerUps.Add(new PowerUp
                {
                    PowerId = (int)powerUp,
                    Name = powerUp.ToString(),
                    Description = PowerController.GetDescription(powerUp)
                });
            }
        }

        if (!dbContext.Map.Any())
        {
            for (int x = 0; x < gameOptions.Width; x++)
            {
                for (int y = 0; y < gameOptions.Height; y++)
                {
                    dbContext.Map.Add(new Pixel
                    {
                        X = x,
                        Y = y,
                        User = null
                    });
                }
            }

            dbContext.SaveChanges();

            Pixel? testUserSpawn =
                dbContext.Map.FirstOrDefault(pixel => pixel.X == testUser.SpawnX && pixel.Y == testUser.SpawnY);

            Pixel? testOpponentSpawn = dbContext.Map.FirstOrDefault(pixel =>
                pixel.X == testOpponent.SpawnX && pixel.Y == testOpponent.SpawnY);

            if (testUserSpawn != null)
            {
                testUserSpawn.User = testUser;
            }

            if (testOpponentSpawn != null)
            {
                testOpponentSpawn.User = testOpponent;
            }

            dbContext.SaveChanges();
        }

        dbContext.SaveChanges();
    }

    public static void Clear(ApiDbContext dbContext)
    {
        dbContext.Database.EnsureDeleted();
    }
}