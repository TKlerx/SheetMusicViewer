# Tasks: Half-Page Turn Mode

**Input**: Design documents from `/specs/001-half-page-turn/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md

**Tests**: Tests are included per Constitution Principle V (Test-First).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup

**Purpose**: Create the new source files and enum types needed by all user stories

- [ ] T001 [P] Create HalfPageTurnLayout enum (Preview, ReadingFlow) in SheetMusicLib/HalfPageTurnMode.cs
- [ ] T002 [P] Create HalfPageTurnTests test class skeleton in AvaloniaTests/Tests/HalfPageTurnTests.cs

**Checkpoint**: Enum type compiles, test project references it

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core bitmap cropping and split-view rendering that ALL user stories depend on

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T003 Add static bitmap cropping helper method to PdfViewerWindow.axaml.cs that uses SKBitmap.ExtractSubset() to crop a rendered page bitmap to its top half or bottom half, returning a new Avalonia Bitmap (depends on T001)
- [ ] T004 Add split-view rendering path to ShowPageAsync() in SheetMusicViewer.Desktop/PdfViewerWindow.axaml.cs: when _isInSplitView is true, build a 2-row Grid with two InkCanvasControl instances (IsInkingEnabled=false), each showing a cropped half-page bitmap. Row arrangement depends on HalfPageTurnLayout (Preview: top=next page top half, bottom=current page bottom half; ReadingFlow: top=current page bottom half, bottom=next page top half). Include a 1px horizontal divider between rows. (depends on T003)

**Checkpoint**: Foundation ready — split-view rendering works when _isInSplitView is manually set to true

---

## Phase 3: User Story 1 — Activate Half-Page Turn Mode (Priority: P1) 🎯 MVP

**Goal**: Forward and backward navigation alternates between full-page and split-view display when half-page turn is enabled

**Independent Test**: Open any multi-page PDF in single-page mode, enable half-page turn, click forward repeatedly. Display alternates: full → split → full → split.

### Tests for User Story 1

- [ ] T005 [P] [US1] Write unit tests for navigation state alternation logic in AvaloniaTests/Tests/HalfPageTurnTests.cs: test that _isInSplitView toggles on each forward navigation, resets on direct page jump, does not activate on last page forward, and reverses correctly on backward navigation
- [ ] T006 [P] [US1] Write unit tests for both layout variants in AvaloniaTests/Tests/HalfPageTurnTests.cs: given Preview layout, verify top=next page top half and bottom=current page bottom half; given ReadingFlow layout, verify top=current page bottom half and bottom=next page top half

### Implementation for User Story 1

- [ ] T007 [US1] Add _isInSplitView boolean field and _halfPageTurnEnabled boolean field to PdfViewerWindow.axaml.cs. Add _halfPageTurnLayout field of type HalfPageTurnLayout (default: Preview)
- [ ] T008 [US1] Modify NavigateAsync() in PdfViewerWindow.axaml.cs: when _halfPageTurnEnabled is true and in single-page mode, alternate _isInSplitView on each forward/backward navigation instead of always advancing by NumPagesPerView. On forward from full page: set _isInSplitView=true and call ShowPageAsync (same page, split view shows current+next). On forward from split view: set _isInSplitView=false, increment page, call ShowPageAsync. Reverse for backward. Guard: do not enter split view on last page forward or first page backward. (depends on T007)
- [ ] T009 [US1] Modify ShowPageAsync() call site to pass _isInSplitView state so the rendering path from T004 activates. Ensure direct page jumps (slider, page number TextBox, TOC links) reset _isInSplitView to false before calling ShowPageAsync. (depends on T004, T008)
- [ ] T010 [US1] Handle edge case: when on last page and _halfPageTurnEnabled, forward navigation stays on last page in full view (_isInSplitView remains false). When on first page, backward navigation stays on first page in full view. (depends on T008)
- [ ] T011 [US1] Ensure ink annotations display correctly in split view: pass the appropriate InkStrokeClass to each InkCanvasControl in the split-view grid. The cropped bitmap and ClipToBounds=true on InkCanvasControl will visually clip strokes to the visible half. Offset ink stroke Y-coordinates for the bottom-half crop. (depends on T004)

**Checkpoint**: Half-page turn alternation works for both layouts with all navigation methods. Ink annotations visible. MVP complete.

---

## Phase 4: User Story 2 — Toggle Half-Page Turn On and Off (Priority: P2)

**Goal**: User can toggle half-page turn mode and switch layout variants via the menu. Option disabled in dual-page mode.

**Independent Test**: Toggle half-page turn on/off via menu. Switch between Preview and Reading Flow. Verify immediate effect. Verify option disabled in dual-page mode.

### Implementation for User Story 2

- [ ] T012 [US2] Add menu items to PdfViewerWindow.axaml: a "Half-Page Turn" checkbox MenuItem (following the Show2Pages pattern with CheckBox icon and IsHitTestVisible=False), and a "Half-Page Layout" submenu with "Preview" and "Reading Flow" radio-style MenuItems. Place after the existing "Show 2 Pages" menu item. Bind IsEnabled to a new computed property that is true only when PdfUIEnabled is true and Show2Pages is false.
- [ ] T013 [US2] Add click handlers in PdfViewerWindow.axaml.cs for the half-page turn menu items: toggle _halfPageTurnEnabled on click (similar to Show2Pages toggle pattern). When disabling while _isInSplitView is true, reset to full-page view by setting _isInSplitView=false and calling ShowPageAsync(CurrentPageNumber). Layout switch updates _halfPageTurnLayout. (depends on T007, T012)
- [ ] T014 [US2] Add mutual exclusion with Show2Pages: when Show2Pages is set to true, automatically set _halfPageTurnEnabled=false and _isInSplitView=false. Update the Show2Pages setter in PdfViewerWindow.axaml.cs to include this logic. When half-page turn is enabled, prevent enabling Show2Pages (or auto-disable half-page turn). (depends on T007)
- [ ] T015 [US2] Bind menu item checked state to _halfPageTurnEnabled and _halfPageTurnLayout properties. Add OnPropertyChanged notifications so menu checkboxes update visually when state changes programmatically (e.g., when Show2Pages disables half-page turn). (depends on T013)

**Checkpoint**: Menu toggle and layout switching work. Dual-page mode disables half-page turn.

---

## Phase 5: User Story 3 — Persist Half-Page Turn Preference (Priority: P3)

**Goal**: Half-page turn enabled/disabled and layout variant persist across application sessions

**Independent Test**: Enable half-page turn with Reading Flow layout, close and reopen the application, verify mode and layout are restored.

### Tests for User Story 3

- [ ] T016 [P] [US3] Write unit test for setting persistence in AvaloniaTests/Tests/HalfPageTurnTests.cs: verify that HalfPageTurnEnabled and HalfPageTurnLayout are saved to and loaded from AppSettings following the Show2Pages pattern

### Implementation for User Story 3

- [ ] T017 [US3] Add HalfPageTurnEnabled (bool, default false) and HalfPageTurnLayout (string, default "Preview") properties to AppSettings in SheetMusicLib/AppSettings.cs. Add matching properties to the LocalSettings helper class. Add load logic in Load() and save logic in SaveLocal(), following the exact Show2Pages pattern.
- [ ] T018 [US3] Load persisted settings on PdfViewerWindow initialization: read AppSettings.Instance.HalfPageTurnEnabled and AppSettings.Instance.HalfPageTurnLayout, set _halfPageTurnEnabled and _halfPageTurnLayout fields accordingly. (depends on T017, T007)
- [ ] T019 [US3] Save settings on change: when _halfPageTurnEnabled or _halfPageTurnLayout changes (in menu click handlers from T013), update AppSettings.Instance and call SaveLocal(). (depends on T017, T013)

**Checkpoint**: Settings survive application restart.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Edge cases, performance, and cleanup

- [ ] T020 Verify page caching behavior: ensure the existing page cache pre-renders the next page when in single-page mode so the split view has both bitmaps ready. If not already cached, ensure TryAddCacheEntry is called for CurrentPageNumber+1 when half-page turn is enabled. In PdfViewerWindow.axaml.cs ShowPageAsync prefetch logic.
- [ ] T021 Handle window resize during split view: ensure the split-view grid layout adapts correctly when the window is resized. The 2-row grid with Star sizing should handle this automatically, but verify InkCanvasControl properly rescales the cropped bitmaps.
- [ ] T022 Run quickstart.md validation checklist from specs/001-half-page-turn/quickstart.md — verify all 8 items pass on at least one platform

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 (T001) — BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Foundational (Phase 2) completion
- **User Story 2 (Phase 4)**: Depends on User Story 1 (Phase 3) for _halfPageTurnEnabled field
- **User Story 3 (Phase 5)**: Depends on User Story 2 (Phase 4) for menu handlers to hook save logic into
- **Polish (Phase 6)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational — no cross-story deps
- **User Story 2 (P2)**: Depends on US1 fields (_halfPageTurnEnabled, _isInSplitView) but is independently testable
- **User Story 3 (P3)**: Depends on US2 menu handlers but is independently testable

### Within Each User Story

- Tests written first (T005-T006 before T007-T011)
- Fields/state before logic (T007 before T008-T011)
- Navigation logic before rendering integration (T008 before T009)
- Core before edge cases (T008-T009 before T010-T011)

### Parallel Opportunities

- T001 and T002 can run in parallel (different files)
- T005 and T006 can run in parallel (same file but independent test methods)
- T016 can run in parallel with US3 implementation tasks

---

## Parallel Example: User Story 1

```bash
# Launch setup tasks together:
Task T001: "Create HalfPageTurnLayout enum in SheetMusicLib/HalfPageTurnMode.cs"
Task T002: "Create test class skeleton in AvaloniaTests/Tests/HalfPageTurnTests.cs"

# Launch US1 tests together (after foundation):
Task T005: "Unit tests for navigation state alternation"
Task T006: "Unit tests for both layout variants"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T002)
2. Complete Phase 2: Foundational (T003-T004)
3. Complete Phase 3: User Story 1 (T005-T011)
4. **STOP and VALIDATE**: Half-page turn alternation works with both layouts
5. Demo-ready with hardcoded enable (no menu toggle yet)

### Incremental Delivery

1. Setup + Foundational → Bitmap cropping and split-view rendering ready
2. User Story 1 → Navigation alternation works (MVP!)
3. User Story 2 → Menu toggle and layout switching added
4. User Story 3 → Settings persist across sessions
5. Polish → Edge cases, performance, validation

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Total: 22 tasks (2 setup, 2 foundational, 7 US1, 4 US2, 4 US3, 3 polish)
