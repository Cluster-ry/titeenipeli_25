using Titeenipeli.Context;
using Titeenipeli.Controllers;
using Titeenipeli.Inputs;
using Titeenipeli.Schema;
using Titeenipeli.Services.RepositoryServices;

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

    private ApiDbContext _dbContext;

    [TestCase("#TEST_FLAG", 200, TestName = "Should return success code for valid flag")]
    [TestCase("#INVALID_FLAG", 400, TestName = "Should return failure code for invalid flag")]
    [TestCase(null, 400, TestName = "Should return failure code for null flag")]
    public void Test1(string token, int statusCode)
    {
        CtfFlagRepositoryService ctfFlagRepositoryService = new CtfFlagRepositoryService(_dbContext);
        ctfFlagRepositoryService.Add(new CtfFlag { Token = "#TEST_FLAG" });

        CtfController ctfController = new CtfController(ctfFlagRepositoryService);

        PostCtfInput input = new PostCtfInput { Token = token };

        IStatusCodeActionResult? result = ctfController.PostCtf(input) as IStatusCodeActionResult;
        result?.StatusCode.Should().Be(statusCode);
    }
}