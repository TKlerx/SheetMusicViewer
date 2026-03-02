# Research: Half-Page Turn Mode

**Feature**: 001-half-page-turn
**Date**: 2026-03-01

## R1: Split-View Rendering Approach

**Decision**: Use SKBitmap.ExtractSubset() to crop page bitmaps before
converting to Avalonia Bitmap, then display two cropped bitmaps in a
Grid with two rows.

**Rationale**: The existing rendering pipeline renders full-page bitmaps
via PDFtoImage and converts them with ConvertSkBitmapToAvaloniaBitmap().
Cropping at the SKBitmap level (before Avalonia conversion) is the most
efficient approach because:
- ExtractSubset() is a fast pixel-copy operation, not a re-render
- The cropped bitmap is half the size, reducing memory for the Avalonia
  WriteableBitmap
- Both page bitmaps are already available in the page cache
- No changes needed to the PDF rendering pipeline

**Alternatives considered**:
- Avalonia ClipToBounds with offset rendering: Would require modifying
  InkCanvasControl's ArrangeOverride to position the image off-screen
  and clip. More complex and harder to get ink annotations aligned.
- CroppedBitmap wrapper: Avalonia's CroppedBitmap could wrap the source
  bitmap, but InkCanvasControl expects a full Bitmap, not a cropped one.
  Would require refactoring InkCanvasControl.
- RenderTargetBitmap: Overkill; introduces an extra render pass.

## R2: Grid Layout for Split View

**Decision**: Use a 2-row Grid (similar to existing 3-column Grid for
dual-page mode) with two InkCanvasControl instances, each showing a
cropped half-page bitmap.

**Rationale**: The existing ShowPageAsync() already builds a Grid
dynamically for single vs dual-page display. Adding a third layout
variant (split-view with rows instead of columns) follows the same
pattern. Each InkCanvasControl receives a pre-cropped bitmap and
renders it with ink annotations visible but not editable
(IsInkingEnabled = false).

**Alternatives considered**:
- Single InkCanvasControl with two images: Would complicate ink stroke
  positioning since strokes are stored relative to a single page.
- Custom control: Unnecessary complexity when Grid + InkCanvasControl
  already handles the layout.

## R3: Navigation State Machine

**Decision**: Add a boolean `_isInSplitView` field to PdfViewerWindow
that tracks whether the current display is full-page or split-view.
Navigation alternates this state.

**Rationale**: The state machine is simple:
- Full page (page N) → forward → Split view (N, N+1) → forward → Full page (N+1)
- Split view (N, N+1) → backward → Full page (N)
- Full page (N) → backward → Split view (N-1, N)
- Direct jump → always Full page (target)

A single boolean is sufficient. The "current page" is always the page
whose bottom half is visible (or the full page in full-page state).
This keeps CurrentPageNumber semantics simple.

**Alternatives considered**:
- Enum with more states: Not needed; only two states exist.
- Separate page tracking for both halves: Overcomplicates the model.
  The "current page" + "is split" + "which page is next" (always
  current+1 or current-1) fully defines the display.

## R4: Setting Persistence Pattern

**Decision**: Add `HalfPageTurnEnabled` (bool) and `HalfPageTurnLayout`
(string: "Preview" or "ReadingFlow") to AppSettings.LocalSettings,
following the exact same pattern as `Show2Pages`.

**Rationale**: The existing Show2Pages setting demonstrates the pattern:
property on AppSettings, mirrored in LocalSettings helper class, loaded
in Load(), saved in SaveLocal(). No new serialization infrastructure
needed.

**Alternatives considered**:
- Roaming settings (userdata.json): Not appropriate; this is a UI
  preference tied to the machine/display, not to the music collection.
- Enum serialization: System.Text.Json handles string-to-enum via
  JsonStringEnumConverter, but a simple string property is more robust
  for forward compatibility.

## R5: Ink Annotation Clipping in Split View

**Decision**: Crop ink strokes to match the visible half of each page.
When creating the InkCanvasControl for a half-page, filter the ink
stroke points to only include those within the visible region, or rely
on ClipToBounds=true (already set) to visually clip strokes that extend
beyond the cropped bitmap.

**Rationale**: InkCanvasControl already sets ClipToBounds=true. If the
bitmap is cropped and the control is sized to the cropped bitmap, any
ink strokes that extend beyond the visible half will be naturally
clipped by Avalonia's layout system. However, stroke coordinates are
relative to the full page, so the Y-coordinates need to be offset for
the bottom-half crop. This offset must be applied when passing strokes
to the InkCanvasControl.

**Alternatives considered**:
- Hide all ink in split view: Simpler but loses valuable visual context
  (annotations are often placement cues for musicians).
- Re-render strokes at cropped coordinates: More accurate but requires
  modifying InkCanvasControl's stroke rendering pipeline.
