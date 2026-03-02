# Implementation Plan: Playlist Auto-Advance

**Branch**: `002-playlist-auto-advance` | **Date**: 2026-03-01 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/002-playlist-auto-advance/spec.md`

## Summary

Add an auto-advance mode that automatically opens the next playlist
title when the musician navigates past the last page of the current
title. The feature introduces a PlaylistContext that tracks the active
playlist and current title index, a title page-range calculator using
TOC entries, and a navigation interceptor that triggers cross-title
transitions via the existing LoadPdfFileAndShowAsync() method. A
persistent position indicator ("3 / 12") is shown in the viewer. The
mode is session-level (not persisted) and only active when a title is
opened from a playlist.

## Technical Context

**Language/Version**: C# / .NET 10.0
**Primary Dependencies**: Avalonia 11.3.9, PDFtoImage 5.0.0, SkiaSharp 3.116.1, CommunityToolkit.MVVM 8.4.0
**Storage**: Existing roaming JSON (userdata.json for playlists), local JSON (settings.json). No new storage needed — PlaylistContext is runtime-only.
**Testing**: xUnit (AvaloniaTests project, Tests project)
**Target Platform**: Windows (x64/x86/ARM), macOS (arm64), Linux (x64)
**Project Type**: Desktop application (cross-platform)
**Performance Goals**: Title transition latency imperceptible (<50ms perceived), matching existing page turn performance. Pre-cache first page of next title.
**Constraints**: No new dependencies; must use existing page cache and LoadPdfFileAndShowAsync() for title transitions; no PDF mutation; cross-platform Avalonia-only UI
**Scale/Scope**: Single-user desktop app; feature touches ~6 files

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| # | Principle | Status | Notes |
|---|-----------|--------|-------|
| I | Performance-First | ✅ PASS | Title transitions reuse LoadPdfFileAndShowAsync() which pre-loads volume bytes and uses page cache. First page of next title can be pre-cached when nearing the last page of current title. No new rendering overhead. |
| II | Cross-Platform | ✅ PASS | Implementation uses only Avalonia UI (TextBlock for indicator, MenuItem for toggle). No platform-specific code. |
| III | Non-Destructive | ✅ PASS | Feature is navigation-only. No PDF writes. PlaylistContext is runtime state. Playlists already stored in roaming JSON (no changes to storage). |
| IV | Library-First | ✅ PASS | PlaylistContext class and title page-range calculation live in SheetMusicLib. Navigation interception and UI indicator are in Desktop project (appropriate for UI-layer). |
| V | Test-First | ✅ PASS | Unit tests for PlaylistContext (title advancement, page-range calculation, boundary conditions). Integration tests for cross-book navigation. |

No violations. No complexity tracking entries needed.

## Project Structure

### Documentation (this feature)

```text
specs/002-playlist-auto-advance/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```text
SheetMusicLib/
├── PlaylistContext.cs           # NEW: Runtime state tracking active playlist,
│                                #   current title index, page-range calculation,
│                                #   title advancement logic
├── PdfMetaDataCore.cs           # MODIFIED: Add helper to calculate last page
│                                #   of a TOC entry
└── MusicDataTypes.cs            # UNCHANGED (Playlist, PlaylistEntry, TOCEntry
                                 #   already defined)

SheetMusicViewer.Desktop/
├── PdfViewerWindow.axaml        # MODIFIED: Add playlist position indicator
│                                #   TextBlock, auto-advance menu toggle
├── PdfViewerWindow.axaml.cs     # MODIFIED: Integrate PlaylistContext with
│                                #   NavigateAsync, add auto-advance interception,
│                                #   pre-cache next title's first page
├── ChooseMusicWindow.cs         # MODIFIED: Set PlaylistContext when opening
│                                #   a title from a playlist (pass playlist +
│                                #   entry index back to viewer)
└── GestureHandler.cs            # UNCHANGED

AvaloniaTests/
└── Tests/
    └── PlaylistAutoAdvanceTests.cs  # NEW: Tests for PlaylistContext,
                                     #   page-range calculation, advancement
```

**Structure Decision**: Existing multi-project structure. New
PlaylistContext class in SheetMusicLib (core logic). UI integration in
Desktop project. Tests in AvaloniaTests.
