using MediatR;

namespace OCRWeb.Document.Application.Commands.CropPdf;

/// <summary>Crop a source PDF into a new derived (Cropped) file; returns the new file id.</summary>
public record CropPdfCommand(
    Guid SourcePdfFileId,
    int PageNo,
    int CropX,
    int CropY,
    int Width,
    int Height,
    string? FileName) : IRequest<Guid>;
