using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Titeenipeli.Context;
using Titeenipeli.Schema;

namespace Titeenipeli.Helpers;

public static class DbFiller
{
    public static void Initialize(ApiDbContext dbContext)
    {
        RelationalDatabaseCreator databaseCreator =
            (RelationalDatabaseCreator)dbContext.Database.GetService<IDatabaseCreator>();

        if (!dbContext.CtfFlags.Any())
        {
            dbContext.CtfFlags.Add(new CtfFlag
            {
                Flag = "#TEST_FLAG",
                Id = 0
            });

            dbContext.SaveChanges();
        }

        if (!dbContext.Guilds.Any())
        {
            Guild[] guilds =
            [
                new Guild { Color = 0 },
                new Guild { Color = 1 },
                new Guild { Color = 2 },
                new Guild { Color = 3 },
                new Guild { Color = 4 },
                new Guild { Color = 5 },
                new Guild { Color = 6 },
                new Guild { Color = 7 }
            ];

            dbContext.Guilds.AddRange(guilds);

            dbContext.SaveChanges();
        }

        User testUser = new User
        {
            Code = "test",
            Guild = dbContext.Guilds.FirstOrDefault() ?? throw new InvalidOperationException(),
            SpawnX = 0,
            SpawnY = 0
        };

        if (!dbContext.Users.Any())
        {
            dbContext.Users.Add(testUser);

            dbContext.SaveChanges();
        }

        if (!dbContext.Map.Any())
        {
            Random random = new Random();
            for (int x = 0; x < 10; x++)
            for (int y = 0; y < 10; y++)
                dbContext.Map.Add(new Pixel
                {
                    X = x,
                    Y = y,
                    User = random.Next(10) == 5 ? testUser : null
                });
        }

        dbContext.SaveChanges();
    }

    public static void Clear(ApiDbContext dbContext)
    {
        dbContext.Database.EnsureDeleted();
    }
}