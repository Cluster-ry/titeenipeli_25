using SkiaSharp;

namespace Titeenipeli.Services;

public class BackgroundGraphicsService : IBackgroundGraphicsService
{
    private const int _imageChunkSize = 32;

    private readonly int _splitBitmapWidth;
    private readonly int _splitBitmapHeight;
    private readonly byte[,][] _splitBitmaps;

    public BackgroundGraphicsService() {
        var image = SKImage.FromEncodedData(@"Resources/Images/Background.jpg");
        var bitmap = SKBitmap.FromImage(image);
        var allBytes = bitmap.Bytes;

        _splitBitmapWidth = bitmap.Width / 32;
        _splitBitmapHeight = bitmap.Width / 32;
        _splitBitmaps = new byte[_splitBitmapWidth, _splitBitmapHeight][];
        var pixelChunk = _imageChunkSize * _imageChunkSize * bitmap.BytesPerPixel;

        for (int y = 0; y < _splitBitmapHeight; y =+ _imageChunkSize)
        {
            for (int x = 0; x < _splitBitmapWidth; x =+ _imageChunkSize)
            {
                var beginIndex = x + y * bitmap.Width * bitmap.BytesPerPixel;
                var splitBytes = allBytes.Skip(beginIndex).Take(pixelChunk).ToArray();
                _splitBitmaps[x, y] = splitBytes;
            }
        }
    }

    public byte[] GetBackgroundGraphic(int x, int y) {
        if (x > _splitBitmapWidth || y > _splitBitmapHeight) {
            return _splitBitmaps[0, 0];
        }

        return _splitBitmaps[x, y];
    }
}