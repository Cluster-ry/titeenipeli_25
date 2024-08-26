using Telegram.Bot;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Titeenipeli_bot;

internal class TgBot
{
    // Variables
    private static string? token;
    private static TelegramBotClient? bot;
    private static string? uri;

    private static void Main(string[] args)
    {
        // Initializing the bot with its token
        IConfiguration configuration = new ConfigurationBuilder()
                                       .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
                                       .AddJsonFile("appsettings.json")
                                       .Build();

        token = configuration["token"];
        uri = configuration["uri"];
        if (token is null)
        {
            Console.WriteLine("Unable to find token, exiting...");
            return;
        }

        if (string.IsNullOrEmpty(uri))
        {
            Console.WriteLine("Unable to set uri, exiting...");
            return;
        }

        bot = new TelegramBotClient(token);
        Handlers handlers = new Handlers(bot, uri);
        using CancellationTokenSource cts = new CancellationTokenSource();

        // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool, so we use cancellation token
        bot.StartReceiving(
            handlers.HandleUpdate,
            errorHandler: handlers.HandleError,
            cancellationToken: cts.Token
        );

        // Tell the user the bot is online
        Console.WriteLine("TiteenipeliBot is running and is listening for updates. Press enter to stop");
        Console.ReadLine();

        // Send cancellation request to stop the bot
        cts.Cancel();
    }
}