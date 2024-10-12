using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TiteenipeliBot.BackendApiClient;
using TiteenipeliBot.BackendApiClient.Inputs;

namespace TiteenipeliBot;

public class Handlers(TelegramBotClient bot, BackendOptions backendOptions)
{
    // Variables
    private readonly static Dictionary<GuildEnum, string> GuildDict = new Dictionary<GuildEnum, string>
    {
        {
            GuildEnum.Cluster, "Cluster (lappeen Ranta)"
        },
        {
            GuildEnum.OulunTietoteekkarit, "Otit (Oulu)"
        },
        {
            GuildEnum.Digit, "Digit (Turku)"
        },
        {
            GuildEnum.Date, "Date (Turku)"
        },
        {
            GuildEnum.Tietokilta, "Tik (Otaniemi)"
        },
        {
            GuildEnum.Algo, "Algo (Jyväskylä)"
        },
        {
            GuildEnum.Tutti, "Tutti (Vaasa)"
        },
        {
            GuildEnum.Sosa, "Sosa (Lahti)"
        },
        {
            GuildEnum.TietoTeekkarikilta, "TiTe (Tampere)"
        },
        {
            GuildEnum.Datateknologerna, "Datateknologerna (Åbo)"
        }
    };

    // Pre-assign menu text
    private const string GuildMenuText = "<b>Please choose your guild from the menu below.</b>";

    // Pre-assign button text
    private const string AcceptButton = "I Accept";

    // pre-assigned Keyboard buttons
    private readonly static KeyboardButton ClusterButton = new(GuildDict[GuildEnum.Cluster]);
    private readonly static KeyboardButton OtitButton = new(GuildDict[GuildEnum.OulunTietoteekkarit]);
    private readonly static KeyboardButton DigitButton = new(GuildDict[GuildEnum.Digit]);
    private readonly static KeyboardButton DateButton = new(GuildDict[GuildEnum.Date]);
    private readonly static KeyboardButton TikButton = new(GuildDict[GuildEnum.Tietokilta]);
    private readonly static KeyboardButton AlgoButton = new(GuildDict[GuildEnum.Algo]);
    private readonly static KeyboardButton TuttiButton = new(GuildDict[GuildEnum.Tutti]);
    private readonly static KeyboardButton SosaButton = new(GuildDict[GuildEnum.Sosa]);
    private readonly static KeyboardButton TiTeButton = new(GuildDict[GuildEnum.TietoTeekkarikilta]);
    private readonly static KeyboardButton ÅboButton = new(GuildDict[GuildEnum.Datateknologerna]);

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
            GuildEnum chosenGuild = GuildDict.FirstOrDefault(x => x.Value == text).Key;
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

            PostUsersInput userInput = new(user);
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

    private async Task SendGuildData(User user, GuildEnum guild)
    {
        try
        {
            BackendClient apiClient = new(backendOptions);

            PostUsersInput userInput = new(user)
            {
                Guild = guild.ToString()
            };
            string? token = await apiClient.CreateUserOrLoginRequest(userInput) ?? throw new Exception("Token was unexpectedly null.");
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
        string loginUrl = $"{backendOptions.Url}?token={token}";
        await bot.SendTextMessageAsync(
            user.Id,
            $"Open the following link to enter the game:\n\n" +
            $"<a href=\"{loginUrl}\">{loginUrl}</a>",
            parseMode: ParseMode.Html
        );
    }
}