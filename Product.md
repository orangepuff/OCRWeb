# OCRWeb — Product Overview

> Source of truth for product scope. Architecture/implementation status lives in `development-plan.docx`; this file is about *what* the product does and for *whom*, not *how* it's built.

## 1. What OCRWeb is

A web-based system for uploading PDF documents, organizing them into projects, preparing pages (cropping/sectioning), and — in a later phase — running OCR (optical character recognition) on them so their content becomes searchable/indexed text.

Today's build covers the **document intake and project workflow**. The OCR pipeline itself (page extraction, recognition, indexing) is explicitly out of scope for the current phase — see [Section 4](#4-out-of-scope--future).

## 2. Core use cases

These two flows define the current phase (from `development-plan.docx` §8):

**Use Case 1 — New project from a PDF**
A user uploads a PDF. The original file is stored. A project record is created to hold it (project lifecycle itself is being built — see `OCRWeb.ProjectManagement` in [Section 3](#3-feature-scope-by-area)).

**Use Case 2 — Open an existing project**
A user lists a project's files, views file detail, streams/downloads the original content, and can re-crop a previously-derived file from its saved crop parameters (no need to re-select the crop region from scratch).

## 3. Feature scope by area

Status values match `development-plan.docx`'s legend: **Implemented** (built + tested), **Partial** (some of the area works), **Scaffold** (project exists, no logic yet), **Planned** (not started).

| Area | Status | What it does |
|---|---|---|
| Sign-in with Google | In progress | Users authenticate via their Google account; the Bff is the auth boundary, the API never sees a Google token directly. First-time sign-in can auto-create a local account (feature-flagged) or link to an existing admin/local account by matching email. See `docs/Authentication/authentication-design.docx`. |
| Document upload & crop | Partial | Upload an original PDF; crop a region of a page into a new derived file; list a project's files; view file detail; stream/download file content. Split-into-sections is designed but not built. |
| Project lifecycle | Planned | Create / open / archive a project; naming and duplicate-handling policy. Uploads currently aren't yet tied to a real project record. |
| OCR processing | Future / out of scope | Page extraction, a recognition job queue, and indexing so document text becomes searchable. Not started. |

## 4. Out of scope / future

- OCR pipeline internals (page extraction, job queue, text recognition, search indexing)
- Anything beyond the two core use cases above (no reporting, billing, multi-tenant admin, etc. currently planned)

## 5. Known open risks (carried from `development-plan.docx` §9)

- Large PDF uploads may need streaming and size limits enforced at the Bff/API layers (not yet implemented).
- Crop coordinate orientation between the frontend and the underlying PDF point system needs to be confirmed once the frontend crop UI exists — see `Design.md`.
- `OCRWeb.ProjectManagement` and the create-project-from-PDF orchestration aren't built yet — today's upload flow doesn't yet attach files to a real project.

## 6. Related documents

- `development-plan.docx` — architecture and per-project implementation status
- `docs/Authentication/authentication-design.docx` — Google sign-in design and implementation record
- `docs/Authentication/prepare-for-dev.docx` — per-machine dev environment setup
- `Design.md` — frontend UI/UX design (screens, flows, components)
