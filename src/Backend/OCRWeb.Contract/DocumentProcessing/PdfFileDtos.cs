namespace OCRWeb.Contract.DocumentProcessing;

/// <summary>Lightweight list item — metadata only, safe to return for many files.</summary>
public record PdfFileListItemDto(
    Guid Id,
    Guid ProjectId,
    string FileName,
    string ContentType,
    long SizeBytes,
    string FileType,
    DateTime InsertedTime);

/// <summary>Crop / section parameters.</summary>
public record FilePropertiesDto(int PageNo, int CropX, int CropY, int Width, int Height);

/// <summary>Full metadata for a single file (still no binary content).</summary>
public record PdfFileDetailDto(
    Guid Id,
    Guid ProjectId,
    string FileName,
    string ContentType,
    long SizeBytes,
    string FileType,
    FilePropertiesDto? Properties,
    int InsertedUserId,
    DateTime InsertedTime,
    int? UpdatedUserId,
    DateTime? UpdatedTime);

/// <summary>Binary payload for download/streaming.</summary>
public record PdfFileContentDto(byte[] Content, string ContentType, string FileName);

/// <summary>Request body for cropping an existing PDF into a new derived file.</summary>
public record CropPdfRequest(int PageNo, int CropX, int CropY, int Width, int Height, string? FileName);
