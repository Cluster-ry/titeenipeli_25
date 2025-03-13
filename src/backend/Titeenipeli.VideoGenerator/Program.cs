using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Titeenipeli.Common.Results;
using Titeenipeli.Inputs;

namespace Titeenipeli.VideoGenerator;

public static class Program
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private static VideoSettings? VideoSettings { get; set; }
    private static readonly string FrameDirectory = "frames";

    public static async Task<int> Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json", false, true)
                            .Build();

        VideoSettings = configuration.GetSection("Video").Get<VideoSettings>();

        if (VideoSettings is null)
        {
            await Console.Error.WriteLineAsync("Missing Video section in appsettings.json");
            return -1;
        }

        Console.WriteLine("Titeenipeli Video Generator");

        if (args.Length is < 2 or > 3 || args[0] is not ("-f" or "-u"))
        {
            Console.WriteLine("Usage: ./Titeenipeli.VideoGenerator <options>");
            Console.WriteLine("Options:");
            Console.WriteLine("  -f: <input file>");
            Console.WriteLine("  -u: <url>");
            return -1;
        }

        List<Event>? events = [];

        switch (args[0])
        {
            case "-f":
                events = ReadFile(args[1]);
                break;
            case "-u":
                Console.WriteLine("God's authentication token: ");
                string token = Console.ReadLine() ?? string.Empty;
                events = await ReadFromUrl(args[1], token);
                break;
        }

        if (events is null)
        {
            await Console.Error.WriteLineAsync("No events found");
            return -1;
        }

        var startTime = DateTime.Now;
        GenerateFrames(events);
        Console.WriteLine($"Frame generation completed in {DateTime.Now - startTime}");
        GenerateVideo();
        Directory.Delete(FrameDirectory, true);

        return 0;
    }

    private static List<Event>? ReadFile(string filename)
    {
        using (var streamReader = new StreamReader(filename))
        {
            string json = streamReader.ReadToEnd();
            return JsonSerializer.Deserialize<GetEventsResult>(json, JsonSerializerOptions)?.Events;
        }
    }

    private static async Task<List<Event>?> ReadFromUrl(string url, string authenticationToken)
    {
        var address = new Uri(url);
        var cookieContainer = new CookieContainer();

        var clientHandler = new HttpClientHandler
        {
            CookieContainer = cookieContainer,
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
        };

        var httpClient = new HttpClient(clientHandler) { BaseAddress = address };

        string authenticateJson = JsonSerializer.Serialize(new PostAuthenticateInput { Token = authenticationToken });
        await httpClient.PostAsync("api/v1/users/authenticate",
            new StringContent(authenticateJson,
                Encoding.UTF8,
                "application/json"));

        var eventsResponse = await httpClient.GetAsync("api/v1/result/events");

        return JsonSerializer.Deserialize<GetEventsResult>(eventsResponse.Content.ReadAsStringAsync().Result,
            JsonSerializerOptions)?.Events;
    }

    private static void GenerateFrames(List<Event> events)
    {
        Directory.CreateDirectory(FrameDirectory);
        var image = new Image<Rgba32>(
            VideoSettings!.MapWidth * VideoSettings.PixelSize,
            VideoSettings.MapHeight * VideoSettings.PixelSize);

        var backgroundImage = Image.Load("Background.webp");
        backgroundImage.Mutate(x => x.Resize(
            VideoSettings.MapWidth * VideoSettings.PixelSize,
            VideoSettings.MapHeight * VideoSettings.PixelSize));

        int frameCount = 0;
        int totalFrames = (int)Math.Ceiling(VideoSettings.Duration.TotalSeconds * VideoSettings.FrameRate);

        var startTime = events[0].Timestamp;
        var endTime = events[^1].Timestamp;
        var frameDuration = (endTime - startTime) / totalFrames;

        for (var frameTime = startTime; frameTime <= endTime + frameDuration; frameTime += frameDuration)
        {
            Console.Write($"\rGenerating frame {frameCount} of {totalFrames}");
            var currentFrame = frameTime;
            var lastFrame = frameTime - frameDuration;
            var frameEvents = events.Where(@event =>
                @event.Timestamp > lastFrame &&
                @event.Timestamp <= currentFrame);

            foreach (var @event in frameEvents)
            {
                var rectangle = new Rectangle(
                    @event.Pixel.X * VideoSettings.PixelSize,
                    @event.Pixel.Y * VideoSettings.PixelSize,
                    VideoSettings.PixelSize,
                    VideoSettings.PixelSize);

                image.Mutate(context => context.Clear(GuildColor.GetGuildColor(@event.Guild), rectangle));
            }

            var frameImage = backgroundImage.CloneAs<Rgba32>();
            frameImage.Mutate(context => context.DrawImage(image, 1f));

            using (var file = File.OpenWrite(Path.Combine(
                       Environment.CurrentDirectory,
                       $"{FrameDirectory}/{frameCount++:0000000}.png")))
            {
                frameImage.SaveAsPng(file);
            }
        }

        Console.WriteLine();
    }

    private static void GenerateVideo()
    {
        using (var ffmpeg = new Process())
        {
            ffmpeg.StartInfo.FileName = "generate-video.sh";
            ffmpeg.StartInfo.WorkingDirectory = Environment.CurrentDirectory;
            ffmpeg.StartInfo.Arguments = $"{VideoSettings!.FrameRate} {FrameDirectory} out.mp4";
            ffmpeg.StartInfo.CreateNoWindow = true;
            ffmpeg.StartInfo.UseShellExecute = true;

            ffmpeg.Start();
            ffmpeg.WaitForExit();
        }
    }
}