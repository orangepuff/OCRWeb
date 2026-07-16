# OCRWeb.Pdf

**Shared technical library — NOT a bounded context.** No DbContext, no `[schema]`, no domain
aggregates. It only manipulates PDF bytes/pages and knows nothing about documents, projects,
or OCR rules.

> Status: **in use.** `IPdfManipulator` + `PdfCropArea` live in `OCRWeb.Pdf.Contract`; the
> PDFsharp adapter `PdfSharpManipulator` lives here. `OCRWeb.Document` depends on the port
> (`OCRWeb.Pdf.Contract`); `OCRWeb.API` references the adapter and registers it. `OCRWeb.OCR`
> will depend on `OCRWeb.Pdf.Contract` once it needs page rendering.

## Why it is separate

Split / crop / render-page are *format mechanics*, not a business capability. Once a second
context needs them, sharing via a technical library keeps the modules decoupled:

```
OCRWeb.Pdf.Contract   ← port:    IPdfManipulator + PDF operation DTOs
        ▲
OCRWeb.Pdf            ← adapter: PDFsharp / PDFium implementation
        ▲                    ▲
OCRWeb.Document          OCRWeb.OCR
(crop/split → derived     (render page → crop region → run OCR)
 PdfFile aggregates)
```

- **OCRWeb.Pdf.Contract** — the `IPdfManipulator` port and request/result records. Consumers
  (`OCRWeb.Document`, `OCRWeb.OCR`) depend only on this.
- **OCRWeb.Pdf** — the concrete adapter. Only the composition root (`OCRWeb.API`) references it,
  to register the implementation.

## Ownership boundary (important)

This library answers *"given these bytes and this rectangle, give me the result"*. It does **not**
own the meaning of the rectangle:

- Crop coordinates that describe *how a derived document was produced* → owned by **OCRWeb.Document**
  (`FileProperties`, provenance).
- Region coordinates that describe *what field to extract* → owned by **OCRWeb.OCR** (region rule).

Same numbers, different owners. `OCRWeb.Pdf` just does the geometry.

## Layout

| File | Role |
|------|------|
| `OCRWeb.Pdf.Contract/IPdfManipulator.cs` | port + `PdfCropArea` DTO |
| `OCRWeb.Pdf/PdfSharpManipulator.cs` | PDFsharp adapter (registered in `OCRWeb.API`) |

Adapter registration lives in the composition root (`OCRWeb.API/Program.cs`), not in a module —
so any module can consume `IPdfManipulator` without depending on the concrete engine.
