namespace Titeenipeli.IntegrationTest;

[SetUpFixture]
public class BaseFixture
{
    protected readonly PostgreSqlContainer Postgres = new PostgreSqlBuilder()
                                                      .WithImage("postgres:15-alpine")
                                                      .Build();

    [OneTimeSetUp]
    public async Task Setup()
    {
        await Postgres.StartAsync();
    }

    [OneTimeTearDown]
    public async Task Cleanup()
    {
        await Postgres.DisposeAsync();
    }
}