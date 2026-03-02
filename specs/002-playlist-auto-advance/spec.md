# Feature Specification: Playlist Auto-Advance

**Feature Branch**: `002-playlist-auto-advance`
**Created**: 2026-03-01
**Status**: Draft
**Input**: User description: "As a musician I when I have created a playlist (like the music sheets I need for a concert for the different titles), I want that when being on the last page of a title in the playlist and clicking, that the first page of the next title of the playlist is shown. This mode shall be enabled optionally."

## Clarifications

### Session 2026-03-01

- Q: When the user jumps to a page belonging to a different title in the same book, should the playlist context update to that title? → A: Yes. The playlist context updates to the title containing the jumped-to page (if that title exists in the playlist). This ensures auto-advance triggers at the correct title boundary.
- Q: Should the viewer show a visual indicator of the current playlist position during auto-advance? → A: Yes. A small persistent indicator (e.g., "3 / 12") is shown in the viewer status area whenever a playlist context is active.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Auto-Advance to Next Playlist Title (Priority: P1)

As a musician performing a concert, I have a playlist of titles I need
to play in order. When I reach the last page of the current title and
click forward, the viewer should automatically open the first page of
the next title in the playlist. This way I can move seamlessly from
one piece to the next without returning to the playlist screen to
manually select the next song.

**Why this priority**: This is the core value of the feature. Without
automatic advancement, the musician must interrupt their performance to
manually navigate to the next piece.

**Independent Test**: Create a playlist with at least 3 titles from
different books. Open the first title, navigate to its last page, and
click forward. The viewer should load the first page of the second
title. Repeat for the transition between the second and third titles.

**Acceptance Scenarios**:

1. **Given** a playlist is active with auto-advance enabled and the
   user is viewing the last page of title N, **When** the user clicks
   forward, **Then** the viewer opens the first page of title N+1 from
   the playlist.
2. **Given** a playlist is active with auto-advance enabled and the
   user is viewing the last page of the final title in the playlist,
   **When** the user clicks forward, **Then** the viewer stays on the
   current page (no wrap-around to the beginning of the playlist).
3. **Given** a playlist is active with auto-advance enabled and the
   user is not on the last page of the current title, **When** the
   user clicks forward, **Then** normal page navigation occurs (next
   page within the same title).

---

### User Story 2 - Enable and Disable Auto-Advance Mode (Priority: P2)

As a musician, I want to toggle auto-advance mode on or off so that I
can choose whether the viewer automatically progresses through my
playlist or stops at the end of each title (for practice sessions where
I may want to repeat a piece).

**Why this priority**: The user explicitly requested this mode be
optional. Without a toggle, the feature could be disruptive when the
musician wants to stay on the current title.

**Independent Test**: Open a playlist title, toggle auto-advance off,
navigate to the last page, and click forward. The viewer should stay on
the last page. Toggle auto-advance on and click forward again — the
viewer should advance to the next title.

**Acceptance Scenarios**:

1. **Given** a playlist is active, **When** the user enables
   auto-advance mode via the menu, **Then** the next forward
   navigation past the last page of a title advances to the next
   playlist title.
2. **Given** a playlist is active with auto-advance enabled, **When**
   the user disables auto-advance mode, **Then** forward navigation
   past the last page of a title behaves as it does today (stays on
   the last page or follows existing behavior).
3. **Given** no playlist is active, **When** the user views the
   auto-advance option, **Then** the option is unavailable or
   visually disabled.

---

### User Story 3 - Navigate Backward Through Playlist (Priority: P3)

As a musician, I want backward navigation to also work across playlist
titles so that if I am on the first page of a title and click backward,
the viewer shows the last page of the previous title in the playlist.

**Why this priority**: Symmetric navigation is expected but not
explicitly requested. Forward auto-advance is the critical need;
backward is a natural complement.

**Independent Test**: Open the second title in a playlist via
auto-advance, then click backward from the first page. The viewer
should show the last page of the previous title.

**Acceptance Scenarios**:

1. **Given** a playlist is active with auto-advance enabled and the
   user is on the first page of title N (where N > 1), **When** the
   user clicks backward, **Then** the viewer opens the last page of
   title N-1 from the playlist.
2. **Given** a playlist is active with auto-advance enabled and the
   user is on the first page of the first title, **When** the user
   clicks backward, **Then** the viewer stays on the current page.

---

### Edge Cases

- What happens when a playlist title references a book or page that no
  longer exists (e.g., the PDF was deleted or moved)? The viewer should
  skip that title and advance to the next valid title, displaying a
  brief notification that a title was skipped.
- What happens when the current title spans multiple volumes in a
  multi-volume book? The "last page of a title" is determined by the
  Table of Contents, regardless of volume boundaries. Auto-advance
  triggers only after the actual last page of the song.
