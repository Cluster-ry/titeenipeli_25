namespace Titeenipeli.Inputs;

public class PostUserInput
{
    public required string Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Username { get; init; }
    public required string PhotoUrl { get; init; }
    public required string AuthDate { get; init; }
    public required string Hash { get; init; }
}