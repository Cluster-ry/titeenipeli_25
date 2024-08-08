using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Titeenipeli_bot
{
    public class Handlers
    {
        // Variables
        string ip = "http://localhost:5129/api/v1"; // how to get this for production?
        static bool tosAccepted = false; // I know its more of a privacy notice than tos but tos is easier to write :D
        static private bool userCreated = false;
        static bool choosingGuild = false;
        guildEnum guildChosen;
        static bool guildSelected = false;
        private TelegramBotClient bot;
        enum guildEnum
        {
            Cluster,
            Otit,
            Digit,
            Date,
            Tik,
            Algo,
            Tutti,
            Sosa,
            TiTe
        };
        static Dictionary<guildEnum, string> guildDict =
            new()
            {
                { guildEnum.Cluster, "Cluster (lappeen Ranta)" },
                { guildEnum.Otit, "Otit (Oulu)" },
                { guildEnum.Digit, "Digit (Turku)" },
                { guildEnum.Date, "Date (Turku)" },
                { guildEnum.Tik, "Tik (Otaniemi)" },
                { guildEnum.Algo, "Algo (Jyväskylä)" },
                { guildEnum.Tutti, "Tutti (Vaasa)" },
                { guildEnum.Sosa, "Sosa (Lahti)" },
                { guildEnum.TiTe, "TiTe (Tampere)" },
            };

        // Pre-assign menu text
        const string tosMenu =
           "<b>Welcome to Titeenipeli-bot!</b>\n\nIn order to play this years Titeenipeli, you have to consent to us using your Telegram nickname. This can be seen by other players in the pixels you've placed";
        const string guildMenuText = "<b>Please choose your guild from the menu below.</b>";

        // Pre-assign button text
        const string acceptButton = "I Accept";

        // pre-assigned Keyboard buttons
        static KeyboardButton ClusterButton = new KeyboardButton(guildDict[guildEnum.Cluster]);
        static KeyboardButton OtitButton = new KeyboardButton(guildDict[guildEnum.Otit]);
        static KeyboardButton DigitButton = new KeyboardButton(guildDict[guildEnum.Digit]);
        static KeyboardButton DateButton = new KeyboardButton(guildDict[guildEnum.Date]);
        static KeyboardButton TikButton = new KeyboardButton(guildDict[guildEnum.Tik]);
        static KeyboardButton AlgoButton = new KeyboardButton(guildDict[guildEnum.Algo]);
        static KeyboardButton TuttiButton = new KeyboardButton(guildDict[guildEnum.Tutti]);
        static KeyboardButton SosaButton = new KeyboardButton(guildDict[guildEnum.Sosa]);
        static KeyboardButton TiTeButton = new KeyboardButton(guildDict[guildEnum.TiTe]);

        // Build keyboards
        static InlineKeyboardMarkup tosMenuMarkup = new(InlineKeyboardButton.WithCallbackData(acceptButton));
        protected static ReplyKeyboardMarkup guildKeyboard =
            new(
                [ // This layout matches with how the keyboard is shown to the user
                    [ClusterButton],
                    [OtitButton, DigitButton],
                    [DateButton, TikButton],
                    [AlgoButton, TuttiButton],
                    [SosaButton, TiTeButton]
                ]
            );

        public Handlers(TelegramBotClient bot)
        {
            this.bot = bot;
        }

        // Each time a user interacts with the bot, this method is called
        public async Task HandleUpdate(ITelegramBotClient _, Update update, CancellationToken cancellationToken)
        {
            switch (update.Type)
            {
                // A message was received
                case UpdateType.Message:
                    await HandleMessage(update.Message!);
                    break;

                // A button was pressed
                case UpdateType.CallbackQuery:
                    await HandleButton(update.CallbackQuery!);
                    break;
            }
        }

        public async Task HandleError(ITelegramBotClient _, Exception exception, CancellationToken cancellationToken)
        {
            await Console.Error.WriteLineAsync($"Exeption at the bot: '{exception.Message}'");
            throw new Exception();
        }

        async Task HandleMessage(Message msg)
        {
            User? user = msg.From ?? null;
            string text = msg.Text ?? string.Empty;

            if (user is null)
                return;

            // Print to console
            Console.WriteLine(
                $"[{DateTime.Now:dd-MM-yyyy HH:mm:ss}] User '{user.FirstName}' wrote '{text}'"
            );

            // TODO: here should come persistent data check

            // When we get a command, we react accordingly
            if (text.StartsWith('/'))
            {
                await HandleCommand(user, text);
            }
            else if (choosingGuild)
            {
                if (guildDict.ContainsValue(text))
                {
                    guildChosen = guildDict.FirstOrDefault(x => x.Value == text).Key;
                    await SendGuildData(user, (int) guildChosen); // changed for now since api takes the guild as int
                    return;
                }
                else
                {
                    await bot.SendTextMessageAsync(
                        user.Id,
                        "Unrecognized guild. Please select your guild by using the keyboard given."
                    );
                }
            }
            else
            {
                // Lastly, if the text has no meaning we tell user that we ain't chatGPT
                await bot.SendTextMessageAsync(user.Id, "Sorry, but I'm no chatbot. Use /start.");
            }
        }

        async Task HandleCommand(User user, string command)
        {
            // Here you can find every command and the accosiated method for running it
            switch (command)
            {
                case "/start":
                    await SendTosMenu(user);
                    break;
                case "/guild":
                    await SendGuildMenu(user);
                    break;
                case "/game":
                    await SendGame(user);
                    break;
            }

            await Task.CompletedTask;
        }

        async Task HandleButton(CallbackQuery query)
        {
            string text = string.Empty;
            InlineKeyboardMarkup markup = new(Array.Empty<InlineKeyboardButton>());

            if (query.Data == acceptButton)
            {
                // Close the query to end the client-side loading animation
                await bot.AnswerCallbackQueryAsync(query.Id);

                // Replace menu text and keyboard
                await bot.EditMessageTextAsync(
                    query.Message!.Chat.Id,
                    query.Message.MessageId,
                    "Thank you!\n\nYou are now being signed in!", // EXTRA:  better markup
                    parseMode: ParseMode.Html,
                    replyMarkup: markup
                );
                // no need to accept it twice
                if (tosAccepted)
                {
                    return;
                }
                tosAccepted = true;
                Thread.Sleep(1 * 1000);

                await HandleUserSignup(query.From);
            }
            else
            {
                // this should never happen
                await bot.SendTextMessageAsync(query.Message!.Chat.Id, $"you pressed '{query}'");
            }
        }

        async Task SendTosMenu(User user)
        {
            // check if tos is already done, if so then skip
            if (tosAccepted)
            {
                await HandleUserSignup(user);
                return;
            }

            await bot.SendTextMessageAsync(
                user.Id,
                tosMenu,
                parseMode: ParseMode.Html,
                replyMarkup: tosMenuMarkup
            );
        }

        async Task HandleUserSignup(User user)
        {
            // this prevents sequence breaks
            if (userCreated)
            {
                await bot.SendTextMessageAsync(
                    user.Id,
                    "User already created!",
                    replyMarkup: null // doesen't remove the keyboard
                );
                return;
            }

            UserProfilePhotos userPhotos = await bot.GetUserProfilePhotosAsync(user.Id,0,1);
            PhotoSize photo = userPhotos.Photos[0][0]; // with this you can only get the fileID

            Dictionary<string, string> json = new Dictionary<string, string>(){
                {"id", user.Id.ToString()},
                {"firstName", user.FirstName},
                {"lastName", user.LastName?? ""},
                {"username", user.Username?? ""},
                {"photoUrl", ""}, // the download URL includes the token, which shouldn't be sent (atleast without encryption)
                {"authDate", DateTime.Now.ToString()},
                {"hash", ""} // TODO: create hash
            };

            
            await Requests.CreateUserRequestAsync(ip, JsonConvert.SerializeObject(json));
            
            userCreated = true;

            // success message
            await bot.SendTextMessageAsync(
                user.Id,
                "User created! Now choose your guild.",
                replyMarkup: null
            );

            // After a small time window, jump straight to choosing your guild
            Thread.Sleep(1000);
            await SendGuildMenu(user);
            return;
        }

        async Task SendGuildMenu(User user)
        {
            // this prevents sequence breaks
            if (!userCreated)
            {
                await bot.SendTextMessageAsync(
                    user.Id,
                    "User not signed in. Please proceed with user creation with /start."
                );
                return;
            }

            if (guildSelected)
            {
                // NOTE: could print the chosen guild?
                await bot.SendTextMessageAsync(user.Id, "Guild already chosen. Start the game with /game.");
                return;
            }

            // Starts guild selection check
            choosingGuild = true;

            guildKeyboard.OneTimeKeyboard = true;
            // select guild message (with guild keyboard)
            await bot.SendTextMessageAsync(
                user.Id,
                guildMenuText,
                replyMarkup: guildKeyboard,
                parseMode: ParseMode.Html
            );
        }

        async Task SendGuildData(User user, int guild)
        {
            Dictionary<string, string> guildJson = new() {
                {"guild", guild.ToString()}
            };
            await Requests.SetGuildRequestAsync(ip, JsonConvert.SerializeObject(guildJson));

            choosingGuild = false;
            guildSelected = true;
            await bot.SendTextMessageAsync(
                user.Id,
                String.Format("You selected the guild {0}! Now start the game with /game.", guildChosen.ToString()),
                replyMarkup: new ReplyKeyboardRemove()
            );
            return;
        }

        async Task SendGame(User user)
        {
            // this prevents sequence breaks
            if (!userCreated)
            {
                await bot.SendTextMessageAsync(
                    user.Id,
                    "User not signed in. Please proceed with user creation with /start."
                );
                return;
            }

            if (!guildSelected)
            {
                await bot.SendTextMessageAsync(user.Id, "You haven't selected your guild yet. Use /guild.");
                return;
            }

            await bot.SendTextMessageAsync(user.Id, "Opening game...");

            // TODO: send to game with userid as param
        }
    }
}