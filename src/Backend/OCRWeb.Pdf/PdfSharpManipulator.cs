using OCRWeb.Pdf.Contract;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace OCRWeb.Pdf;

/// <summary>
/// PDFsharp-based adapter for <see cref="IPdfManipulator"/>.
/// Crop produces a new single-page PDF with the page's CropBox set to the requested area.
/// </summary>
public class PdfSharpManipulator : IPdfManipulator
{
    public byte[] Crop(byte[] source, PdfCropArea area)
    {
        ArgumentNullException.ThrowIfNull(source);

        using var inputStream = new MemoryStream(source);
        using var input = PdfReader.Open(inputStream, PdfDocumentOpenMode.Import);

        if (area.PageNo < 1 || area.PageNo > input.PageCount)
            throw new ArgumentOutOfRangeException(
                nameof(area), $"Page {area.PageNo} is out of range (1..{input.PageCount}).");

        using var output = new PdfDocument();
        var page = output.AddPage(input.Pages[area.PageNo - 1]);
        page.CropBox = new PdfRectangle(new XRect(area.X, area.Y, area.Width, area.Height));

        using var outputStream = new MemoryStream();
        output.Save(outputStream, closeStream: false);
        return outputStream.ToArray();
    }
}
