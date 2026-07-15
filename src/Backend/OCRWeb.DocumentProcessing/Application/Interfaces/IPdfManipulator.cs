namespace OCRWeb.DocumentProcessing.Application.Interfaces;

/// <summary>Area to crop, in PDF points, on a 1-based page.</summary>
public sealed record PdfCropArea(int PageNo, int X, int Y, int Width, int Height);

/// <summary>
/// Port for PDF binary manipulation. The implementation (PDFsharp) is an infrastructure adapter.
/// </summary>
public interface IPdfManipulator
{
    /// <summary>Produce a new single-page PDF cropped to <paramref name="area"/> of the source page.</summary>
    byte[] Crop(byte[] source, PdfCropArea area);
}
