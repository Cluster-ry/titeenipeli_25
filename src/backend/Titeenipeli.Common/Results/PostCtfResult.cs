namespace Titeenipeli.Common.Results;

public sealed class PostCtfResult
{
    public required String Title { get; init; }
    public required String Message { get; init; }
    public required List<String> Benefits { get; init; }
}