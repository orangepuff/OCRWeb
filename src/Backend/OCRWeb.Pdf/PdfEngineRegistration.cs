using Microsoft.Extensions.DependencyInjection;
using OCRWeb.Pdf.Contract;

namespace OCRWeb.Pdf;

/// <summary>
/// Registers the shared PDF engine (the PDFsharp adapter behind <see cref="IPdfManipulator"/>).
/// Called once from the composition root; any module can then depend on the port.
/// </summary>
public static class PdfEngineRegistration
{
    public static IServiceCollection AddPdfEngine(this IServiceCollection services)
    {
        services.AddSingleton<IPdfManipulator, PdfSharpManipulator>();
        return services;
    }
}
