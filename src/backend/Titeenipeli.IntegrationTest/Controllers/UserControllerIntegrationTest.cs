using Titeenipeli.Context;
using Titeenipeli.Controllers;
using Titeenipeli.Inputs;
using Titeenipeli.Models;
using Titeenipeli.Options;
using Titeenipeli.Schema;
using Titeenipeli.Services;
using Titeenipeli.Services.RepositoryServices;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace Titeenipeli.IntegrationTest.Controllers;

[TestFixture]
public class UserControllerIntegrationTest : BaseFixture
{
    [SetUp]
    public async Task BeforeEach()
    {
        _dbContext = new ApiDbContext(new DbContextOptionsBuilder().UseNpgsql(Postgres.GetConnectionString()).Options);
        await _dbContext.Database.EnsureCreatedAsync();
    }

    [TearDown]
    public async Task AfterEach()
    {
        await _dbContext.DisposeAsync();
    }

    private const string ClaimName = "jwt-claim";

    private readonly JwtService _jwtService = new JwtService(new JwtOptions
        {
            ClaimName = ClaimName,
            CookieName = "Auth",
            Encryption = string.Join("", Enumerable.Repeat(0, 32).Select(_ => (char)new Random().Next(97, 122))),
            Secret = string.Join("", Enumerable.Repeat(0, 256).Select(_ => (char)new Random().Next(97, 122)))
        }
    );

    private ApiDbContext _dbContext;

    [TestCase("19E0AD9B-E94C-44CC-A956-D094C7A4D58D", 200, TestName = "Should return success for new user")]
    [TestCase("9A81A0A0-1502-42B3-862E-50C2FF8B038F", 200, TestName = "Should return success for existing user")]
    public void PostUserTest(string telegramId, int statusCode)
    {
        UserRepositoryService userRepositoryService = new UserRepositoryService(_dbContext);

        userRepositoryService.Add(new User
        {
            Guild = null,
            Code = "",

            SpawnX = 0,
            SpawnY = 0,

            TelegramId = "9A81A0A0-1502-42B3-862E-50C2FF8B038F",
            FirstName = "",
            LastName = "",
            Username = "",
            PhotoUrl = "",
            AuthDate = "",
            Hash = ""
        });


        GuildRepositoryService guildRepositoryService = new GuildRepositoryService(_dbContext);


        UserController controller = new UserController(_jwtService,
            userRepositoryService,
            guildRepositoryService)
        {
            ControllerContext =
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        PostUsersInput input = new PostUsersInput
        {
            Id = telegramId,
            AuthDate = "",
            Hash = "",
            Username = "",
            FirstName = "",
            LastName = "",
            PhotoUrl = ""
        };

        IStatusCodeActionResult? result = controller.PostUsers(input) as IStatusCodeActionResult;
        result?.StatusCode.Should().Be(statusCode);
    }


    [TestCase("9A1367B1-1C7C-448E-88DA-DCAD275669FE", "1", 200, TestName = "Should return success with valid guild")]
    [TestCase("9A1367B1-1C7C-448E-88DA-DCAD275669FE", "2", 400, TestName = "Should return failure with invalid guild")]
    [TestCase("9A1367B1-1C7C-448E-88DA-DCAD275669FE", null, 400, TestName = "Should return failure without guild")]
    [TestCase("71E8F4BC-1B08-4DA8-B411-88F4BA1B3238", "1", 400,
        TestName = "Should return failure if user already has a guild")]
    public void PutUserTest(string telegramId, string guild, int statusCode)
    {
        UserRepositoryService userRepositoryService = new UserRepositoryService(_dbContext);
        userRepositoryService.Add(new User
        {
            Guild = null,
            Code = "",

            SpawnX = 0,
            SpawnY = 0,

            TelegramId = "9A1367B1-1C7C-448E-88DA-DCAD275669FE",
            FirstName = "",
            LastName = "",
            Username = "",
            PhotoUrl = "",
            AuthDate = "",
            Hash = ""
        });

        userRepositoryService.Add(new User
        {
            Guild = new Guild { Color = 1 },
            Code = "",

            SpawnX = 0,
            SpawnY = 0,

            TelegramId = "71E8F4BC-1B08-4DA8-B411-88F4BA1B3238",
            FirstName = "",
            LastName = "",
            Username = "",
            PhotoUrl = "",
            AuthDate = "",
            Hash = ""
        });

        GuildRepositoryService guildRepositoryService = new GuildRepositoryService(_dbContext);
        guildRepositoryService.Add(new Guild { Color = 1 });

        User? user = userRepositoryService.GetByTelegramId(telegramId);

        UserController controller =
            new UserController(_jwtService,
                userRepositoryService,
                guildRepositoryService)
            {
                ControllerContext =
                {
                    HttpContext = new DefaultHttpContext
                    {
                        Items = new Dictionary<object, object>
                        {
                            {
                                ClaimName,
                                new JwtClaim
                                {
                                    CoordinateOffset = new Coordinate { X = 0, Y = 0 },
                                    GuildId = null,
                                    Id = user!.Id
                                }
                            }
                        }!
                    }
                }
            };

        PutUsersInput input = new PutUsersInput { Guild = guild };
        IStatusCodeActionResult? result = controller.PutUsers(input) as IStatusCodeActionResult;
        result?.StatusCode.Should().Be(statusCode);
    }
}