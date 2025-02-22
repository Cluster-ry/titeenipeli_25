namespace Titeenipeli.IntegrationTest;

[SetUpFixture]
public class BaseFixture
{
    protected static readonly PostgreSqlContainer Postgres = new PostgreSqlBuilder()
                                                             .WithImage("postgres:15-alpine")
                                                             .Build();

    [OneTimeSetUp]
    public static async Task Setup()
    {
        await Postgres.StartAsync();
    }

    [OneTimeTearDown]
    public static async Task Cleanup()
    {
        await Postgres.DisposeAsync();
    }
}