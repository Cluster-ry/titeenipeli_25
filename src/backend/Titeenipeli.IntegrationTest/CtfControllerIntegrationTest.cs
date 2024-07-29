using Titeenipeli.Context;
using Titeenipeli.Controllers;
using Titeenipeli.Inputs;
using Titeenipeli.Schema;
using Titeenipeli.Services.RepositoryServices;

namespace Titeenipeli.IntegrationTest;

[TestFixture]
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
public class Tests
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
                                                     .WithImage("postgres:15-alpine")
                                                     .Build();

    [SetUp]
    public async Task Setup()
    {
        await _postgres.StartAsync();
    }

    [TearDown]
    public async Task Cleanup()
    {
        await _postgres.DisposeAsync();
    }

    [TestCase("#TEST_TOKEN", ExpectedResult = 200)]
    [TestCase("#INVALID_TOKEN", ExpectedResult = 400)]
    public int? Test1(string token)
    {
        ApiDbContext dbContext =
            new ApiDbContext(new DbContextOptionsBuilder().UseNpgsql(_postgres.GetConnectionString()).Options);

        dbContext.Database.EnsureCreated();

        CtfFlagRepositoryService ctfFlagRepositoryRepositoryService =
            new CtfFlagRepositoryService(dbContext);

        ctfFlagRepositoryRepositoryService.Add(new CtfFlag { Token = "#TEST_TOKEN" });

        CtfController ctfController = new CtfController(ctfFlagRepositoryRepositoryService);

        PostCtfInput input = new PostCtfInput { Token = token };

        IStatusCodeActionResult? result = ctfController.PostCtf(input) as IStatusCodeActionResult;
        return result?.StatusCode;
    }
}