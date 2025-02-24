using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Titeenipeli.Common.Database;
using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Database.Services;
using Titeenipeli.Common.Enums;
using Titeenipeli.Controllers;
using Titeenipeli.Inputs;
using Titeenipeli.Options;
using Titeenipeli.Services;
using Titeenipeli.Services.Grpc;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace Titeenipeli.IntegrationTest.Controllers;

[TestFixture]
public class CtfControllerIntegrationTest : BaseFixture
{
    [SetUp]
    public async Task BeforeEach()
    {
        _dbContext = new ApiDbContext(new DbContextOptionsBuilder().UseNpgsql(Postgres.GetConnectionString()).Options);
        await _dbContext.Database.EnsureCreatedAsync();
    }

    [OneTimeTearDown]
    public async Task AfterAll()
    {
        await _dbContext.Database.EnsureDeletedAsync();
        await _dbContext.DisposeAsync();
    }

    private readonly ApiDbContext _dbContext =
        new ApiDbContext(new DbContextOptionsBuilder().UseNpgsql(Postgres.GetConnectionString()).Options);

    [TestCase("#TEST_FLAG", 200, TestName = "Should return success code for valid flag")]
    [TestCase("#INVALID_FLAG", 400, TestName = "Should return failure code for invalid flag")]
    [TestCase(null, 400, TestName = "Should return failure code for null flag")]
    public void Test1(string? token, int statusCode)
    {
        var ctfFlagRepositoryService = new CtfFlagRepositoryService(_dbContext);
        var userRepositoryService = new UserRepositoryService(_dbContext);
        var guildRepositoryService = new GuildRepositoryService(_dbContext);
        var powerUpService = new PowerupService(new GameOptions());
        var jwtService = new JwtService(new JwtOptions());
        var miscGameStateUpdateCoreService = new MiscGameStateUpdateCoreService(powerUpService, new Logger<StateUpdateService>(new LoggerFactory()));

        var guild = GenerateGuild();
        var user = GenerateUser(guild);

        guildRepositoryService.Add(guild);
        guildRepositoryService.SaveChanges();
        userRepositoryService.Add(user);
        userRepositoryService.SaveChanges();
        jwtService.CreateJwtClaim(user);

        ctfFlagRepositoryService.Add(new CtfFlag { Token = "#TEST_FLAG" });
        ctfFlagRepositoryService.SaveChanges();

        var jwtClaim = jwtService.CreateJwtClaim(user);
        var httpcontext = new DefaultHttpContext();
        httpcontext.Items[jwtService.GetJwtClaimName()] = jwtClaim;

        CtfController ctfController = new CtfController(ctfFlagRepositoryService, userRepositoryService, guildRepositoryService, jwtService, miscGameStateUpdateCoreService);
        ctfController.ControllerContext = new ControllerContext
        {
            HttpContext = httpcontext
        };

        PostCtfInput input = new PostCtfInput { Token = token! };


        IStatusCodeActionResult? result = ctfController.PostCtf(input) as IStatusCodeActionResult;
        result?.StatusCode.Should().Be(statusCode);
    }


    private Guild GenerateGuild()
    {
        return new Guild
        {
            Name = GuildName.Cluster,
        };
    }

    private User GenerateUser(Guild guild)
    {
        return new User
        {
            Guild = guild,
            Code = "user",
            SpawnX = 0,
            SpawnY = 0,
            TelegramId = "0",
            FirstName = "",
            LastName = "",
            Username = "",
        };
    }
}