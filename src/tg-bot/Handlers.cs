using System.Globalization;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Titeenipeli_bot;

public class Handlers(TelegramBotClient bot, string url)
{
    // Variables
    private readonly static Dictionary<guildEnum, string> GuildDict = new Dictionary<guildEnum, string>
    {
        {
            guildEnum.Cluster, "Cluster (lappeen Ranta)"
        },
        {
            guildEnum.OulunTietoteekkarit, "Otit (Oulu)"
        },
        {
            guildEnum.Digit, "Digit (Turku)"
        },
        {
            guildEnum.Date, "Date (Turku)"
        },
        {
            guildEnum.Tietokilta, "Tik (Otaniemi)"
        },
        {
            guildEnum.Algo, "Algo (Jyväskylä)"
        },
        {
            guildEnum.Tutti, "Tutti (Vaasa)"
        },
        {
            guildEnum.Sosa, "Sosa (Lahti)"
        },
        {
            guildEnum.TietoTeekkarikilta, "TiTe (Tampere)"
        },
        {
            guildEnum.Datateknologerna, "Datateknologerna (Åbo)"
        }
    };

    // Pre-assign menu text
    private const string GuildMenuText = "<b>Please choose your guild from the menu below.</b>";

    // Pre-assign button text
    private const string AcceptButton = "I Accept";

    // pre-assigned Keyboard buttons
    private readonly static KeyboardButton ClusterButton = new KeyboardButton(GuildDict[guildEnum.Cluster]);
    private readonly static KeyboardButton OtitButton = new KeyboardButton(GuildDict[guildEnum.OulunTietoteekkarit]);
    private readonly static KeyboardButton DigitButton = new KeyboardButton(GuildDict[guildEnum.Digit]);
    private readonly static KeyboardButton DateButton = new KeyboardButton(GuildDict[guildEnum.Date]);
    private readonly static KeyboardButton TikButton = new KeyboardButton(GuildDict[guildEnum.Tietokilta]);
    private readonly static KeyboardButton AlgoButton = new KeyboardButton(GuildDict[guildEnum.Algo]);
    private readonly static KeyboardButton TuttiButton = new KeyboardButton(GuildDict[guildEnum.Tutti]);
    private readonly static KeyboardButton SosaButton = new KeyboardButton(GuildDict[guildEnum.Sosa]);
    private readonly static KeyboardButton TiTeButton = new KeyboardButton(GuildDict[guildEnum.TietoTeekkarikilta]);
    private readonly static KeyboardButton ÅboButton = new KeyboardButton(GuildDict[guildEnum.Datateknologerna]);

    // Build keyboards
    private readonly static ReplyKeyboardMarkup GuildKeyboard = new ReplyKeyboardMarkup(
    [ // This layout matches with how the keyboard is shown to the user
        [ClusterButton, ÅboButton],
        [OtitButton, DigitButton],
        [DateButton, TikButton],
        [AlgoButton, TuttiButton],
        [SosaButton, TiTeButton]
    ]);

    // Each time a user interacts with the bot, this method is called
    public async Task HandleUpdate(ITelegramBotClient _, Update update, CancellationToken cancellationToken)
    {
        switch (update.Type)
        {
            // A message was received
            case UpdateType.Message:
                await HandleMessage(update.Message);
                break;

            // A button was pressed
            case UpdateType.CallbackQuery:
                await HandleButton(update.CallbackQuery);
                break;
        }
    }

    public async Task HandleError(ITelegramBotClient _, Exception exception, CancellationToken cancellationToken)
    {
        await Console.Error.WriteLineAsync($"Exception at the bot: '{exception.Message}'");
        throw new Exception();
    }

