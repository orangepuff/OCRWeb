namespace OCRWeb.DocumentProcessing.Domain.ValueObjects;

/// <summary>
/// Crop / section parameters for a derived PDF, persisted as JSON (column sFileProperties).
/// Kept so a cropped file can be re-edited from its previous position instead of from scratch.
/// Null for Original files.
/// </summary>
public class FileProperties
{
    public int PageNo { get; private set; }
    public int CropX { get; private set; }
    public int CropY { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }

    private FileProperties() { } // EF / JSON materialization

    public FileProperties(int pageNo, int cropX, int cropY, int width, int height)
    {
        if (pageNo < 1) throw new ArgumentOutOfRangeException(nameof(pageNo), "Page number is 1-based.");
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
        if (cropX < 0) throw new ArgumentOutOfRangeException(nameof(cropX));
        if (cropY < 0) throw new ArgumentOutOfRangeException(nameof(cropY));

        PageNo = pageNo;
        CropX = cropX;
        CropY = cropY;
        Width = width;
        Height = height;
    }
}
