using Telegram.Bot;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using TiteenipeliBot.Options;

namespace TiteenipeliBot;

internal class TgBot
{
    private static void Main(string[] args)
    {
        TelegramOptions telegramOptions = new TelegramOptions();
        BackendOptions backendOptions = new BackendOptions();
        IConfigurationRoot configurationRoot = new ConfigurationManager()
                                       .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
                                       .AddJsonFile("appsettings.json")
                                       .Build();
        configurationRoot.GetSection("Telegram").Bind(telegramOptions);
        configurationRoot.GetSection("Backend").Bind(backendOptions);

        if (telegramOptions.Token is null)
        {
            Console.WriteLine("Unable to get Telegram bot token, exiting...");
            return;
        }

        if (string.IsNullOrEmpty(backendOptions.Url))
        {
            Console.WriteLine("Unable to get backend URL, exiting...");
            return;
        }

        if (string.IsNullOrEmpty(backendOptions.Token))
        {
            Console.WriteLine("Unable to get backend token, exiting...");
            return;
        }

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
        Console.WriteLine("TiteenipeliBot is running and is listening for updates. Press enter to stop");
        Console.ReadLine();

        // Send cancellation request to stop the bot
        cts.Cancel();
    }
}