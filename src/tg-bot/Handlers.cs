using System.Globalization;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Titeenipeli_bot
{
    public class Handlers(TelegramBotClient bot)
    {
        // Variables
        private readonly string _url = "http://localhost:5129"; // how to get this for production?
        private readonly List<UserData> _userList = new List<UserData>(); // TODO: Make this guy persistent?
        private UserData? _currentUser; 
        static readonly Dictionary<guildEnum, string> GuildDict =
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
        private const string TosMenu =
           "<b>Welcome to Titeenipeli-bot!</b>\n\nIn order to play this years Titeenipeli, you have to consent to us using your Telegram nickname. This can be seen by other players in the pixels you've placed";

        private const string GuildMenuText = "<b>Please choose your guild from the menu below.</b>";

        // Pre-assign button text
        private const string AcceptButton = "I Accept";

        // pre-assigned Keyboard buttons
        private static readonly KeyboardButton ClusterButton = new KeyboardButton(GuildDict[guildEnum.Cluster]);
        private static readonly KeyboardButton OtitButton = new KeyboardButton(GuildDict[guildEnum.Otit]);
        private static readonly KeyboardButton DigitButton = new KeyboardButton(GuildDict[guildEnum.Digit]);
        private static readonly KeyboardButton DateButton = new KeyboardButton(GuildDict[guildEnum.Date]);
        private static readonly KeyboardButton TikButton = new KeyboardButton(GuildDict[guildEnum.Tik]);
        private static readonly KeyboardButton AlgoButton = new KeyboardButton(GuildDict[guildEnum.Algo]);
        private static readonly KeyboardButton TuttiButton = new KeyboardButton(GuildDict[guildEnum.Tutti]);
        private static readonly KeyboardButton SosaButton = new KeyboardButton(GuildDict[guildEnum.Sosa]);
        private static readonly KeyboardButton TiTeButton = new KeyboardButton(GuildDict[guildEnum.TiTe]);

        // Build keyboards
        private static readonly InlineKeyboardMarkup TosMenuMarkup = new(InlineKeyboardButton.WithCallbackData(AcceptButton));

        private static readonly ReplyKeyboardMarkup GuildKeyboard =
            new(
                [ // This layout matches with how the keyboard is shown to the user
                    [ClusterButton],
                    [OtitButton, DigitButton],
                    [DateButton, TikButton],
                    [AlgoButton, TuttiButton],
                    [SosaButton, TiTeButton]
                ]
            );

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
            await Console.Error.WriteLineAsync($"Exception at the bot: '{exception.Message}'");
            throw new Exception();
        }

        async Task HandleMessage(Message msg)
        {
            User? user = msg.From ?? null;
            string text = msg.Text ?? string.Empty;

            if (user is null || text.Length <= 0)
                return;

            // Print to console
            Console.WriteLine(
                $"[{DateTime.Now:dd-MM-yyyy HH:mm:ss}] User '{user.FirstName}' wrote '{text}'"
            );

            _currentUser = _userList.Find(x => x.Id == user.Id);
            if (_currentUser == null)
            {
                _currentUser = new UserData(user.Id);
                _userList.Add(_currentUser);
            }

            // When we get a command, we react accordingly
            if (text.StartsWith('/'))
            {
                await HandleCommand(user, text);
            }
            else if (_currentUser.ChoosingGuild)
            {
                if (GuildDict.ContainsValue(text))
                {
                    _currentUser.GuildChosen = GuildDict.FirstOrDefault(x => x.Value == text).Key;
                    await SendGuildData(user, (int) _currentUser.GuildChosen); // changed since api takes the guild as int
                    return;
                }

                await bot.SendTextMessageAsync(
                    user.Id,
                    "Unrecognized guild. Please select your guild by using the keyboard given."
                );
            }
            else
            {
                // Lastly, if the text has no meaning we tell user that we ain't chatGPT
                await bot.SendTextMessageAsync(user.Id, "Sorry, but I'm no chatbot. Use /start.");
            }
        }

        async Task HandleCommand(User user, string command)
        {
            // Here you can find every command and the associated method for running it
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
            
            // InlineKeyboardMarkup markup = new(Array.Empty<InlineKeyboardButton>());

            if (query.Data == AcceptButton)
            {
                // Close the query to end the client-side loading animation
                await bot.AnswerCallbackQueryAsync(query.Id);

                // Replace menu text and keyboard
                await bot.EditMessageTextAsync(
                    query.Message!.Chat.Id,
                    query.Message.MessageId,
                    "Thank you!\n\nYou are now being signed in!", // EXTRA:  better markup
                    parseMode: ParseMode.Html,
                    replyMarkup: InlineKeyboardMarkup.Empty()
                );
                // no need to accept it twice
                if (_currentUser!.TosAccepted)
                {
                    return;
                }
                _currentUser.TosAccepted = true;
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
            if (_currentUser!.TosAccepted)
            {
                await HandleUserSignup(user);
                return;
            }

            await bot.SendTextMessageAsync(
                user.Id,
                TosMenu,
                parseMode: ParseMode.Html,
                replyMarkup: TosMenuMarkup
            );
        }

        async Task HandleUserSignup(User user)
        {
            // this prevents sequence breaks
            if (_currentUser!.UserCreated)
            {
                await bot.SendTextMessageAsync(
                    user.Id,
                    "User already created!",
                    replyMarkup: new ReplyKeyboardRemove()
                );
                return;
            }

            try
            {
                UserProfilePhotos userPhotos = await bot.GetUserProfilePhotosAsync(user.Id,0,1);
                PhotoSize dummy = userPhotos.Photos[0][0]; // with this you can only get the fileID
                // the download URL includes the token, which shouldn't be sent (at-least without encryption)
                // TODO: find a way to send photo without token 

                Dictionary<string, string> json = new Dictionary<string, string>
                {
                    {"id", user.Id.ToString()},
                    {"firstName", user.FirstName},
                    {"lastName", user.LastName?? ""},
                    {"username", user.Username?? ""},
                    {"photoUrl", ""}, 
                    {"authDate", DateTime.Now.ToString(CultureInfo.CurrentCulture)},
                    {"hash", ""} // TODO: create hash
                };

            
                await Requests.CreateUserRequestAsync(_url, JsonConvert.SerializeObject(json));
            
                _currentUser.UserCreated = true;

                // success message
                await bot.SendTextMessageAsync(
                    user.Id,
                    "User created! Now choose your guild.",
                    replyMarkup: null
                );

                // After a small-time window, jump straight to choosing your guild
                Thread.Sleep(1000);
                await SendGuildMenu(user);
            }
            catch
            {
                await bot.SendTextMessageAsync(
                    user.Id,
                    "There was an error with User signup. Please try again.",
                    replyMarkup: new ReplyKeyboardRemove()
                );
            }
        }

        async Task SendGuildMenu(User user)
        {
            // this prevents sequence breaks
            if (!_currentUser!.UserCreated)
            {
                await bot.SendTextMessageAsync(
                    user.Id,
                    "User not signed in. Please proceed with user creation with /start."
                );
                return;
            }

            if (_currentUser.GuildSelected)
            {
                // NOTE: could print the chosen guild?
                await bot.SendTextMessageAsync(user.Id, "Guild already chosen. Start the game with /game.");
                return;
            }

            // Starts guild selection check
            _currentUser.ChoosingGuild = true;

            GuildKeyboard.OneTimeKeyboard = true;
            // select guild message (with guild keyboard)
            await bot.SendTextMessageAsync(
                user.Id,
                GuildMenuText,
                replyMarkup: GuildKeyboard,
                parseMode: ParseMode.Html
            );
        }

        async Task SendGuildData(User user, int guild)
        {
            Dictionary<string, string> guildJson = new() {
                {"guild", guild.ToString()}
            };
            try
            {
                await Requests.SetGuildRequestAsync(_url, JsonConvert.SerializeObject(guildJson));
                _currentUser!.ChoosingGuild = false;
                _currentUser.GuildSelected = true;
                await bot.SendTextMessageAsync(
                    user.Id,
                    $"You selected the guild {_currentUser.GuildChosen.ToString()}! Now start the game with /game.",
                    replyMarkup: new ReplyKeyboardRemove()
                );
            }
            catch (Exception)
            {
                await bot.SendTextMessageAsync(
                    user.Id,
                    "There was an error with setting the guild. Please try again.",
                    replyMarkup: new ReplyKeyboardRemove()
                );
            }
        }

        async Task SendGame(User user)
        {
            // this prevents sequence breaks
            if (!_currentUser!.UserCreated)
            {
                await bot.SendTextMessageAsync(
                    user.Id,
                    "User not signed in. Please proceed with user creation with /start."
                );
                return;
            }

            if (!_currentUser.GuildSelected)
            {
                await bot.SendTextMessageAsync(user.Id, "You haven't selected your guild yet. Use /guild.");
                return;
            }

            AuthenticationHeaderValue? cookies = Requests.GetCookies();
            Console.WriteLine($"token: {cookies}");
            await bot.SendTextMessageAsync(
                user.Id,
                $"Open the following link to enter the game:\n\n" +
                $"{_url}?Authorization={cookies}" //TODO: Proper url
                );
        }
    }
}