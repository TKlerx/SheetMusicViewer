# Quickstart: Half-Page Turn Mode

**Feature**: 001-half-page-turn

## Prerequisites

- .NET 10.0 SDK installed
- Repository cloned and on branch `001-half-page-turn`

## Build

```bash
dotnet build SheetMusicViewer.Desktop
```

## Run

```bash
dotnet run --project SheetMusicViewer.Desktop
```

## Test the Feature

1. Open any multi-page PDF from the Books or Singles tab.
2. Ensure single-page mode is active (uncheck "Show 2 Pages" in the
   View menu if needed).
3. Enable half-page turn via View menu → "Half-Page Turn" checkbox.
4. Optionally select layout variant: View menu → "Half-Page Layout" →
   "Preview" or "Reading Flow".
5. Click forward (right side of page, Next button, or Page Down key).
6. Verify: display shows a split view with halves of two adjacent pages.
7. Click forward again.
8. Verify: display shows the next page in full.
9. Click backward twice to verify the sequence reverses.
10. Jump to a specific page via the page number input or slider.
11. Verify: display resets to full-page view of the target page.

## Run Tests

```bash
dotnet test AvaloniaTests
```

## Validation Checklist

- [ ] Split view renders without visible seam or gap between halves
- [ ] Preview layout: top = next page start, bottom = current page end
- [ ] Reading Flow layout: top = current page end, bottom = next page start
- [ ] Ink annotations visible (but not editable) on both halves
- [ ] No perceptible delay on split-view transitions
- [ ] Half-page turn option disabled when dual-page mode is active
- [ ] Setting persists after application restart
- [ ] Works on Windows, macOS, and Linux builds
