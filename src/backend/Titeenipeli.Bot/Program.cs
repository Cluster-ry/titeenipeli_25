using System.Reflection;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Titeenipeli.Bot.Options;

namespace Titeenipeli.Bot;

public static class Program
{
    static ManualResetEvent _quitEvent = new ManualResetEvent(false);

    private static void Main(string[] args)
    {
        var telegramOptions = new TelegramOptions();
        var backendOptions = new BackendOptions();
        var configurationRoot = new ConfigurationManager()
                                .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
                                .AddJsonFile("appsettings.json")
                                .AddEnvironmentVariables()
                                .Build();

        configurationRoot.GetSection("Telegram").Bind(telegramOptions);
        configurationRoot.GetSection("Backend").Bind(backendOptions);

        if (string.IsNullOrEmpty(telegramOptions.Token))
        {
            Console.WriteLine("Unable to get Telegram bot token, exiting...");
            return;
        }

        if (string.IsNullOrEmpty(backendOptions.BackendUrl))
        {
            Console.WriteLine("Unable to get backend URL, exiting...");
            return;
        }

        if (string.IsNullOrEmpty(backendOptions.Token))
        {
            Console.WriteLine("Unable to get backend token, exiting...");
            return;
        }

        Console.CancelKeyPress += (_, eventArguments) =>
        {
            _quitEvent.Set();
            eventArguments.Cancel = true;
        };

        TelegramBotClient bot = new TelegramBotClient(telegramOptions.Token);
        Handlers handlers = new Handlers(bot, backendOptions);
        using CancellationTokenSource cts = new CancellationTokenSource();

        // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool, so we use cancellation token
        bot.StartReceiving(
            handlers.HandleUpdate,
            errorHandler: handlers.HandleError,
            cancellationToken: cts.Token
        );

        // Tell the user the bot is online
        Console.WriteLine("TiteenipeliBot is running and is listening for updates. Press Ctrl-C to stop");
        _quitEvent.WaitOne();

        // Send cancellation request to stop the bot
        cts.Cancel();
    }
}