namespace Titeenipeli.VideoGenerator;

public class VideoSettings
{
    public TimeSpan Duration { get; init; }
    public int FrameRate { get; init; }
    public int PixelSize { get; init; }
    public int MapWidth { get; init; }
    public int MapHeight { get; init; }
    public string OutputFile { get; init; } = "output.mp4";
    public DateTime StartTime { get; init; } = DateTime.MinValue;
    public DateTime EndTime { get; init; } = DateTime.MaxValue;
}