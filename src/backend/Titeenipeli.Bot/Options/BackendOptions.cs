namespace Titeenipeli.Bot.Options;

public class BackendOptions
{
    public string BackendUrl { get; init; } = "";
    public string FrontendUrl { get; init; } = "";
    public string AuthorizationHeaderName { get; init; } = "";
    public string Token { get; init; } = "";
}