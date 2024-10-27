using Titeenipeli.Enumeration;

namespace Titeenipeli.Results;

public class ErrorResult
{
    public required string Title { get; init; }

    public required ErrorCode Code { get; init; }
    public required string Description { get; init; }
}