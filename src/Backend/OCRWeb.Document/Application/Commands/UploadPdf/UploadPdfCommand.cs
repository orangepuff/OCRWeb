using MediatR;

namespace OCRWeb.Document.Application.Commands.UploadPdf;

/// <summary>
/// Store an uploaded original PDF; returns the new file id.
/// </summary>
public record UploadPdfCommand(int ProjectId, string FileName, string ContentType, byte[] Content) : IRequest<int>;
