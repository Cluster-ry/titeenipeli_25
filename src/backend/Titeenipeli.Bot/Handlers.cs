using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Titeenipeli.Bot.BackendApiClient;
using Titeenipeli.Bot.Options;
using Titeenipeli.Common.Enums;
using Titeenipeli.Common.Inputs;

namespace Titeenipeli.Bot;

public class Handlers(TelegramBotClient bot, BackendOptions backendOptions)
{
    // Variables
    private static readonly Dictionary<GuildName, string> GuildDict = new Dictionary<GuildName, string>
    {
        {
            GuildName.Cluster, "Cluster (lappeen Ranta)"
        },
        {
            GuildName.OulunTietoteekkarit, "Otit (Oulu)"
        },
        {
            GuildName.Digit, "Digit (Turku)"
        },
        {
            GuildName.Date, "Date (Turku)"
        },
        {
            GuildName.Tietokilta, "Tik (Otaniemi)"
        },
        {
            GuildName.Algo, "Algo (Jyväskylä)"
        },
        {
            GuildName.Tutti, "Tutti (Vaasa)"
        },
        {
            GuildName.Sosa, "Sosa (Lahti)"
        },
        {
            GuildName.TietoTeekkarikilta, "TiTe (Tampere)"
        },
        {
            GuildName.Datateknologerna, "Datateknologerna (Åbo)"
        }
    };

    // Pre-assign menu text
    private const string GuildMenuText = "<b>Please choose your guild from the menu below.</b>";

    // Pre-assign button text
    private const string AcceptButton = "I Accept";

    // pre-assigned Keyboard buttons
    private static readonly KeyboardButton ClusterButton = new(GuildDict[GuildName.Cluster]);
    private static readonly KeyboardButton OtitButton = new(GuildDict[GuildName.OulunTietoteekkarit]);
    private static readonly KeyboardButton DigitButton = new(GuildDict[GuildName.Digit]);
    private static readonly KeyboardButton DateButton = new(GuildDict[GuildName.Date]);
    private static readonly KeyboardButton TikButton = new(GuildDict[GuildName.Tietokilta]);
    private static readonly KeyboardButton AlgoButton = new(GuildDict[GuildName.Algo]);
    private static readonly KeyboardButton TuttiButton = new(GuildDict[GuildName.Tutti]);
    private static readonly KeyboardButton SosaButton = new(GuildDict[GuildName.Sosa]);
    private static readonly KeyboardButton TiTeButton = new(GuildDict[GuildName.TietoTeekkarikilta]);
    private static readonly KeyboardButton ÅboButton = new(GuildDict[GuildName.Datateknologerna]);

    // Build keyboards
    private static readonly ReplyKeyboardMarkup GuildKeyboard = new ReplyKeyboardMarkup(
    [ // This layout matches with how the keyboard is shown to the user
        [ClusterButton, ÅboButton],
        [OtitButton, DigitButton],
        [DateButton, TikButton],
        [AlgoButton, TuttiButton],
        [SosaButton, TiTeButton]
    ]);

    // Each time a user interacts with the bot, this method is called
    public async Task HandleUpdate(ITelegramBotClient _, Update update, CancellationToken __)
    {
        switch (update.Type)
        {
            // A message was received
            case UpdateType.Message:
                if (update.Message != null)
                {
                    await HandleMessage(update.Message);
                }
                break;

            // A button was pressed
            case UpdateType.CallbackQuery:
                if (update.CallbackQuery != null)
                {
                    await HandleButton(update.CallbackQuery);
                }
                break;
        }
    }

    public async Task HandleError(ITelegramBotClient _, Exception exception, CancellationToken __)
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
            GuildName chosenGuild = GuildDict.FirstOrDefault(x => x.Value == text).Key;
            await SendGuildData(user, chosenGuild);
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
            case "/start" or "/login":
                await HandleUserCreationOrLogin(user);
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

        await HandleUserCreationOrLogin(query.From);
    }

    private async Task HandleUserCreationOrLogin(User user)
    {
        try
        {
            BackendClient apiClient = new(backendOptions);

            var userInput = new PostUsersInput
            {
                TelegramId = user.Id.ToString(),
                FirstName = user.FirstName,
                LastName = user.LastName ?? "",
                Username = user.Username ?? ""
            };

            string? token = await apiClient.CreateUserOrLoginRequest(userInput);

            // if results is not a 0, a guild must be set
            if (token != null)
            {
                await SendGame(user, token);
            }
            else
            {
                // success message
                await bot.SendTextMessageAsync(
                    user.Id,
                    "User created! Now choose your guild.",
                    replyMarkup: null
                );

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

    private async Task SendGuildData(User user, GuildName guild)
    {
        try
        {
            BackendClient apiClient = new(backendOptions);

            var userInput = new PostUsersInput
            {
                TelegramId = user.Id.ToString(),
                FirstName = user.FirstName,
                LastName = user.LastName ?? "",
                Username = user.Username ?? "",
                Guild = guild.ToString()
            };

            string token = await apiClient.CreateUserOrLoginRequest(userInput) ??
                           throw new Exception("Token was unexpectedly null.");
            await SendGame(user, token);
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

    private async Task SendGame(User user, string token)
    {
        string loginUrl = $"{backendOptions.FrontendUrl}?token={token}";
        await bot.SendTextMessageAsync(
            user.Id,
            $"Open the following link to enter the game:\n\n" +
            $"<a href=\"{loginUrl}\">{loginUrl}</a>",
            parseMode: ParseMode.Html
        );
    }
}