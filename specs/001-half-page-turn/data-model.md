# Data Model: Half-Page Turn Mode

**Feature**: 001-half-page-turn
**Date**: 2026-03-01

## Entities

### HalfPageTurnLayout (Enum — SheetMusicLib)

Defines the two split-view layout variants.

| Value       | Description                                                   |
|-------------|---------------------------------------------------------------|
| Preview     | Top = next page start, Bottom = current page end (default)    |
| ReadingFlow | Top = current page end, Bottom = next page start              |

### HalfPageTurnSettings (Value object — SheetMusicLib)

Persisted user preference for half-page turn mode.

| Field    | Type                 | Default         | Description                        |
|----------|----------------------|-----------------|------------------------------------|
| Enabled  | bool                 | false           | Whether half-page turn is active   |
| Layout   | HalfPageTurnLayout   | Preview         | Selected layout variant            |

### ViewState (Runtime state — SheetMusicViewer.Desktop)

Transient UI state tracking the current display mode. Not persisted.

| Field         | Type | Default | Description                                    |
|---------------|------|---------|------------------------------------------------|
| IsInSplitView | bool | false   | True when displaying a split view of two pages |

## State Transitions

```text
                    ┌──────────────────┐
                    │   Full Page (N)  │
                    └────────┬─────────┘
                             │
              forward        │         backward
           ┌─────────────────┼──────────────────┐
           ▼                                    │
┌──────────────────────┐                        │
│ Split View (N, N+1)  │────────────────────────┘
└──────────┬───────────┘        backward
           │
           │ forward
           ▼
    ┌──────────────────┐
    │  Full Page (N+1) │
    └──────────────────┘

Special transitions:
  - Direct jump (slider/TOC/page entry) → always Full Page (target)
  - Last page + forward → stay Full Page (no split view)
  - First page + backward → stay Full Page (no split view)
  - Toggle off while in split view → Full Page (current page)
  - Switch to dual-page mode → Full Page (half-page turn disabled)
```

## Persistence

Settings stored in `settings.json` (local machine settings) via
AppSettings.LocalSettings:

```json
{
  "HalfPageTurnEnabled": false,
  "HalfPageTurnLayout": "Preview"
}
```

## Relationships

- **HalfPageTurnSettings** → read by PdfViewerWindow on startup
- **HalfPageTurnSettings** → written by PdfViewerWindow on toggle/layout change
- **ViewState.IsInSplitView** → controls ShowPageAsync() rendering path
- **ViewState.IsInSplitView** → controls NavigateAsync() delta behavior
- **Show2Pages** → when true, HalfPageTurnSettings.Enabled is forced false (mutual exclusion)
