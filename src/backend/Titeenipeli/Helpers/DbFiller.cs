using Titeenipeli.Common.Database;
using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Enums;
using Titeenipeli.Options;
using Titeenipeli.Services;

namespace Titeenipeli.Helpers;

public static class DbFiller
{
    public static void Initialize(ApiDbContext dbContext, GameOptions gameOptions)
    {
        if (!dbContext.CtfFlags.Any())
        {
            dbContext.AddRange(GetCtfFlags());
            dbContext.SaveChanges();
        }

        if (!dbContext.Guilds.Any())
        {
            List<Guild> guilds = [];
            var guildNames = Enum.GetValues<GuildName>().ToList();
            // Skip Nobody.
            guildNames = guildNames.Skip(1).ToList();
            guilds.AddRange(from GuildName name in guildNames
                            select new Guild
                            {
                                Name = name,
                                ActiveCtfFlags = [],
                                BaseRateLimit = gameOptions.PixelsPerMinutePerGuild,
                                PixelBucketSize = gameOptions.InitialPixelBucketSize,
                                FogOfWarDistance = gameOptions.FogOfWarDistance
                            });

            dbContext.Guilds.AddRange(guilds);

            dbContext.SaveChanges();
        }

        var guildNames2 = Enum.GetValues<GuildName>().ToList();
        guildNames2 = guildNames2.Skip(1).ToList();
        var id = 1;
        List<User> testUsers = [];
        foreach (var guildName in guildNames2)
        {
            for (int i = 1; i < 40; i = i + 2)
            {
                var x = 0;
                var y = 1;
                if ((int)guildName <= 5)
                {
                    x = i + ((int)guildName - 1) * 40;
                }
                else
                {
                    x = i + ((int)guildName - 6) * 40;
                    y = 199;
                }
                var user = new User
                {
                    Code = "test_" + id.ToString(),
                    Guild = dbContext.Guilds.FirstOrDefault(guild => guild.Name == guildName) ?? throw new InvalidOperationException(),
                    SpawnX = x,
                    SpawnY = y,
                    PixelBucket = gameOptions.InitialPixelBucket,
                    PowerUps = [],
                    TelegramId = id.ToString(),
                    FirstName = "",
                    LastName = "",
                    Username = "",
                };
                testUsers.Add(user);
                id = ++id;
            }
            for (int i = 2; i < 40; i = i + 2)
            {
                var x = 0;
                var y = 0;
                if ((int)guildName <= 5)
                {
                    x = i + ((int)guildName - 1) * 40;
                    y = 3;
                }
                else
                {
                    x = i + ((int)guildName - 6) * 40;
                    y = 197;
                }
                var user = new User
                {
                    Code = "test_" + id.ToString(),
                    Guild = dbContext.Guilds.FirstOrDefault(guild => guild.Name == guildName) ?? throw new InvalidOperationException(),
                    SpawnX = x,
                    SpawnY = y,
                    PixelBucket = gameOptions.InitialPixelBucket,
                    PowerUps = [],
                    TelegramId = id.ToString(),
                    FirstName = "",
                    LastName = "",
                    Username = "",
                };
                testUsers.Add(user);
                id = ++id;
            }
        }

        /*
        var testUser = new User
        {
            Code = "test",
            Guild = dbContext.Guilds.FirstOrDefault() ?? throw new InvalidOperationException(),
            SpawnX = 5,
            SpawnY = 5,
            PixelBucket = gameOptions.InitialPixelBucket,
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
            PixelBucket = gameOptions.InitialPixelBucket,
            PowerUps = [],
            TelegramId = "1001",
            FirstName = "",
            LastName = "",
            Username = ""
        };
        */

        var god = new User
        {
            Code = "God",
            Guild = new Guild
            {
                Name = GuildName.Nobody
            },
            SpawnX = -10,
            SpawnY = -10,
            PowerUps = [],
            AuthenticationToken = "puE0g4NkCQlfIQFnrs5xPr0aRQZ9STCv",
            TelegramId = "1099",
            FirstName = "God",
            LastName = "",
            Username = "God",
            IsGod = true
        };



        if (!dbContext.Users.Any())
        {
            dbContext.Users.AddRange(god);
            foreach (var user in testUsers)
            {
                dbContext.Users.Add(user);
            }
            dbContext.SaveChanges();
        }


        if (!dbContext.PowerUps.Any())
        {
            var powerUpService = new PowerupService(gameOptions);
            foreach (var powerUp in Enum.GetValues<PowerUps>())
            {
                dbContext.PowerUps.Add(new PowerUp
                {
                    PowerId = (int)powerUp,
                    Name = powerUp.ToString(),
                    Directed = powerUpService.GetByEnum(powerUp)?.Directed ?? false
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
            /*
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
                        */
            var users = dbContext.Users.ToList();

            foreach (var user in users)
            {
                var spawn = dbContext.Map.FirstOrDefault(pixel => pixel.X == user.SpawnX && pixel.Y == user.SpawnY);

                if (spawn != null)
                {
                    spawn.User = user;
                }
            }

            dbContext.SaveChanges();
        }

        dbContext.SaveChanges();
    }

    public static void Clear(ApiDbContext dbContext)
    {
        dbContext.Database.EnsureDeleted();
    }

    private static List<CtfFlag> GetCtfFlags()
    {
        return
        [
            new CtfFlag
            {
                Token = "#TEST_FLAG"
            },
            new CtfFlag
            {
                Token = "#TITEENIKIRVES",
                Powerup = new PowerUp
                {
                    PowerId = (int)PowerUps.Titeenikirves,
                    Name = PowerUps.Titeenikirves.ToString(),
                    Directed = true,
                }
            },
            new CtfFlag
            {
                Token = "FGSTLBGXM3YB7USWS28KE2JV9Z267L48"
            },
            new CtfFlag
            {
                Token = "#COMMAND_NOT_FOUND"
            },
            new CtfFlag
            {
                Token = "#RUUSU_KASVAA_MUN_SYDÄMMESSÄNI"
            },
            new CtfFlag
            {
                Token = "#GOOD_FOR_YOU"
            },
            new CtfFlag
            {
                Token = "#I_DONT_KNOW_THE_RULES"
            },
            new CtfFlag
            {
                Token = "#TÄLLÄ_EI_SAA_POWER_UPIA"
            },
            new CtfFlag
            {
                Token = "#TÄLLÄ_SAA"
            },
            new CtfFlag
            {
                Token = "#ARE_YOU_SURE?"
            },
            new CtfFlag
            {
                Token = "#OH_YOU_FOUND_THIS?"
            },
            new CtfFlag
            {
                Token = "#TITEENIJAMIT2025"
            },
        ];
    }
}