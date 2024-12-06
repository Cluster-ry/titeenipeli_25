using ICSharpCode.SharpZipLib.Core;
using Microsoft.AspNetCore.Http;
using Titeenipeli.Common.Database;
using Titeenipeli.Common.Database.Schema;
using Titeenipeli.Common.Database.Services;
using Titeenipeli.Controllers;
using Titeenipeli.Inputs;
using Titeenipeli.Services;

namespace Titeenipeli.IntegrationTest.Controllers;

[TestFixture]
public class CtfControllerIntegrationTest : BaseFixture
{
    [SetUp]
    public async Task BeforeEach()
    {
        await _dbContext.Database.EnsureCreatedAsync();
    }

    private readonly ApiDbContext _dbContext =
        new ApiDbContext(new DbContextOptionsBuilder().UseNpgsql(Postgres.GetConnectionString()).Options);

    [TestCase("#TEST_FLAG", 200, TestName = "Should return success code for valid flag")]
    [TestCase("#INVALID_FLAG", 400, TestName = "Should return failure code for invalid flag")]
    [TestCase(null, 400, TestName = "Should return failure code for null flag")]
    public void Test1(string token, int statusCode)
    {
        CtfFlagRepositoryService ctfFlagRepositoryService = new CtfFlagRepositoryService(_dbContext);
        UserRepositoryService userRepositoryService = new UserRepositoryService(_dbContext);
        GuildRepositoryService guildRepositoryService = new GuildRepositoryService(_dbContext);
        JwtService jwtService = new JwtService(new());

        var guild = GenerateGuild();
        var user = GenerateUser(guild);

        guildRepositoryService.Add(guild);
        userRepositoryService.Add(user);
        jwtService.CreateJwtClaim(user);

        ctfFlagRepositoryService.Add(new CtfFlag { Token = "#TEST_FLAG" });

        var jwtClaim = jwtService.CreateJwtClaim(user);
        var httpcontext = new DefaultHttpContext();
        httpcontext.Items[jwtService.GetJwtClaimName()] = jwtClaim;

        CtfController ctfController = new CtfController(ctfFlagRepositoryService, userRepositoryService, guildRepositoryService, jwtService);
        ctfController.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
        {
            HttpContext = httpcontext
        };

        PostCtfInput input = new PostCtfInput { Token = token };


        IStatusCodeActionResult? result = ctfController.PostCtf(input) as IStatusCodeActionResult;
        result?.StatusCode.Should().Be(statusCode);
    }


    private Guild GenerateGuild()
    {
        return new Guild
        {
            Name = Common.Enums.GuildName.Cluster,
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