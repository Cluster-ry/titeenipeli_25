using Microsoft.Extensions.Hosting.Internal;
using Moq;
using Titeenipeli.Common.Database;
using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Database.Services;
using Titeenipeli.Common.Enums;
using Titeenipeli.Common.Inputs;
using Titeenipeli.Controllers;
using Titeenipeli.Options;
using Titeenipeli.Services;

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

    private readonly JwtService _jwtService = new(new JwtOptions
    {
        ClaimName = ClaimName,
        CookieName = "Auth",
        Encryption = string.Join("", Enumerable.Repeat(0, 32).Select(_ => (char)new Random().Next(97, 122))),
        Secret = string.Join("", Enumerable.Repeat(0, 256).Select(_ => (char)new Random().Next(97, 122)))
    });

    private ApiDbContext _dbContext;

    [TestCase("19E0AD9B-E94C-44CC-A956-D094C7A4D58D", 200, TestName = "Should return success for new user")]
    [TestCase("9A81A0A0-1502-42B3-862E-50C2FF8B038F", 200, TestName = "Should return success for existing user")]
    public void PostUserTest(string telegramId, int statusCode)
    {
        var botOptions = new BotOptions();
        var gameOptions = new GameOptions();
        var userRepositoryService = new UserRepositoryService(_dbContext);

        userRepositoryService.Add(new User
        {
            Guild = new Guild { Name = GuildName.Tietokilta },
            Code = "",

            SpawnX = 0,
            SpawnY = 0,

            TelegramId = "9A81A0A0-1502-42B3-862E-50C2FF8B038F",
            FirstName = "",
            LastName = "",
            Username = ""
        });

        userRepositoryService.SaveChanges();


        var guildRepositoryService = new GuildRepositoryService(_dbContext);

        var userProvider = new UserProviderStub();
        userProvider.Initialize(userRepositoryService.GetAll());

        var mockMapUpdaterService = new Mock<IMapUpdaterService>();
        mockMapUpdaterService.Setup(service => service.PlaceSpawn(It.IsAny<User>()))
                             .Returns<User>(user => new Task<User>(() => user));


        var controller = new UserController(new HostingEnvironment(),
            botOptions,
            gameOptions,
            userProvider,
            userRepositoryService,
            guildRepositoryService,
            _jwtService,
            mockMapUpdaterService.Object)
        {
            ControllerContext =
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        var input = new PostUsersInput
        {
            TelegramId = telegramId,
            Username = "",
            FirstName = "",
            LastName = ""
        };

        // ReSharper disable once SuspiciousTypeConversion.Global
        var result = controller.PostUsers(input) as IStatusCodeActionResult;
        result?.StatusCode.Should().Be(statusCode);
    }
}