    private async Task HandleMessage(Message msg)
    {
        User? user = msg.From ?? null;
        string text = msg.Text ?? string.Empty;

        if (user is null || text.Length <= 0)
            return;

        // Print to console
        Console.WriteLine(
            $"[{DateTime.Now:dd-MM-yyyy HH:mm:ss}] User '{user.FirstName}' wrote '{text}'"
        );

        // When we get a command, we react accordingly
        if (text.StartsWith('/'))
        {
            await HandleCommand(user, text);
            return;
        }

        if (GuildDict.ContainsValue(text))
        {
            guildEnum chosenGuild = GuildDict.FirstOrDefault(x => x.Value == text).Key;
            await SendGuildData(user, (int)chosenGuild); // changed since api takes the guild as int
            return;
        }

        // Lastly, if the text has no meaning we tell user that we ain't chatGPT
        await bot.SendTextMessageAsync(user.Id, "Sorry, but I'm no chatbot. Use /start.");
    }

    private async Task HandleCommand(User user, string command)
    {
        // Here you can find every command and the associated method for running it
        switch (command)
        {
            case "/guild":
                await SendGuildMenu(user);
                break;
            case "/start" or "/game":
                await HandleUser(user);
                break;
        }

        await Task.CompletedTask;
    }

    private async Task HandleButton(CallbackQuery query)
    {
        // InlineKeyboardMarkup markup = new(Array.Empty<InlineKeyboardButton>());

        if (query.Data != AcceptButton)
        {
            // this should never happen
            await bot.SendTextMessageAsync(query.Message!.Chat.Id, $"you pressed '{query}'");
            return;
        }

        // Close the query to end the client-side loading animation
        await bot.AnswerCallbackQueryAsync(query.Id);

        // Replace menu text and keyboard
        await bot.EditMessageTextAsync(
            query.Message!.Chat.Id,
            query.Message.MessageId,
            "Thank you! Your guild has been set!",
            ParseMode.Html,
            replyMarkup: InlineKeyboardMarkup.Empty()
        );

        // quick break before spamming user with more messages
        // Thread.Sleep(1 * 1000);
        await HandleUser(query.From);
    }

    private async Task HandleUser(User user)
    {
        try
        {
            // this message prob should be updated and not just spam more messages.
            await bot.SendTextMessageAsync(
                user.Id,
                "Sending data to the game...",
                replyMarkup: null
            );

            UserProfilePhotos userPhotos = await bot.GetUserProfilePhotosAsync(user.Id, 0, 1);
            PhotoSize dummy = userPhotos.Photos[0][0]; // with this you can only get the fileID
            // the download URL includes the token, which shouldn't be sent (at-least without encryption)
            // TODO: find a way to send photo without token 

            Dictionary<string, string> json = new Dictionary<string, string>
            {
                {
                    "id", user.Id.ToString()
                },
                {
                    "firstName", user.FirstName
                },
                {
                    "lastName", user.LastName ?? ""
                },
                {
                    "username", user.Username ?? ""
                },
                {
                    "photoUrl", ""
                },
                {
                    "authDate", DateTime.Now.ToString(CultureInfo.CurrentCulture)
                },
                {
                    "hash", ""
                } // TODO: create hash
            };


            int result = await Requests.CreateUserRequestAsync(url, JsonConvert.SerializeObject(json));

            // if results is not a 0, a guild must be set
            if (result == 0)
            {
                await SendGame(user);
            }
            else
            {
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

    private async Task SendGuildMenu(User user)
    {
        GuildKeyboard.OneTimeKeyboard = true;
        // select guild message (with guild keyboard)
        await bot.SendTextMessageAsync(
            user.Id,
            GuildMenuText,
            replyMarkup: GuildKeyboard,
            parseMode: ParseMode.Html
        );
    }

    private async Task SendGuildData(User user, int guild)
    {
        Dictionary<string, string> guildJson = new Dictionary<string, string>
        {
            {
                "guild", guild.ToString()
            }
        };
        try
        {
            await Requests.SetGuildRequestAsync(url, JsonConvert.SerializeObject(guildJson));

            await bot.SendTextMessageAsync(
                user.Id,
                $"You selected your guild! Now start the game with /game.",
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

    private async Task SendGame(User user)
    {
        AuthenticationHeaderValue? header = Requests.GetAuthHeader(); // replace this with proper user token
        await bot.SendTextMessageAsync(
            user.Id,
            $"Open the following link to enter the game:\n\n" +
            $"{url}?token={header}" //TODO: Proper game url
        );
    }
}