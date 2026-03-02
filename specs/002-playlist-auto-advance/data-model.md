# Data Model: Playlist Auto-Advance

**Feature**: 002-playlist-auto-advance
**Date**: 2026-03-01

## Entities

### PlaylistContext (Runtime state — SheetMusicLib)

Tracks the active playlist session. Created when a title is opened
from the playlist UI. Not persisted.

| Field               | Type                        | Default | Description                                          |
|---------------------|-----------------------------|---------|------------------------------------------------------|
| Playlist            | Playlist                    | —       | The active playlist (reference, not copy)             |
| PdfMetadataList     | List\<PdfMetaDataReadResult\> | —     | All loaded PDF metadata for resolving book references |
| CurrentEntryIndex   | int                         | 0       | Index into Playlist.Entries for the current title     |
| AutoAdvanceEnabled  | bool                        | true    | Whether auto-advance is active                        |

**Computed properties**:
- `CurrentEntry` → `Playlist.Entries[CurrentEntryIndex]`
- `TotalEntries` → `Playlist.Entries.Count`
- `HasNextEntry` → `CurrentEntryIndex < TotalEntries - 1`
- `HasPreviousEntry` → `CurrentEntryIndex > 0`
- `PositionText` → `$"{CurrentEntryIndex + 1} / {TotalEntries}"`

**Methods**:
- `GetTitlePageRange(PdfMetaDataReadResult metadata)` → (firstPage, lastPage)
  Calculates the page range for the current entry using TOC entries.
- `AdvanceToNext()` → PlaylistEntry? (returns next entry or null)
- `GoToPrevious()` → PlaylistEntry? (returns previous entry or null)
- `UpdateContextForPage(int pageNo, PdfMetaDataReadResult metadata)` →
  Updates CurrentEntryIndex if the page belongs to a different playlist
  title in the same book.

### TitlePageRange (Value — SheetMusicLib)

Page range of a title within a book, derived from TOC.

| Field     | Type | Description                                         |
|-----------|------|-----------------------------------------------------|
| FirstPage | int  | Start page of the title (from TOCEntry.PageNo)      |
| LastPage  | int  | End page (next TOC entry's PageNo - 1, or MaxPageNum - 1) |

### Existing Entities (unchanged)

- **Playlist**: Name, Entries list, CreatedDate, ModifiedDate. Already
  defined in MusicDataTypes.cs. No modifications needed.
- **PlaylistEntry**: SongName, Composer, PageNo, BookName, Notes.
  Already defined in MusicDataTypes.cs. No modifications needed.
- **TOCEntry**: SongName, Composer, PageNo, etc. Already defined in
  MusicDataTypes.cs. No modifications needed.

## State Transitions

```text
    ┌─────────────────────────┐
    │   No Playlist Context   │ ← Open title from Books/Query/Favorites
    └────────────┬────────────┘
                 │
                 │ Open title from Playlist UI
                 ▼
    ┌─────────────────────────┐
    │ PlaylistContext Active   │
    │ (Entry N, AutoAdv=ON)   │
    └────┬───────────┬────────┘
         │           │
   Forward past      │  Backward past
   last page         │  first page
         │           │
         ▼           ▼
    ┌──────────┐  ┌──────────┐
    │ Load     │  │ Load     │
    │ Entry    │  │ Entry    │
    │ N+1      │  │ N-1      │
    └────┬─────┘  └────┬─────┘
         │             │
         ▼             ▼
    ┌─────────────────────────┐
    │ PlaylistContext Active   │
    │ (Entry updated)         │
    └─────────────────────────┘

Special transitions:
  - Toggle auto-advance OFF → context preserved, advancement disabled
  - Toggle auto-advance ON → advancement resumes from current position
  - Page jump within same book → context updates to matching title
  - Page jump to non-playlist page → context preserved (no update)
  - Last entry + forward → stay on last page (no wrap)
  - First entry + backward → stay on first page (no wrap)
  - Open non-playlist title → context cleared
  - Entry references missing book → skip to next valid entry + notify
```

## Relationships

```text
PlaylistContext ──references──→ Playlist
PlaylistContext ──references──→ List<PdfMetaDataReadResult>
PlaylistContext ──indexes──→ PlaylistEntry (via CurrentEntryIndex)
PlaylistEntry ──identifies──→ PdfMetaDataReadResult (via BookName)
PlaylistEntry ──identifies──→ TOCEntry (via PageNo + BookName)
TOCEntry + next TOCEntry ──derives──→ TitlePageRange
```

## Validation Rules

- PlaylistContext.CurrentEntryIndex MUST be in range [0, TotalEntries - 1]
- AdvanceToNext() MUST return null when CurrentEntryIndex == TotalEntries - 1
- GoToPrevious() MUST return null when CurrentEntryIndex == 0
- GetTitlePageRange() MUST handle: title with no TOC entry (use entire book),
  title as last TOC entry (use MaxPageNum - 1), title in middle of TOC
- UpdateContextForPage() MUST only update if the page belongs to a
  playlist entry in the same book; otherwise preserve current context
