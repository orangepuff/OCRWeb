# OCRWeb.Frontend

Angular 22 + Angular Material application (upload UI, project list, and workspace screens).
Talks only to the BFF (`OCRWeb.Bff`) — never directly to `OCRWeb.API`.

> Placeholder. Not part of `OCRWeb.slnx` (Node/Angular toolchain, not MSBuild).

## Scaffold

```bash
# from repo root
ng new OCRWeb.Frontend --directory src/Frontend/OCRWeb.Frontend --style scss --routing
ng add @angular/material
```

Depends on `OCRWeb.Frontend.Shared` for reusable UI (Text, Label, Button, CropPdf, ...).
