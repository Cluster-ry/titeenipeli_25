using SkiaSharp;
using Titeenipeli.Common.Models;

namespace Titeenipeli.Services;

public class BackgroundGraphicsService : IBackgroundGraphicsService
{
    private const int _imageChunkSize = 32;

    private readonly int _splitBitmapWidth;
    private readonly int _splitBitmapHeight;
    private readonly byte[,][] _splitBitmaps;

    public BackgroundGraphicsService()
    {
        var image = SKImage.FromEncodedData(@"Resources/Images/Background.jpg");
        var bitmap = SKBitmap.FromImage(image);
        var allBytes = bitmap.Bytes;

        _splitBitmapWidth = bitmap.Width / 32;
        _splitBitmapHeight = bitmap.Width / 32;
        _splitBitmaps = new byte[_splitBitmapWidth, _splitBitmapHeight][];
        var bytesPerPixel = bitmap.BytesPerPixel;
        var rowSize = bitmap.Width * bytesPerPixel;
        var sliceSize = _imageChunkSize * bytesPerPixel;
        var chunkSize = _imageChunkSize * _imageChunkSize * bytesPerPixel;

        for (int y = 0; y < _splitBitmapHeight; y++)
        {
            for (int x = 0; x < _splitBitmapWidth; x++)
            {
                var chunk = new byte[chunkSize];
                for (int sliceIndex = 0; sliceIndex < _imageChunkSize; sliceIndex++)
                {
                    var beginIndex = x * _imageChunkSize * bytesPerPixel + y * rowSize * _imageChunkSize + sliceIndex * rowSize;
                    var chunkSlice = allBytes.Skip(beginIndex).Take(sliceSize).ToArray();
                    var chunkIndex = sliceIndex * sliceSize;
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