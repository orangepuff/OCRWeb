using MediatR;

namespace OCRWeb.DocumentProcessing.Application.Commands.UploadPdf;

/// <summary>Store an uploaded original PDF; returns the new file id.</summary>
public record UploadPdfCommand(Guid ProjectId, string FileName, string ContentType, byte[] Content)
    : IRequest<Guid>;
