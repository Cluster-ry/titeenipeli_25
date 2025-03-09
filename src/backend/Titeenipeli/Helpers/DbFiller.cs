using Titeenipeli.Common.Database;
using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Enums;
using Titeenipeli.Options;
using Titeenipeli.Services;

namespace Titeenipeli.Helpers;

public static class DbFiller
{
    public static void Initialize(ApiDbContext dbContext, GameOptions gameOptions, bool isDevelopment)
    {
        const bool isPerformanceTest = false;

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
            guilds.AddRange(
                from GuildName name in guildNames
                select new Guild
                {
                    Name = name,
                    ActiveCtfFlags = [],
                    BaseRateLimit = gameOptions.PixelsPerMinutePerGuild,
                    PixelBucketSize = gameOptions.InitialPixelBucketSize,
                    FogOfWarDistance = gameOptions.FogOfWarDistance
                }
            );

            dbContext.Guilds.AddRange(guilds);

            dbContext.SaveChanges();
        }


        List<User> defaultUsers = [];

        if (isDevelopment)
        {
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

            defaultUsers.AddRange(testUser, testOpponent);
        }

        if (isPerformanceTest)
        {
            defaultUsers = GetPerformanceTestUsers(dbContext, gameOptions);
        }


        var god = new User
        {
            Code = "God",
            Guild = new Guild { Name = GuildName.Nobody },
            SpawnX = -10,
            SpawnY = -10,
            PowerUps = [],
            AuthenticationToken = "MHrVvoXYtTrSaxfMAYvzaKAmLiXemiLP",
            TelegramId = "99",
            FirstName = "God",
            LastName = "",
            Username = "God",
            IsGod = true
        };

        defaultUsers.Add(god);

