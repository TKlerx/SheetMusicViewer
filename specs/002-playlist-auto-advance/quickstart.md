# Quickstart: Playlist Auto-Advance

**Feature**: 002-playlist-auto-advance

## Prerequisites

- .NET 10.0 SDK installed
- Repository cloned and on branch `002-playlist-auto-advance`
- At least 2 PDF books with TOC entries in your music root folder

## Build

```bash
dotnet build SheetMusicViewer.Desktop
```

## Run

```bash
dotnet run --project SheetMusicViewer.Desktop
```

## Test the Feature

### Setup: Create a Test Playlist

1. Open the application and set a music root folder containing PDF books.
2. Go to the Playlists tab (press Alt+P or click the tab).
3. Create a new playlist (e.g., "Concert Setlist").
4. Add at least 3 titles from different books to the playlist.

### Test Auto-Advance (Forward)

5. Double-click the first title in the playlist to open it in the viewer.
6. Verify: the playlist position indicator shows "1 / 3" (or your count)
   at the bottom of the viewer.
7. Enable auto-advance via View menu → "Auto-Advance Playlist" checkbox.
8. Navigate forward to the last page of the first title.
9. Click forward one more time.
10. Verify: the viewer loads the first page of the second title.
11. Verify: the position indicator updates to "2 / 3".
12. Repeat until reaching the last title's last page.
13. Click forward — verify the viewer stays on the last page (no wrap).

### Test Auto-Advance (Backward)

14. From the second title's first page, click backward.
15. Verify: the viewer loads the last page of the first title.
16. Verify: the position indicator updates to "1 / 3".

### Test Toggle

17. Disable auto-advance via the View menu.
18. Navigate to the last page of the current title and click forward.
19. Verify: the viewer stays on the last page (normal behavior).
20. Re-enable auto-advance and click forward.
21. Verify: the viewer advances to the next title.

### Test Page Jump

22. While in a playlist, jump to a page in a different title (same book)
    via the slider or page number input.
23. Verify: the position indicator updates to reflect the new title.

## Run Tests

```bash
dotnet test AvaloniaTests
```

## Validation Checklist

- [ ] Auto-advance forward works across titles in the same book
- [ ] Auto-advance forward works across titles in different books
- [ ] Auto-advance backward works (first page → previous title's last page)
- [ ] Last title + forward → stays on last page (no wrap)
- [ ] First title + backward → stays on first page (no wrap)
- [ ] Toggle on/off takes effect immediately
- [ ] Auto-advance menu option disabled when no playlist is active
- [ ] Position indicator visible and accurate
- [ ] Position indicator hidden when no playlist is active
- [ ] Page jump updates playlist context to correct title
- [ ] Missing book skipped with notification
- [ ] Works in both single-page and dual-page modes
- [ ] Title transitions appear without perceptible delay