- What happens when a playlist contains only one title? Auto-advance
  has no effect; forward navigation past the last page stays on the
  last page.
- What happens when auto-advance is enabled but the user jumps to a
  specific page via slider or page number input? The playlist context
  updates to the title containing the jumped-to page (if that title
  exists in the active playlist). Auto-advance then applies from that
  title's position onward. If the jumped-to page does not belong to
  any playlist title, the playlist context is preserved at its
  previous position.
- What happens when the user opens a title that is not in any playlist?
  Auto-advance does not activate because there is no playlist context.
- What happens when a title has no TOC entry (so its page range is
  unknown)? The entire book is treated as a single title; auto-advance
  triggers after the last page of the book.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST provide an auto-advance mode that
  automatically opens the next playlist title when the user navigates
  past the last page of the current title.
- **FR-002**: Auto-advance MUST only be active when the user has opened
  a title from a playlist (playlist context is set).
- **FR-003**: The system MUST determine the last page of a title using
  the Table of Contents: the last page is the page before the next
  TOC entry's start page, or the last page of the book if the title
  is the final TOC entry.
- **FR-004**: When auto-advance is enabled and the user reaches the
  last page of the final title in the playlist, forward navigation
  MUST NOT wrap around; the viewer stays on the last page.
- **FR-005**: The system MUST provide a toggle to enable or disable
  auto-advance mode, accessible via the application menu.
- **FR-006**: The auto-advance toggle MUST be unavailable or disabled
  when no playlist is active.
- **FR-007**: When auto-advance is disabled mid-session, the playlist
  context MUST be preserved so re-enabling auto-advance resumes from
  the current position in the playlist.
- **FR-008**: If a playlist title references a book or page that cannot
  be loaded, the system MUST skip to the next valid title and notify
  the user.
- **FR-009**: Auto-advance MUST work with both single-page and
  dual-page viewing modes.
- **FR-010**: When the user jumps to a specific page, the system MUST
  update the playlist context to the title containing that page (if the
  title exists in the active playlist). If the page does not belong to
  any playlist title, the previous playlist context MUST be preserved.
- **FR-011**: When a playlist context is active, the viewer MUST
  display a persistent playlist position indicator (e.g., "3 / 12")
  showing the current title index and total titles in the playlist.
  The indicator MUST update when the current title changes.
- **FR-012**: The transition to the next playlist title MUST appear
  without perceptible delay, using the existing page caching system
  to pre-render the first page of the next title.
- **FR-013**: When auto-advance is enabled and the user navigates
  backward from the first page of a title (where the title is not the
  first in the playlist), the system MUST open the last page of the
  previous playlist title. If the title is the first in the playlist,
  backward navigation MUST stay on the current page.

### Key Entities

- **Playlist Context**: Tracks which playlist is active, the current
  title index within the playlist, and whether auto-advance is enabled.
  Set when a title is opened from a playlist; cleared when the user
  opens a title outside the playlist.
- **Title Page Range**: The start and end page of a title within a
  book, derived from the Table of Contents entries. Used to determine
  when auto-advance should trigger.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A musician can play through an entire concert playlist
  (10+ titles across multiple books) by only clicking forward, without
  ever returning to the playlist screen.
- **SC-002**: The transition between playlist titles appears within
  the same responsiveness threshold as a normal page turn.
- **SC-003**: Toggling auto-advance on or off takes effect on the very
  next forward navigation, with no stale state.
- **SC-004**: When auto-advance is disabled, forward navigation at the
  end of a title behaves identically to the current application
  behavior (no regression).
- **SC-005**: Skipped titles (due to missing books or pages) produce a
  visible notification so the musician is aware of the issue.
- **SC-006**: The playlist position indicator is visible at all times
  when a playlist context is active, and correctly reflects the current
  title index after every navigation and page jump.
- **SC-007**: Backward navigation from the first page of any non-first
  playlist title opens the last page of the previous title with the
  same responsiveness as a normal page turn.

## Assumptions

- A "title" in the playlist corresponds to a single song identified by
  its TOC entry. The page range of a title is from its TOC page number
  to one page before the next TOC entry (or the last page of the book
  for the final entry).
- The playlist order defines the performance order. Titles are advanced
  in the order they appear in the playlist, not alphabetically or by
  any other sort.
- Opening a title from the playlist (via double-click or context menu
  in the playlist UI) establishes the playlist context. Opening a title
  through any other means (books tab, query, favorites) does not set a
  playlist context.
- The auto-advance preference is a session-level toggle (not persisted
  across application restarts), since concert setups are typically
  configured fresh each time. If persistence is desired, it can be
  added later.
