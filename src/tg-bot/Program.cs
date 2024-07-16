using Telegram.Bot;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Titeenipeli_bot
{
    class TgBot
    {
        // Variables
        private static string token;
        private static TelegramBotClient bot;

        static void Main(string[] args)
        {
            // Initializing the bot with its token
            IConfiguration configuration = new ConfigurationBuilder()
                .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
                .Build();

            token = configuration["token"];
            if (token is null) {
                Console.WriteLine("Unable to find token, exiting...");
                return;
            }

            bot = new TelegramBotClient(token);
            Handlers handlers = new(bot);
            // Runnin the bot
            using CancellationTokenSource cts = new CancellationTokenSource();

                // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool, so we use cancellation token
                bot.StartReceiving(
                    updateHandler: handlers.HandleUpdate,
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
}
