# Generic file schema, in-place crop, and project-delete cascade

Three related changes to `OCRWeb.Document`'s `docproc` schema and behavior.

## Schema rename (generic naming, column-prefix convention)

`docproc.PDFFiles` and `docproc.PDFFileContents` were PDF-specific names tied to the concept of
a "Project" (`ProjectId`), and neither table's own PK followed the repo's column-prefix
convention (`Id` instead of `iId`). Renamed:

| Old | New |
|---|---|
| `docproc.PDFFiles` | `docproc.Files` |
| `PDFFiles.Id` | `Files.iId` |
| `PDFFiles.ProjectId` | `Files.iParentId` |
| `docproc.PDFFileContents` | `docproc.FileContents` |
| `PDFFileContents.PdfFileId` (PK+FK) | `FileContents.iFileId` (FK only, unique) |
| *(none)* | `FileContents.iId` - new `int IDENTITY` surrogate PK |
| *(none)* | `Files.btActive` - new `bit NOT NULL DEFAULT 1` |

`FileContents` used to share its PK with `Files` (`PdfFileId` was both PK and FK, a common 1:1
pattern). It now has its own technical `iId` identity PK; `iFileId` is a plain FK with a unique
index to keep the 1:1 relationship. This PK is a shadow property (`PdfFileContentConfiguration`) -
nothing in the domain model needs it, since the aggregate is always addressed via `PdfFile.Id`.

On the domain side, `PdfFile.ProjectId` is renamed to `ParentId`: the Document context doesn't
model "Project" as a concept, it only holds a foreign id by convention (see `CLAUDE.md`'s
"contexts reference each other by id only"). The Application-layer commands/queries
(`UploadPdfCommand.ProjectId`, `ListPdfFilesQuery.ProjectId`, ...) and the public
`PdfFileListItemDto.ProjectId` / `PdfFileDetailDto.ProjectId` contract fields keep the name
`ProjectId`, since from the caller's side it genuinely is one - only the internal persisted
name changed.

`Files.btActive` (default active) is used to filter `IPdfFileRepository.ListByProjectAsync` -
listing a project's files only returns active ones. It is not yet wired to any "deactivate a
file" feature; the column and the listing filter exist ahead of that.

## Crop replaces content in place

`CropPdfCommandHandler` used to create a brand-new derived `PdfFile` row and remove the source
(add-then-delete, so a failure never lost data). It now calls `PdfFile.ApplyCrop(...)` on the
*source* file directly - same id, same row, content/size/checksum/properties/name updated and
`FileType` set to `Cropped`. There's only ever one current file per project, and cropping just
replaces it; no second row is created. `PdfFile.CreateDerived` was removed as dead code once
this was its only caller.

## Project deletion cascades to its files

`Files`/`FileContents` are hard-deleted (not soft-deleted - `btActive` is a display filter, not
a delete mechanism) when the owning project is deleted. `OCRWeb.ProjectManagement` and
`OCRWeb.Document` are separate bounded contexts with separate DbContexts/schemas, so this can't
be a DB-level FK cascade across schemas. Instead:

1. `DeleteProjectCommandHandler` publishes `ProjectDeletedNotification(ProjectId)` (a MediatR
   `INotification` carrying only the id, defined in `OCRWeb.ProjectManagement.Contract`) after
   the project itself is removed.
2. `OCRWeb.Document.Application.Events.ProjectDeleted.ProjectDeletedNotificationHandler` reacts
   to it and calls `IPdfFileRepository.RemoveAllByParentIdAsync`, which issues a set-based
   `ExecuteDeleteAsync` on `Files` for that parent id (ignoring `btActive`, since this is a full
   teardown, not a listing query). `FileContents` rows cascade via the existing FK
   (`ON DELETE CASCADE`) at the DB level.

This is the first cross-context MediatR notification in the codebase; `OCRWeb.Document` takes a
`ProjectReference` on `OCRWeb.ProjectManagement.Contract` (contract-only, per the dependency
direction rules) to consume the notification type.
