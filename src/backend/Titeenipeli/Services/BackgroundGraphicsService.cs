using SkiaSharp;
using Titeenipeli.Common.Models;

namespace Titeenipeli.Services;

public class BackgroundGraphicsService : IBackgroundGraphicsService
{
    private const int ImageChunkSize = 32;

    private readonly int _splitBitmapWidth;
    private readonly int _splitBitmapHeight;
    private readonly byte[,][] _splitBitmaps;

    public BackgroundGraphicsService()
    {
        var image = SKImage.FromEncodedData("Resources/Images/Background.jpg");
        var bitmap = SKBitmap.FromImage(image);
        byte[]? allBytes = bitmap.Bytes;

        _splitBitmapWidth = bitmap.Width / 32;
        _splitBitmapHeight = bitmap.Width / 32;
        _splitBitmaps = new byte[_splitBitmapWidth, _splitBitmapHeight][];
        int bytesPerPixel = bitmap.BytesPerPixel;
        int rowSize = bitmap.Width * bytesPerPixel;
        int sliceSize = ImageChunkSize * bytesPerPixel;
        int chunkSize = ImageChunkSize * ImageChunkSize * bytesPerPixel;

        for (int y = 0; y < _splitBitmapHeight; y++)
        {
            for (int x = 0; x < _splitBitmapWidth; x++)
            {
                byte[] chunk = new byte[chunkSize];
                for (int sliceIndex = 0; sliceIndex < ImageChunkSize; sliceIndex++)
                {
                    int beginIndex = x * ImageChunkSize * bytesPerPixel + y * rowSize * ImageChunkSize + sliceIndex * rowSize;
                    byte[] chunkSlice = allBytes.Skip(beginIndex).Take(sliceSize).ToArray();
                    int chunkIndex = sliceIndex * sliceSize;
                    chunkSlice.CopyTo(chunk, chunkIndex);
                }
                _splitBitmaps[x, y] = chunk;
            }
        }
    }

    public byte[]? GetBackgroundGraphic(Coordinate coordinate)
    {
        if (coordinate.X < 0 || coordinate.X >= _splitBitmapWidth || coordinate.Y < 0 || coordinate.Y >= _splitBitmapHeight)
        {
            return null;
        }

        return _splitBitmaps[coordinate.X, coordinate.Y];
    }
}