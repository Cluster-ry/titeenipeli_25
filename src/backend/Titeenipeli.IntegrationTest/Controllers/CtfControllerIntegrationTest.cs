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

        ctfFlagRepositoryService.Add(new CtfFlag { Token = "#TEST_FLAG" });

        CtfController ctfController = new CtfController(ctfFlagRepositoryService, userRepositoryService, guildRepositoryService, jwtService);

        PostCtfInput input = new PostCtfInput { Token = token };

        IStatusCodeActionResult? result = ctfController.PostCtf(input) as IStatusCodeActionResult;
        result?.StatusCode.Should().Be(statusCode);
    }
}