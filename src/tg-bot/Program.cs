using System.Formats.Asn1;
using System.Runtime.CompilerServices;
using System.Xml;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

// Variables
var tos_accepted = false; // I know its more of a privacy notice than tos but tos is easier to write :D
var user_created = false;
var choosing_guild = false;
string GuildChosen = "";
var GuildSelected = false;

// Pre-assign menu text
const string tosMenu =
    "<b>Welcome to Titeenipeli-bot!</b>\n\nIn order to play this years Titeenipeli, you have to consent to us using your Telegram nickname. This can be seen by other players in the pixels you've placed";
const string guildMenuText = "<b>Please choose your guild from the menu below.</b>";

// Pre-assign button text
const string acceptButton = "I Accept";

// pre-assigned Keyboard buttons
KeyboardButton ClusterButton = new("Cluster (Lappeen ranta)");
KeyboardButton OtitButton = new("Otit (Oulu)");
KeyboardButton DigitButton = new("Digit (Turku)");
KeyboardButton DateButton = new("Date (Turku)");
KeyboardButton TikButton = new("Tik (Otaniemi)");
KeyboardButton AlgoButton = new("Algo (Jyväskylä)");
KeyboardButton TuttiButton = new("Tutti (Vaasa)");
KeyboardButton SosaButton = new("Sosa (Lahti)");
KeyboardButton TiTeButton = new("TiTe (Tampere)");

// Build keyboards
InlineKeyboardMarkup tosMenuMarkup = new(InlineKeyboardButton.WithCallbackData(acceptButton));
ReplyKeyboardMarkup guildKeyboard =
    new(
        [ // This layout matches with how the keyboard is shown to the suer
            [ClusterButton],
            [OtitButton, DigitButton],
            [DateButton, TikButton],
            [AlgoButton, TuttiButton],
            [SosaButton, TiTeButton]
        ]
    );
guildKeyboard.OneTimeKeyboard = true;

// TODO: hide Token
var bot = new TelegramBotClient("7007777487:AAHVhkUG8OuTWn5urWbixmuGXOieNh1RO6o");

using var cts = new CancellationTokenSource();

// StartReceiving does not block the caller thread. Receiving is done on the ThreadPool, so we use cancellation token
bot.StartReceiving(
    updateHandler: HandleUpdate,
    errorHandler: HandleError,
    cancellationToken: cts.Token
);

// Tell the user the bot is online
Console.WriteLine("TiteenipeliBot is running and is listening for updates. Press enter to stop");
Console.ReadLine();

// Send cancellation request to stop the bot
cts.Cancel();

// Each time a user interacts with the bot, this method is called
async Task HandleUpdate(ITelegramBotClient _, Update update, CancellationToken cancellationToken)
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

async Task HandleError(
    ITelegramBotClient _,
    Exception exception,
    CancellationToken cancellationToken
)
{
    await Console.Error.WriteLineAsync(exception.Message);
}

async Task HandleMessage(Message msg)
{
    var user = msg.From;
    var text = msg.Text ?? string.Empty;

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
        await HandleCommand(user.Id, text);
    }
    else if (choosing_guild)
    {
        // loop through guild buttons NOTE: Is there a better way to do this?
        foreach (var row in guildKeyboard.Keyboard)
        {
            foreach (KeyboardButton button in row)
            {
                if (text == button.Text)
                {
                    GuildChosen = text;
                    await SendGuildData(user.Id, GuildChosen);
                    return;
                }
            }
        }
    }
    else
    {
        // Lastly, if the text has no meaning we tell user that we ain't chatGPT
        await bot.SendTextMessageAsync(user.Id, "Sorry, but I'm no chatbot. Use /start.");
    }
}

async Task HandleCommand(long userId, string command)
{
    // Here you can find every command and the accosiated method for running it
    switch (command)
    {
        case "/start":
            await SendTosMenu(userId);
            break;
        case "/guild":
            await SendGuildMenu(userId);
            break;
        case "/game":
            await SendGame(userId);
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
        if (tos_accepted)
        {
            return;
        }
        tos_accepted = true;
        Thread.Sleep(1 * 1000);

        await HandleUserSignup(query.Message.Chat.Id);
    }
    else
    {
        // this should never happen
        await bot.SendTextMessageAsync(query.Message!.Chat.Id, $"you pressed '{query}'");
    }
}

async Task SendTosMenu(long userId)
{
    // check if tos is already done, if so then skip
    if (tos_accepted)
    {
        await HandleUserSignup(userId);
        return;
    }

    await bot.SendTextMessageAsync(
        userId,
        tosMenu,
        parseMode: ParseMode.Html,
        replyMarkup: tosMenuMarkup
    );
}

async Task HandleUserSignup(long userid)
{
    // this prevents sequence breaks
    if (user_created)
    {
        await bot.SendTextMessageAsync(
            userid.ToString(),
            "User already created!",
            replyMarkup: null // doesen't remove the keyboard
        );
        return;
    }

    // TODO: User sign-in to db
    user_created = true;

    // success message
    await bot.SendTextMessageAsync(
        userid.ToString(),
        "User created! Now choose your guild.",
        replyMarkup: null
    );

    // After a small time window, jump straight to choosing your guild
    Thread.Sleep(2000);
    await SendGuildMenu(userid);
    return;
}

async Task SendGuildMenu(long userId)
{
    // this prevents sequence breaks
    if (!user_created)
    {
        await bot.SendTextMessageAsync(
            userId,
            "User not signed in. Please proceed with user creation with /start."
        );
        return;
    }

    if (GuildSelected)
    {
        // NOTE: could print the chosen guild?
        await bot.SendTextMessageAsync(userId, "Guild already chosen. Start the game with /game.");
        return;
    }

    // Starts guild selection check
    choosing_guild = true;

    // select guild message (with guild keyboard)
    await bot.SendTextMessageAsync(
        userId,
        guildMenuText,
        replyMarkup: guildKeyboard,
        parseMode: ParseMode.Html
    );
}

async Task SendGuildData(long userid, string guild)
{
    // TODO: send guid data to application
    choosing_guild = false;
    await bot.SendTextMessageAsync(
        userid,
        "Guild Selected! Now start the game with /game.",
        replyMarkup: new ReplyKeyboardRemove()
    );
    return;
}

async Task SendGame(long userId)
{
    // this prevents sequence breaks
    if (!user_created)
    {
        await bot.SendTextMessageAsync(
            userId,
            "User not signed in. Please proceed with user creation with /start."
        );
        return;
    }

    if (!GuildSelected)
    {
        await bot.SendTextMessageAsync(userId, "You haven't selected your guild yet. Use /guild.");
        return;
    }

    await bot.SendTextMessageAsync(userId, "Opening game...");

    // TODO: send to game with userid as param
}
