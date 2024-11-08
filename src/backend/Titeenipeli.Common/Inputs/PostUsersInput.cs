namespace Titeenipeli.Common.Inputs;

public class PostUsersInput
{
    public required string TelegramId { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Username { get; init; }
    public string? Guild { get; init; }
}