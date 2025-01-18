using Titeenipeli.Common.Database.Schema;

namespace Titeenipeli.Common.Results;

public class GetCurrentUserResult
{
    public int Id { get; init; }
    public string Username { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public int Guild { get; init; }


    public GetCurrentUserResult(User user)
    {
        Id = user.Id;
        Username = user.Username;
        FirstName = user.FirstName;
        LastName = user.LastName;
        Guild = user.Guild.Id;
    }
}