        if (!dbContext.Users.Any())
        {
            foreach (var user in defaultUsers)
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
                dbContext.PowerUps.Add(
                    new PowerUp
                    {
                        PowerId = (int)powerUp,
                        Name = powerUp.ToString(),
                        Directed = powerUpService.GetByEnum(powerUp)?.Directed ?? false
                    }
                );
            }
        }

        if (dbContext.Map.Any())
        {
            dbContext.SaveChanges();
            return;
        }


        for (int x = 0; x < gameOptions.Width; x++)
        {
            for (int y = 0; y < gameOptions.Height; y++)
            {
                dbContext.Map.Add(
                    new Pixel
                    {
                        X = x,
                        Y = y,
                        User = null
                    }
                );
            }
        }

        dbContext.SaveChanges();

        if (isDevelopment)
        {
            var users = dbContext.Users.ToList();

            foreach (var user in users)
            {
                var spawn = dbContext.Map.FirstOrDefault(pixel =>
                    pixel.X == user.SpawnX && pixel.Y == user.SpawnY
                );

                if (spawn != null)
                {
                    spawn.User = user;
                }
            }
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
                Token = "#FGSTLBGXM3YB7USWS28KE2JV9Z267L48",
                Powerup = new PowerUp
                {
                    PowerId = (int)PowerUps.Binary,
                    Name = PowerUps.Binary.ToString(),
                    Directed = true,
                }
            },
            new CtfFlag
            {
                Token = "#COMMAND_NOT_FOUND",
                Powerup = new PowerUp
                {
                    PowerId = (int)PowerUps.Binary,
                    Name = PowerUps.Binary.ToString(),
                    Directed = true,
                }
            },
            new CtfFlag
            {
                Token = "#RUUSU_KASVAA_MUN_SYDÄMMESSÄNI",
                Powerup = new PowerUp
                {
                    PowerId = (int)PowerUps.Ruusu,
                    Name = PowerUps.Ruusu.ToString(),
                    Directed = false,
                }
            },
            new CtfFlag
            {
                Token = "#GOOD_FOR_YOU",
                Powerup = new PowerUp
                {
                    PowerId = (int)PowerUps.Siika,
                    Name = PowerUps.Siika.ToString(),
                    Directed = true,
                }
            },
            new CtfFlag
            {
                Token = "#WeAreProudOfYou",
                Powerup = new PowerUp
                {
                    PowerId = (int)PowerUps.Heart,
                    Name = PowerUps.Heart.ToString(),
                    Directed = false,
                }
            },
            new CtfFlag { Token = "#I_DONT_KNOW_THE_RULES", BaserateMultiplier = 1.2f },
            new CtfFlag
            {
                Token = "#TÄLLÄ_EI_SAA_POWER_UPIA",
                Powerup = new PowerUp
                {
                    PowerId = (int)PowerUps.Dino,
                    Name = PowerUps.Dino.ToString(),
                    Directed = false,
                }
            },
            new CtfFlag
            {
                Token = "#TÄLLÄ_SAA",
                Powerup = new PowerUp
                {
                    PowerId = (int)PowerUps.Bottle,
                    Name = PowerUps.Bottle.ToString(),
                    Directed = true,
                }
            },
            new CtfFlag
            {
                Token = "#ARE_YOU_SURE?",
                Powerup = new PowerUp
                {
                    PowerId = (int)PowerUps.SpaceInvader,
                    Name = PowerUps.SpaceInvader.ToString(),
                    Directed = false,
                }
            },
            new CtfFlag
            {
                Token = "#OH_YOU_FOUND_THIS?",
                Powerup = new PowerUp
                {
                    PowerId = (int)PowerUps.SpaceInvader,
                    Name = PowerUps.SpaceInvader.ToString(),
                    Directed = false,
                }
            },
            new CtfFlag { Token = "#TITEENIJAMIT2025", FogOfWarIncrease = 1 },
            new CtfFlag { Token = "#muinaistenroomal4istentavo1n", BucketSizeIncrease = 3 },
            new CtfFlag
            {
                Token = "#DiYgzPKLRjvJmiWa",
                Powerup = new PowerUp
                {
                    PowerId = (int)PowerUps.IsoL,
                    Name = PowerUps.IsoL.ToString(),
                    Directed = true,
                }
            },
            new CtfFlag
            {
                Token = "#oZgaUGMKrSiyNhad",
                Powerup = new PowerUp
                {
                    PowerId = (int)PowerUps.Bottle,
                    Name = PowerUps.Bottle.ToString(),
                    Directed = false,
                }
            },
            new CtfFlag
            {
                Token = "#LjBFDNbrjKyXGdaN",
                Powerup = new PowerUp
                {
                    PowerId = (int)PowerUps.Gem,
                    Name = PowerUps.Gem.ToString(),
                    Directed = false,
                }
            },
            new CtfFlag { Token = "#zRdDdUGrYdtQWnEh", BucketSizeIncrease = 10 },
            new CtfFlag
            {
                Token = "#qoEgKzHALknkGRee",
                Powerup = new PowerUp
                {
                    PowerId = (int)PowerUps.Star,
                    Name = PowerUps.Star.ToString(),
                    Directed = true,
                }
            },
            new CtfFlag
            {
                Token = "#fTrCSPxDPxfajYmq",
                Powerup = new PowerUp
                {
                    PowerId = (int)PowerUps.Heart,
                    Name = PowerUps.Heart.ToString(),
                    Directed = false,
                }
            },
            new CtfFlag
            {
                Token = "#pyQawhWNuhvhpQCt",
                Powerup = new PowerUp
                {
                    PowerId = (int)PowerUps.Siika,
                    Name = PowerUps.Siika.ToString(),
                    Directed = true,
                }
            },
            new CtfFlag { Token = "#JpoHCiDpWKrgzsRL", BaserateMultiplier = 1.1f },
            new CtfFlag { Token = "#hwGtrdNWVzYvCodV", BaserateMultiplier = 1.1f },
            new CtfFlag
            {
                Token = "#FHZaqUaZCmrNQALU",
                Powerup = new PowerUp
                {
                    PowerId = (int)PowerUps.Bottle,
                    Name = PowerUps.Bottle.ToString(),
                    Directed = true,
                }
            },
            new CtfFlag
            {
                Token = "#ZKzsxjzXBKgSouSQ",
                Powerup = new PowerUp
                {
                    PowerId = (int)PowerUps.Dino,
                    Name = PowerUps.Dino.ToString(),
                    Directed = false,
                }
            },
            new CtfFlag
            {
                Token = "#vDyHCnxENDjhJjui",
                Powerup = new PowerUp
                {
                    PowerId = (int)PowerUps.Gem,
                    Name = PowerUps.Gem.ToString(),
                    Directed = false,
                }
            },
            new CtfFlag { Token = "#tFUXMdvwVDWckyeA", BucketSizeIncrease = 5 },
            new CtfFlag { Token = "#VjTSYcnUhzbAqwPS", BaserateMultiplier = 1.3f },
            new CtfFlag
            {
                Token = "#gTxnCQCrzPfBpYce",
                Powerup = new PowerUp
                {
                    PowerId = (int)PowerUps.SpaceInvader,
                    Name = PowerUps.SpaceInvader.ToString(),
                    Directed = false,
                }
            },
            new CtfFlag
            {
                Token = "#BToASAZDhyQqnLew",
                Powerup = new PowerUp
                {
                    PowerId = (int)PowerUps.Binary,
                    Name = PowerUps.Binary.ToString(),
                    Directed = false,
                }
            },
            new CtfFlag
            {
                Token = "#itiaapfkGrbvzwtL",
                Powerup = new PowerUp
                {
                    PowerId = (int)PowerUps.Binary,
                    Name = PowerUps.Binary.ToString(),
                    Directed = true,
                }
            },
            new CtfFlag { Token = "#ZjnJFxkiNwdstvYQ", BucketSizeIncrease = 5 },
            new CtfFlag { Token = "#WbndZQmZGfXAJCMD", BaserateMultiplier = 1.5f },
            new CtfFlag { Token = "#birktyMvnUAhKrfN", FogOfWarIncrease = 1 },
            new CtfFlag
            {
                Token = "#TITYÄÄNIKIRVES",
                Powerup = new PowerUp
                {
                    PowerId = (int)PowerUps.Titeenikirves,
                    Name = PowerUps.Titeenikirves.ToString(),
                    Directed = true,
                }
            },
            new CtfFlag
            {
                Token = "#MFilesSponsorsTiteenit",
                Powerup = new PowerUp
                {
                    PowerId = (int)PowerUps.MFiles,
                    Name = PowerUps.MFiles.ToString(),
                    Directed = false,
                }
            },
            new CtfFlag
            {
                Token = "#43EHAJBKPOAH3GKAJ53C",
                Powerup = new PowerUp
                {
                    PowerId = (int)PowerUps.Star,
                    Name = PowerUps.Star.ToString(),
                    Directed = false,
                }
            },
            new CtfFlag
            {
                Token = "#XTF37CVY50TBZGKQVNRT",
                Powerup = new PowerUp
                {
                    PowerId = (int)PowerUps.Gem,
                    Name = PowerUps.Gem.ToString(),
                    Directed = false,
                }
            },
            new CtfFlag
            {
                Token = "#V8VDVG7LTBD5R7SSW4RR",
                Powerup = new PowerUp
                {
                    PowerId = (int)PowerUps.Heart,
                    Name = PowerUps.Heart.ToString(),
                    Directed = false,
                }
            },
        ];
    }

    private static List<User> GetPerformanceTestUsers(ApiDbContext dbContext, GameOptions gameOptions)
    {
        var guildNames = Enum.GetValues<GuildName>().ToList();
        guildNames = guildNames.Skip(1).ToList();

        int id = 1;
        List<User> testUsers = [];
        foreach (var guildName in guildNames)
        {
            for (int i = 1; i < 40; i += 2)
            {
                int x;
                int y = 1;

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
                    Code = "test_" + id,
                    Guild =
                        dbContext.Guilds.FirstOrDefault(guild => guild.Name == guildName)
                        ?? throw new InvalidOperationException(),
                    SpawnX = x,
                    SpawnY = y,
                    PixelBucket = gameOptions.InitialPixelBucket,
                    PowerUps = [],
                    TelegramId = id.ToString(),
                    FirstName = "",
                    LastName = "",
                    Username = ""
                };

                testUsers.Add(user);
                id = ++id;
            }

            for (int i = 2; i < 40; i += 2)
            {
                int x;
                int y;

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
                    Code = "test_" + id,
                    Guild =
                        dbContext.Guilds.FirstOrDefault(guild => guild.Name == guildName)
                        ?? throw new InvalidOperationException(),
                    SpawnX = x,
                    SpawnY = y,
                    PixelBucket = gameOptions.InitialPixelBucket,
                    PowerUps = [],
                    TelegramId = id.ToString(),
                    FirstName = "",
                    LastName = "",
                    Username = ""
                };

                testUsers.Add(user);
                id = ++id;
            }
        }

        return testUsers;
    }
}
