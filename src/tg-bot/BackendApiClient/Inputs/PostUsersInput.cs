using Telegram.Bot.Types;

namespace TiteenipeliBot.BackendApiClient.Inputs;

public class PostUsersInput(User user)
{
    public string TelegramId { get; init; } = user.Id.ToString();
    public string FirstName { get; init; } = user.FirstName;
    public string LastName { get; init; } = user.LastName ?? "";
    public string Username { get; init; } = user.Username ?? "";
    public string? Guild { get; set; }
}