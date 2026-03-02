# Research: Playlist Auto-Advance

**Feature**: 002-playlist-auto-advance
**Date**: 2026-03-01

## R1: Title Page Range Calculation

**Decision**: Calculate the last page of a TOC entry by finding the
next TOC entry's start page minus one, or using MaxPageNum - 1 for
the final entry.

**Rationale**: TOCEntry only has a PageNo (start page), not an end
page. The page range is implicitly defined by adjacent entries:
- Entry at index i: pages [toc[i].PageNo, toc[i+1].PageNo - 1]
- Last entry: pages [toc[last].PageNo, MaxPageNum - 1]

This is the standard approach for start-page-only TOC structures.
PdfMetaDataReadResult already exposes TocEntries as a list and
MaxPageNum via PageNumberOffset + NumPagesInSet.

**Alternatives considered**:
- Add an EndPageNo field to TOCEntry: Would require migration of
  existing serialized data. Unnecessary since end page is always
  derivable from adjacent entries.
- Calculate on every navigation: Would be fine performance-wise but
  wasteful. Better to compute once when PlaylistContext is established
  and cache the range.

## R2: Cross-Title Navigation Mechanism

**Decision**: Use the existing `LoadPdfFileAndShowAsync(PdfMetaDataReadResult, int pageNo)` method to open the next title. This handles everything: closing the current PDF, loading the new one, pre-loading volume bytes, and rendering the target page.

**Rationale**: LoadPdfFileAndShowAsync is the established entry point
for opening any PDF at a specific page. It handles:
- CloseCurrentPdfFile() cleanup
- Setting _currentPdfMetaData
- Slider/UI state updates
- Volume byte pre-loading
- ShowPageAsync() call

Reusing it ensures consistent behavior and avoids duplicating
initialization logic. The ChooseMusicWindow already uses this path
(via ChosenPdfMetaData/ChosenPageNo properties read by
ChooseMusicAsync).

**Alternatives considered**:
- Direct ShowPageAsync with metadata swap: Would skip cleanup and
  pre-loading steps, risking resource leaks or stale state.
- Event-based approach (fire "open next title" event): Adds
  indirection without benefit since the viewer already owns the
  LoadPdfFileAndShowAsync method.

## R3: PlaylistContext Lifecycle

**Decision**: Create a PlaylistContext class in SheetMusicLib that
holds: the Playlist reference, the list of PdfMetaDataReadResult
objects for resolving entries, the current entry index, and
auto-advance enabled flag. Set it when a title is opened from the
playlist UI; clear it when opening a title from any other source.

**Rationale**: The context must survive across PDF loads (since each
title transition opens a new PDF). It cannot live on PdfViewerWindow's
per-PDF state. A standalone class in SheetMusicLib keeps the logic
testable and the viewer thin.

The ChooseMusicWindow needs to communicate two new pieces of
information to PdfViewerWindow: (1) the Playlist object and (2) the
entry index. This extends the existing ChosenPdfMetaData/ChosenPageNo
pattern with ChosenPlaylist/ChosenPlaylistEntryIndex properties.

**Alternatives considered**:
- Store context in AppSettings: AppSettings is for persistent data;
  PlaylistContext is session-level and runtime-only.
- Store on PdfViewerWindow directly: Would work but violates
  Library-First principle. Logic like "am I on the last page of this
  title?" belongs in SheetMusicLib.

## R4: Navigation Interception Point

**Decision**: Modify NavigateAsync() to check, before clamping,
whether the navigation would go beyond the current title's page range.
If so and auto-advance is enabled, call LoadPdfFileAndShowAsync with
the next playlist entry instead of the normal page navigation.

**Rationale**: NavigateAsync is the single funnel for all navigation
(buttons, keyboard, gestures). Intercepting here ensures consistent
behavior regardless of input method.

The check is:
```
if (autoAdvanceEnabled && playlistContext != null)
  if (delta > 0 && currentPage == lastPageOfCurrentTitle)
    → advance to next playlist title
  if (delta < 0 && currentPage == firstPageOfCurrentTitle)
    → go back to previous playlist title's last page
```

**Alternatives considered**:
- Intercept in ShowPageAsync: Too late; ShowPageAsync already clamps
  the page number. The "beyond boundary" signal would be lost.
- Intercept in GestureHandler: Too early; would miss keyboard and
  button navigation.

## R5: Pre-Caching Next Title's First Page

**Decision**: When the musician is within 2 pages of the current
title's last page, pre-load the next title's PDF volume bytes in the
background. The actual page bitmap rendering cannot be pre-cached in
the current architecture (cache is per-PDF), but volume byte loading
is the main latency bottleneck.

**Rationale**: LoadPdfFileAndShowAsync already pre-loads volume bytes
via GetOrLoadVolumeBytes(). The bottleneck for cross-title transitions
is loading a new PDF file from disk. Pre-loading the bytes when the
musician approaches the title boundary minimizes transition latency.

Page bitmap caching across different PDFs would require a cross-PDF
cache, which is out of scope for this feature. Volume byte pre-loading
is sufficient to meet the <50ms perceived latency target for most PDFs.

**Alternatives considered**:
- Cross-PDF bitmap cache: Significant architectural change to the
  page cache (currently keyed by page number within a single PDF).
  Disproportionate complexity for the benefit.
- No pre-loading: Would cause noticeable delay on title transitions
  for large PDF files. Violates Performance-First principle.

## R6: Playlist Position Indicator Placement

**Decision**: Add a TextBlock at the bottom-center of the viewer
overlay grid, visible only when a PlaylistContext is active. Display
format: "3 / 12" (current title index / total titles).

**Rationale**: The existing UI overlay grid (PdfViewerWindow.axaml)
has controls at the top. The bottom of the screen is currently empty,
making it the natural location for a non-intrusive status indicator.
The TextBlock uses data binding to a PlaylistPositionText property,
which is set to empty string when no playlist is active (hiding via
empty text rather than Visibility toggle, keeping it simple).

**Alternatives considered**:
- Top bar alongside page number: Already crowded with navigation
  controls, ink toggles, and favorites.
- Toast/overlay that fades: More complex; a persistent indicator is
  more useful during a concert since the musician can always check
  their position in the setlist.
