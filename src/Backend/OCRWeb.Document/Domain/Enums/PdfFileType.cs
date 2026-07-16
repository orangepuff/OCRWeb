namespace OCRWeb.Document.Domain.Enums;

/// <summary>
/// Kind of PDF stored. Validation of allowed values lives here (C# enum), not in the DB.
/// </summary>
public enum PdfFileType
{
    Original = 0,
    Cropped = 1,
    Section = 2
}
