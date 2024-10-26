namespace TiteenipeliBot.BackendApiClient.Results;

public class ErrorResult
{
    public required string Title { get; init; }
    public required int Code { get; init; }
    public required string Description { get; init; }
}