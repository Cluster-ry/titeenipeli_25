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
            TelegramId = "1",
            FirstName = "",
            LastName = "",
            Username = ""
        };

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
            AuthenticationToken = "MHrVvoXYtTrSaxfMAYvzaKAmLiXemiLP",
            TelegramId = "99",
            FirstName = "God",
            LastName = "",
            Username = "God",
            IsGod = true
        };


        if (!dbContext.Users.Any())
        {
            dbContext.Users.AddRange(testUser, testOpponent, god);
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
            new CtfFlag
            {
                Token = "#I_DONT_KNOW_THE_RULES",
                BaserateMultiplier = 1.2f
            },
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
            new CtfFlag
            {
                Token = "#TITEENIJAMIT2025",
                FogOfWarIncrease = 1
            },
            new CtfFlag
            {
                Token = "#muinaistenroomal4istentavo1n",
                BucketSizeIncrease = 3
            },
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
            new CtfFlag
            {
                Token = "#zRdDdUGrYdtQWnEh",
                BucketSizeIncrease = 10
            },
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
            new CtfFlag
            {
                Token = "#JpoHCiDpWKrgzsRL",
                BaserateMultiplier = 1.1f
            },
            new CtfFlag
            {
                Token = "#hwGtrdNWVzYvCodV",
                BaserateMultiplier = 1.1f
            },
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
            new CtfFlag
            {
                Token = "#tFUXMdvwVDWckyeA",
                BucketSizeIncrease = 5
            },
            new CtfFlag
            {
                Token = "#VjTSYcnUhzbAqwPS",
                BaserateMultiplier = 1.3f
            },
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
                    PowerId = (int)PowerUps.Glitch,
                    Name = PowerUps.Glitch.ToString(),
                    Directed = true,
                }
            },
            new CtfFlag
            {
                Token = "#ZjnJFxkiNwdstvYQ",
                BucketSizeIncrease = 5
            },
            new CtfFlag
            {
                Token = "#WbndZQmZGfXAJCMD",
                BaserateMultiplier = 1.5f
            },
            new CtfFlag
            {
                Token = "#birktyMvnUAhKrfN",
                FogOfWarIncrease = 1
            },
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
}