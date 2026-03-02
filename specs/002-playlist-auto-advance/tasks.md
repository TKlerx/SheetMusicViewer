# Tasks: Playlist Auto-Advance

**Input**: Design documents from `/specs/002-playlist-auto-advance/`
**Prerequisites**: plan.md (required), spec.md (required), research.md, data-model.md

**Tests**: Tests are included per Constitution Principle V (Test-First).

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

---

## Phase 1: Setup

**Purpose**: Create the new source files and foundational types

- [ ] T001 [P] Create PlaylistContext class in SheetMusicLib/PlaylistContext.cs with fields: Playlist (reference), PdfMetadataList (List\<PdfMetaDataReadResult\>), CurrentEntryIndex (int, default 0), AutoAdvanceEnabled (bool, default true). Add computed properties: CurrentEntry, TotalEntries, HasNextEntry, HasPreviousEntry, PositionText (e.g., "3 / 12"). Add TitlePageRange record (FirstPage, LastPage) in the same file.
- [ ] T002 [P] Create PlaylistAutoAdvanceTests test class skeleton in AvaloniaTests/Tests/PlaylistAutoAdvanceTests.cs with xUnit test structure

**Checkpoint**: PlaylistContext class compiles, test project references it

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core page-range calculation and advancement logic that ALL user stories depend on

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [ ] T003 Add GetLastPageOfTocEntry(int tocIndex) helper method to SheetMusicLib/PdfMetaDataCore.cs: given a TOC entry index, return the last page number (next TOC entry's PageNo - 1, or MaxPageNum - 1 for the final entry). Handle edge case where TocEntries list is empty or null (return MaxPageNum - 1, treating the whole book as one title). (depends on T001)
- [ ] T004 Implement GetTitlePageRange(PdfMetaDataReadResult metadata) method on PlaylistContext in SheetMusicLib/PlaylistContext.cs: find the TOC entry matching CurrentEntry.PageNo in metadata.TocEntries, calculate the page range using T003's helper. If no matching TOC entry found, return (PageNumberOffset, MaxPageNum - 1) to treat entire book as one title. (depends on T003)
- [ ] T005 Implement AdvanceToNext() method on PlaylistContext in SheetMusicLib/PlaylistContext.cs: increment CurrentEntryIndex if HasNextEntry is true, return the new CurrentEntry. Return null if already at last entry. (depends on T001)
- [ ] T006 Implement GoToPrevious() method on PlaylistContext in SheetMusicLib/PlaylistContext.cs: decrement CurrentEntryIndex if HasPreviousEntry is true, return the new CurrentEntry. Return null if already at first entry. (depends on T001)
- [ ] T007 Implement UpdateContextForPage(int pageNo, PdfMetaDataReadResult metadata) method on PlaylistContext in SheetMusicLib/PlaylistContext.cs: scan Playlist.Entries for an entry whose BookName matches metadata's book name and whose page range contains pageNo. If found, update CurrentEntryIndex to that entry's index. If not found, preserve current context. (depends on T004)

**Checkpoint**: Foundation ready — PlaylistContext can calculate page ranges, advance/retreat, and update context on page jumps

---

## Phase 3: User Story 1 — Auto-Advance to Next Playlist Title (Priority: P1) 🎯 MVP

**Goal**: When a playlist is active and the musician navigates past the last page of the current title, the viewer automatically opens the first page of the next title

**Independent Test**: Create a playlist with 3+ titles from different books. Open first title, navigate to last page, click forward. Viewer loads second title's first page. Repeat for second → third.

### Tests for User Story 1

- [ ] T008 [P] [US1] Write unit tests for PlaylistContext.AdvanceToNext() and boundary conditions in AvaloniaTests/Tests/PlaylistAutoAdvanceTests.cs: test advancement returns correct next entry, returns null at last entry, and CurrentEntryIndex updates correctly
- [ ] T009 [P] [US1] Write unit tests for GetTitlePageRange() in AvaloniaTests/Tests/PlaylistAutoAdvanceTests.cs: test page range calculation for middle TOC entry, last TOC entry, and title with no TOC match (whole-book fallback)

### Implementation for User Story 1

- [ ] T010 [US1] Add _playlistContext field (PlaylistContext?, initially null) to PdfViewerWindow.axaml.cs. Add a PlaylistPositionText observable string property (default empty) with OnPropertyChanged notification. (depends on T001)
- [ ] T011 [US1] Modify ChooseMusicWindow.cs: add ChosenPlaylist (Playlist?) and ChosenPlaylistEntryIndex (int) properties. In TryNavigateToPlaylistEntry(), set ChosenPlaylist to _currentPlaylist and ChosenPlaylistEntryIndex to the index of the selected entry within the playlist's Entries list. (depends on T001)
- [ ] T012 [US1] Modify ChooseMusicAsync() in PdfViewerWindow.axaml.cs: after reading chooser.ChosenPdfMetaData, also read chooser.ChosenPlaylist and chooser.ChosenPlaylistEntryIndex. If ChosenPlaylist is not null, create a new PlaylistContext with the playlist, _lstPdfMetaFileData, and the entry index. Otherwise set _playlistContext to null. Update PlaylistPositionText. (depends on T010, T011)
- [ ] T013 [US1] Modify NavigateAsync() in PdfViewerWindow.axaml.cs: before the existing page clamping logic, check if _playlistContext is not null, AutoAdvanceEnabled is true, and delta > 0. If so, get the title page range via _playlistContext.GetTitlePageRange(_currentPdfMetaData). If CurrentPageNumber >= lastPage (or CurrentPageNumber + NumPagesPerView > lastPage for dual-page mode), call _playlistContext.AdvanceToNext(). If it returns a non-null entry, resolve the entry's BookName to a PdfMetaDataReadResult from _lstPdfMetaFileData, then call LoadPdfFileAndShowAsync(metadata, entry.PageNo). Update PlaylistPositionText. Return early (skip normal navigation). (depends on T004, T010)
- [ ] T014 [US1] Handle missing book during auto-advance in PdfViewerWindow.axaml.cs: if the next PlaylistEntry's BookName cannot be resolved to a PdfMetaDataReadResult, skip that entry and try the next one (loop). Display a brief notification (e.g., set the window Title temporarily or use a status message) indicating the skipped title. If all remaining entries are invalid, stay on the current page. (depends on T013)
- [ ] T015 [US1] Add playlist position indicator TextBlock to PdfViewerWindow.axaml: place a TextBlock named "txtPlaylistPosition" at the bottom-center of the main Grid overlay, bound to PlaylistPositionText. Use small font (10pt), gray foreground, and center alignment. Visible only when PlaylistPositionText is not empty (use StringNotEmpty converter or binding with FallbackValue). (depends on T010)
- [ ] T016 [US1] Handle page jump context update: in the slider ValueChanged handler and page number TextBox handler in PdfViewerWindow.axaml.cs, after updating CurrentPageNumber, call _playlistContext?.UpdateContextForPage(CurrentPageNumber, _currentPdfMetaData) and update PlaylistPositionText. (depends on T007, T010)
- [ ] T027 [US1] Handle dual-page mode boundary detection in NavigateAsync() (FR-009): when NumPagesPerView > 1, auto-advance triggers when CurrentPageNumber + NumPagesPerView - 1 >= lastPage of the title. For backward auto-advance, check CurrentPageNumber <= firstPage. (depends on T013)

**Checkpoint**: Auto-advance works forward through a playlist in both single-page and dual-page modes. Position indicator visible and updates. Missing titles are skipped. Page jumps update context. MVP complete.

---

## Phase 4: User Story 2 — Enable and Disable Auto-Advance Mode (Priority: P2)

**Goal**: User can toggle auto-advance on/off via the menu. Option disabled when no playlist is active.

**Independent Test**: Open a playlist title, toggle auto-advance off, navigate past last page — stays on last page. Toggle on, navigate — advances to next title.

### Implementation for User Story 2

- [ ] T017 [US2] Add "Auto-Advance Playlist" checkbox MenuItem to PdfViewerWindow.axaml, placed in the View menu after the half-page turn options (or after Show2Pages if feature 001 is not present). Use the CheckBox icon pattern from Show2Pages. Bind IsEnabled to a new HasPlaylistContext computed property that returns _playlistContext != null. Bind IsChecked to a new AutoAdvanceEnabled property.
- [ ] T018 [US2] Add AutoAdvanceEnabled observable property to PdfViewerWindow.axaml.cs that reads/writes _playlistContext.AutoAdvanceEnabled. When _playlistContext is null, getter returns false. Add HasPlaylistContext computed property. Add click handler for the menu item that toggles _playlistContext.AutoAdvanceEnabled. Fire OnPropertyChanged for both properties when _playlistContext changes. (depends on T010, T017)
- [ ] T019 [US2] Ensure NavigateAsync() auto-advance check (from T013) respects AutoAdvanceEnabled: the interception only fires when _playlistContext.AutoAdvanceEnabled is true. When false, normal page clamping applies (stays on last page). (depends on T013, T018)
- [ ] T020 [US2] Ensure playlist context is preserved when toggling off: verify that disabling AutoAdvanceEnabled does not clear _playlistContext or reset CurrentEntryIndex. Re-enabling should resume from the same position. Write a test in AvaloniaTests/Tests/PlaylistAutoAdvanceTests.cs verifying context preservation across toggle. (depends on T018)

**Checkpoint**: Toggle works, option disabled when no playlist, context preserved across toggle.

---

## Phase 5: User Story 3 — Navigate Backward Through Playlist (Priority: P3)

**Goal**: Backward navigation from the first page of a title opens the last page of the previous title in the playlist

**Independent Test**: Auto-advance to second title, then click backward from its first page. Viewer shows last page of first title.

### Tests for User Story 3

- [ ] T021 [P] [US3] Write unit tests for PlaylistContext.GoToPrevious() and boundary conditions in AvaloniaTests/Tests/PlaylistAutoAdvanceTests.cs: test retreat returns correct previous entry, returns null at first entry, and CurrentEntryIndex updates correctly

### Implementation for User Story 3

- [ ] T022 [US3] Add backward auto-advance interception to NavigateAsync() in PdfViewerWindow.axaml.cs: when _playlistContext is not null, AutoAdvanceEnabled is true, and delta < 0, get the title page range. If CurrentPageNumber <= firstPage (or CurrentPageNumber is at firstPage for single-page mode), call _playlistContext.GoToPrevious(). If it returns a non-null entry, resolve the entry's BookName to metadata, calculate the last page of that entry's TOC range using GetTitlePageRange, then call LoadPdfFileAndShowAsync(metadata, lastPage). Update PlaylistPositionText. Return early. (depends on T004, T013)
- [ ] T023 [US3] Handle boundary: when on the first page of the first playlist entry and navigating backward, do nothing (stay on current page). Verify that GoToPrevious() returns null and NavigateAsync falls through to normal clamping logic. (depends on T022)
- [ ] T024 [US3] Handle missing book during backward navigation in PdfViewerWindow.axaml.cs: same skip-and-notify pattern as T014 but scanning backward through entries. (depends on T022)

**Checkpoint**: Backward navigation works across playlist titles. All three user stories complete.

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Performance, edge cases, and validation

- [ ] T025 Add pre-caching for next title: in ShowPageAsync() in PdfViewerWindow.axaml.cs, when _playlistContext is not null and CurrentPageNumber is within 2 pages of the current title's last page, pre-load the next title's PDF volume bytes in the background via Task.Run(() => nextMetadata.GetOrLoadVolumeBytes(volumeNo)). This reduces transition latency for cross-book advances.
- [ ] T026 Clear _playlistContext when opening a title outside the playlist: in any code path that opens a PDF not via the playlist (e.g., direct book selection from Books/Query/Favorites tabs), set _playlistContext = null and update PlaylistPositionText to empty. Verify in ChooseMusicAsync() that ChosenPlaylist being null triggers context clearing.
- [ ] T028 Run quickstart.md validation checklist from specs/002-playlist-auto-advance/quickstart.md — verify all 13 items pass on at least one platform
- [ ] T029 [US1] Cross-feature: if feature 001 (half-page turn) is present, reset _isInSplitView to false before calling LoadPdfFileAndShowAsync() during playlist auto-advance/retreat. Ensure playlist boundary check runs before half-page turn alternation in NavigateAsync(). See specs/cross-feature-interactions.md. (depends on T013)

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 (T001) — BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Foundational (Phase 2) completion
- **User Story 2 (Phase 4)**: Depends on User Story 1 (T010, T013) for _playlistContext field and NavigateAsync interception
- **User Story 3 (Phase 5)**: Depends on User Story 1 (T013) for NavigateAsync interception pattern
- **Polish (Phase 6)**: Depends on all user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational — no cross-story deps
- **User Story 2 (P2)**: Depends on US1's _playlistContext field and NavigateAsync but independently testable
- **User Story 3 (P3)**: Depends on US1's NavigateAsync interception but independently testable. Can run in parallel with US2.

### Within Each User Story

- Tests written first (T008-T009 before T010-T016)
- Model/context before UI (T010-T012 before T013-T016)
- Core navigation before edge cases (T013 before T014, T016)
- UI indicator after navigation works (T015 after T013)

### Parallel Opportunities

- T001 and T002 can run in parallel (different files)
- T005, T006 can run in parallel within Phase 2 (different methods, same file OK)
- T008 and T009 can run in parallel (independent test methods)
- T021 can run in parallel with US3 implementation
- US2 (Phase 4) and US3 (Phase 5) can run in parallel after US1 is complete

---

## Parallel Example: User Story 1

```bash
# Launch setup tasks together:
Task T001: "Create PlaylistContext class in SheetMusicLib/PlaylistContext.cs"
Task T002: "Create test class skeleton in AvaloniaTests/Tests/PlaylistAutoAdvanceTests.cs"

# Launch US1 tests together (after foundation):
Task T008: "Unit tests for AdvanceToNext() and boundaries"
Task T009: "Unit tests for GetTitlePageRange()"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup (T001-T002)
2. Complete Phase 2: Foundational (T003-T007)
3. Complete Phase 3: User Story 1 (T008-T016)
4. **STOP and VALIDATE**: Auto-advance works forward through playlist with position indicator
5. Demo-ready with auto-advance always on (no toggle yet)

### Incremental Delivery

1. Setup + Foundational → PlaylistContext logic ready
2. User Story 1 → Forward auto-advance works with indicator (MVP!)
3. User Story 2 → Toggle on/off via menu added
4. User Story 3 → Backward navigation through playlist
5. Polish → Pre-caching, dual-page edge case, validation

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (blocking)
3. Once US1 is done:
   - Developer B: User Story 2
   - Developer C: User Story 3 (can run in parallel with US2)

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story is independently testable
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Total: 29 tasks (2 setup, 5 foundational, 10 US1, 4 US2, 4 US3, 4 polish)
