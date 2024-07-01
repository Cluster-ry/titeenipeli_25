using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Titeenipeli.Context;
using Titeenipeli.Options;
using Titeenipeli.Schema;

namespace Titeenipeli.Helpers;

public static class DbFiller
{
    public static void Initialize(ApiDbContext dbContext, GameOptions gameOptions)
    {
        RelationalDatabaseCreator databaseCreator =
            (RelationalDatabaseCreator)dbContext.Database.GetService<IDatabaseCreator>();

        if (!dbContext.CtfFlags.Any())
        {
            dbContext.CtfFlags.Add(new CtfFlag
            {
                Token = "#TEST_FLAG",
                Id = 0
            });

            dbContext.SaveChanges();
        }

        if (!dbContext.Guilds.Any())
        {
            Guild[] guilds =
            [
                new Guild
                {
                    Color = 0
                },
                new Guild
                {
                    Color = 1
                },
                new Guild
                {
                    Color = 2
                },
                new Guild
                {
                    Color = 3
                },
                new Guild
                {
                    Color = 4
                },
                new Guild
                {
                    Color = 5
                },
                new Guild
                {
                    Color = 6
                },
                new Guild
                {
                    Color = 7
                }
            ];

            dbContext.Guilds.AddRange(guilds);

            dbContext.SaveChanges();
        }

        User testUser = new User
        {
            Code = "test",
            Guild = dbContext.Guilds.FirstOrDefault() ?? throw new InvalidOperationException(),
            SpawnX = 5,
            SpawnY = 5
        };

        User testOpponent = new User
        {
            Code = "opponent",
            Guild = dbContext.Guilds.FirstOrDefault(guild => guild.Color == 4) ?? throw new InvalidOperationException(),
            SpawnX = 3,
            SpawnY = 2
        };


        if (!dbContext.Users.Any())
        {
            dbContext.Users.Add(testUser);
            dbContext.Users.Add(testOpponent);

            dbContext.SaveChanges();
        }

        if (!dbContext.Map.Any())
        {
            Random random = new Random(1);
            for (int x = 0; x < gameOptions.Width; x++)
            {
                for (int y = 0; y < gameOptions.Height; y++)
                {
                    dbContext.Map.Add(new Pixel
                    {
                        X = x,
                        Y = y,
                        User = random.Next(10) < 1 && x > 3 && y > 3 && x < 17 && y < 17 ? testUser : testOpponent
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