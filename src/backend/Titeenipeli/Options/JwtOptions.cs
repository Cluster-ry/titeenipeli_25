namespace Titeenipeli.Options;

public class JwtOptions
{
    public string Secret { get; init; } = "";
    public string Encryption { get; init; } = "";
    public string ValidIssuer { get; init; } = "";
    public string ValidAudience { get; init; } = "";
    public string CookieName { get; init; } = "";
    public string ClaimName { get; init; } = "data";
    public int ExpirationDays { get; init; }
}