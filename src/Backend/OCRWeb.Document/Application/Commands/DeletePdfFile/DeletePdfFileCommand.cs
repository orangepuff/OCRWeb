using MediatR;

namespace OCRWeb.Document.Application.Commands.DeletePdfFile;

/// <summary>Delete a PDF file owned by the current user.</summary>
public record DeletePdfFileCommand(Guid Id) : IRequest;
