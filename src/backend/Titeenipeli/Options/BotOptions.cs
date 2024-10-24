namespace Titeenipeli.Options;

public class BotOptions
{
    public string AuthorizationHeaderName { get; init; } = "";
    public string Token { get; init; } = "";
    public int LoginTokenExpirationInMinutes { get; init; }
}