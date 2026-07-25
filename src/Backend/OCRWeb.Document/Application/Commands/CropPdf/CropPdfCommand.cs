using MediatR;

namespace OCRWeb.Document.Application.Commands.CropPdf;

/// <summary>Crop a source PDF's content in place; returns the same file's (now Cropped) id.</summary>
public record CropPdfCommand(
    Guid SourcePdfFileId,
    int PageNo,
    int CropX,
    int CropY,
    int Width,
    int Height,
    string? FileName) : IRequest<Guid>;
