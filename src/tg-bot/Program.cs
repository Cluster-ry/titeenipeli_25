using System.Collections.Generic;
using System.Formats.Asn1;
using System.Runtime.CompilerServices;
using System.Xml;
using DotNetEnv;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Titeenipeli_bot
{
    class TgBot
    {
        // Variables
        private static string token;
        private static TelegramBotClient bot;
        private Handlers handlers = new Handlers(bot);


        static void Main(string[] args)
        {
            // Initializing the bot with its token
            DotNetEnv.Env.Load();
            token = DotNetEnv.Env.GetString("tgtoken");
            bot = new TelegramBotClient(TgBot.token);
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