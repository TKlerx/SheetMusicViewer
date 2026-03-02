# Implementation Plan: Half-Page Turn Mode

**Branch**: `001-half-page-turn` | **Date**: 2026-03-01 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-half-page-turn/spec.md`

## Summary

Add a half-page turn mode for single-page viewing that alternates
between full-page and split-view displays on each navigation click.
The split view shows halves of two adjacent pages simultaneously,
giving musicians a seamless reading experience. Two layout variants
are supported: Preview (next page top, current page bottom) and
Reading Flow (current page top, next page bottom). The feature uses
existing page cache infrastructure and bitmap cropping to render
partial pages without additional rendering overhead.

## Technical Context

**Language/Version**: C# / .NET 10.0
**Primary Dependencies**: Avalonia 11.3.9, PDFtoImage 5.0.0, SkiaSharp 3.116.1, CommunityToolkit.MVVM 8.4.0
**Storage**: Local JSON settings file (settings.json via AppSettings)
**Testing**: xUnit (AvaloniaTests project, Tests project)
**Target Platform**: Windows (x64/x86/ARM), macOS (arm64), Linux (x64)
**Project Type**: Desktop application (cross-platform)
**Performance Goals**: Split-view page turn latency imperceptible (<50ms perceived), matching existing full-page turn performance
**Constraints**: No new dependencies; must use existing page cache; no PDF mutation; cross-platform Avalonia-only UI
**Scale/Scope**: Single-user desktop app; feature touches ~5 files

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| # | Principle | Status | Notes |
|---|-----------|--------|-------|
| I | Performance-First | ✅ PASS | Split view reuses cached page bitmaps; cropping via SKBitmap.ExtractSubset() is O(pixels) with no re-render. Pre-rendering already covers the next page. No new rendering overhead. |
| II | Cross-Platform | ✅ PASS | Implementation uses only Avalonia Grid layout + SkiaSharp bitmap ops. No platform-specific code. |
| III | Non-Destructive | ✅ PASS | Feature is display-only. No PDF writes. Ink annotations are display-only in split view. Setting stored in local JSON. |
| IV | Library-First | ✅ PASS | HalfPageTurnMode enum and setting live in SheetMusicLib. ViewState tracking is UI-layer state (appropriate for Desktop project). |
| V | Test-First | ✅ PASS | Unit tests for state machine logic in SheetMusicLib. Integration tests for setting persistence. UI behavior testable via existing Avalonia test infrastructure. |

No violations. No complexity tracking entries needed.

## Project Structure

### Documentation (this feature)

```text
specs/001-half-page-turn/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
SheetMusicLib/
├── HalfPageTurnMode.cs          # NEW: Enum + state machine for split-view logic
├── AppSettings.cs               # MODIFIED: Add HalfPageTurnEnabled, HalfPageTurnLayout
└── MusicDataTypes.cs            # UNCHANGED (no new data types needed here)

SheetMusicViewer.Desktop/
├── PdfViewerWindow.axaml        # MODIFIED: Add menu items for half-page turn
├── PdfViewerWindow.axaml.cs     # MODIFIED: Split-view rendering in ShowPageAsync,
│                                #   navigation alternation in NavigateAsync,
│                                #   bitmap cropping helper
├── InkCanvasControl.cs          # UNCHANGED (display-only in split view)
└── GestureHandler.cs            # UNCHANGED (delta passed through as-is)

AvaloniaTests/
└── Tests/
    └── HalfPageTurnTests.cs     # NEW: Tests for state machine and settings
```

**Structure Decision**: Existing multi-project structure (SheetMusicLib +
SheetMusicViewer.Desktop + AvaloniaTests). New enum/logic class in
SheetMusicLib; UI modifications in Desktop project; tests in AvaloniaTests.
