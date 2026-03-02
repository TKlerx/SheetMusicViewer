# Feature Specification: Half-Page Turn Mode

**Feature Branch**: `001-half-page-turn`
**Created**: 2026-03-01
**Status**: Draft
**Input**: User description: "As a user I want to be able to set a new mode to get to the next page when viewing music sheets. When I am in single page mode and activate this new mode, when I click, it leaves the bottom part of the current page as is, but shows at the top already the first half of the next page. Hence, as a musician, I can directly continue on the next music sheet page without needing to click anything."

## Clarifications

### Session 2026-03-01

- Q: Should the split view use "Preview" layout (top = next page start, bottom = current page end) or "Reading Flow" layout (top = current page end, bottom = next page start), or both? → A: Both layouts. "Preview" (top = next page, bottom = current page) is the primary mode — it avoids visual disruption while reading the bottom of the current page. "Reading Flow" (top = current page end, bottom = next page start) is a secondary mode providing natural top-to-bottom reading continuity. The user selects which layout variant to use.
- Q: Should ink annotations be editable in split view, display-only, or hidden? → A: Display-only. Ink strokes are visible on both partial pages but cannot be edited. The user disables half-page turn mode to annotate.
- Q: Should all navigation methods (click/tap, keyboard, prev/next buttons) trigger half-page turns, or only click/tap? → A: All navigation methods. Click/tap, keyboard (Page Up/Down, arrow keys), and prev/next buttons all use the half-page turn alternation when the mode is active.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Activate Half-Page Turn Mode (Priority: P1)

As a musician viewing sheet music in single-page mode, I want to enable
a half-page turn mode so that each click shows a split view of two
adjacent pages. Two layout variants are available:

- **Preview layout** (default): The bottom half of the current page
  stays at the bottom of the screen, and the top half of the next page
  appears at the top. This avoids disrupting what I am currently
  reading — the music I am playing stays in place, and I can glance
  upward to preview what comes next.
- **Reading Flow layout**: The bottom half of the current page is shown
  at the top of the screen, and the top half of the next page appears
  at the bottom. This provides a natural top-to-bottom reading
  continuation.

**Why this priority**: This is the core value of the feature. Without
the split-view page turn, the feature has no purpose.

**Independent Test**: Open any multi-page PDF in single-page mode,
enable half-page turn mode, and click to advance. The display should
alternate between full-page and split-page views.

**Acceptance Scenarios**:

1. **Given** a PDF is open in single-page mode showing page N in full,
   **When** half-page turn mode is enabled with Preview layout and the
   user clicks to advance, **Then** the display shows a split view with
   the top half of page N+1 at the top of the screen and the bottom
   half of page N at the bottom of the screen.
2. **Given** a PDF is open in single-page mode showing page N in full,
   **When** half-page turn mode is enabled with Reading Flow layout and
   the user clicks to advance, **Then** the display shows a split view
   with the bottom half of page N at the top of the screen and the top
   half of page N+1 at the bottom of the screen.
3. **Given** the display is showing a split view (in either layout),
   **When** the user clicks to advance again, **Then** the display
   shows page N+1 in full.
4. **Given** half-page turn mode is enabled, **When** the user clicks
   repeatedly, **Then** the display alternates: full page → split view
   → full page → split view, progressing through the document.

---

### User Story 2 - Toggle Half-Page Turn On and Off (Priority: P2)

As a musician, I want to toggle half-page turn mode on or off and
choose which layout variant to use, so I can switch between standard
full-page turns, Preview layout, and Reading Flow layout depending on
the piece I am playing.

**Why this priority**: Usability depends on easy toggling. Some pieces
may not benefit from half-page turns, and different musicians may
prefer different layouts.

**Independent Test**: While viewing a PDF, toggle half-page turn mode
on and off via the menu and switch between layout variants. Verify that
the mode change takes effect immediately on the next click.

**Acceptance Scenarios**:

1. **Given** a PDF is open in single-page mode with half-page turn
   disabled, **When** the user enables half-page turn mode via the
   menu, **Then** the next forward click produces a split view using
   the selected layout variant.
2. **Given** half-page turn mode is active with Preview layout,
   **When** the user switches to Reading Flow layout, **Then** the
   next forward click uses the Reading Flow layout.
3. **Given** half-page turn mode is active and the display shows a
   split view, **When** the user disables half-page turn mode,
   **Then** the display immediately returns to showing the current
   page in full, and subsequent clicks perform standard full-page
   turns.
4. **Given** the application is in dual-page mode, **When** the user
   attempts to enable half-page turn mode, **Then** the option is
   unavailable or visually disabled, since half-page turn only applies
   to single-page mode.

---

### User Story 3 - Persist Half-Page Turn Preference (Priority: P3)

As a musician, I want the application to remember my half-page turn
preference and selected layout variant so I do not have to reconfigure
it every time I open the application.

**Why this priority**: Convenience feature. Musicians who prefer
half-page turns should not need to toggle it on every session, but
the feature works without persistence.

**Independent Test**: Enable half-page turn mode with Reading Flow
layout, close and reopen the application, verify the mode and layout
variant are still active.

**Acceptance Scenarios**:

1. **Given** the user enables half-page turn mode with a specific
   layout variant, **When** the application is closed and reopened,
   **Then** half-page turn mode is still enabled with the same layout
   variant.
2. **Given** the user disables half-page turn mode, **When** the
   application is closed and reopened, **Then** half-page turn mode
   remains disabled.

---

### Edge Cases

- What happens when the user is on the last page and clicks forward
  in half-page turn mode? The split view should not be shown if there
  is no next page; the display remains on the last full page.
- What happens when the user navigates backward in half-page turn
  mode? Backward navigation should reverse the sequence: from a full
  page, go to the split view showing the adjacent halves of the
  previous and current pages (layout variant determines arrangement);
  from a split view, go to the previous full page.
- What happens when the user jumps to a specific page (via slider or
  page number input) while in half-page turn mode? The display should
  reset to show the target page in full, not in a split view.
- What happens when the user switches from single-page to dual-page
  mode while half-page turn is enabled? Half-page turn should be
  automatically disabled or hidden, since it only applies to
  single-page mode.
- What happens if the page is very short (e.g., a single-system page)?
  The split view still divides the display area in half, regardless of
  content height.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: The system MUST provide a half-page turn mode that is
  only available when viewing in single-page mode.
- **FR-002**: The system MUST support two split-view layout variants:
  - **Preview layout** (default): bottom half of the current page at
    the bottom of the screen, top half of the next page at the top.
  - **Reading Flow layout**: bottom half of the current page at the
    top of the screen, top half of the next page at the bottom.
- **FR-003**: When half-page turn mode is active and the user advances
  forward via any navigation method (click/tap, keyboard, or prev/next
  buttons), the display MUST alternate between showing a full page and
  showing a split view using the selected layout variant.
- **FR-004**: When half-page turn mode is active and the user navigates
  backward, the display MUST reverse the alternating sequence.
- **FR-005**: The system MUST disable or hide the half-page turn option
  when the viewer is in dual-page mode.
- **FR-006**: When the user is on the last page, a forward click in
  half-page turn mode MUST NOT produce a split view; the last page
  remains displayed in full.
- **FR-007**: When the user jumps to a specific page (slider, page
  number entry, or TOC link), the display MUST reset to a full-page
  view of the target page.
- **FR-008**: The system MUST persist the user's half-page turn
  preference (enabled/disabled and selected layout variant) across
  application sessions.
- **FR-009**: The half-page turn mode and layout variant MUST be
  accessible via the application menu, consistent with the existing
  page-mode toggle.
- **FR-010**: Page caching MUST pre-render the next page when in
  half-page turn mode so the split view appears without perceptible
  delay.

### Key Entities

- **ViewState**: Tracks whether the current display is in "full page"
  or "split view" state. Resets to "full page" on direct page jumps.
- **HalfPageTurnSetting**: User preference containing: enabled
  (boolean) and layout variant (Preview or Reading Flow). Persisted
  across sessions.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: A musician can advance through a 50-page PDF using
  half-page turn mode without any perceptible delay between clicks
  (split view appears within the same responsiveness threshold as
  standard page turns).
- **SC-002**: 100% of forward and backward navigation clicks produce
  the correct alternating full/split display sequence for the selected
  layout variant.
- **SC-003**: Toggling half-page turn mode on or off, or switching
  layout variant, takes effect on the very next user interaction, with
  no stale display state.
- **SC-004**: The half-page turn preference and layout variant survive
  application restart without user re-configuration.
- **SC-005**: Half-page turn mode is automatically unavailable when
  dual-page mode is active, preventing user confusion.

## Assumptions

- The split view divides the display area into two equal vertical
  halves (50/50 split), regardless of the actual content layout on
  each page.
- Ink annotations are displayed (but not editable) on both the partial
  current page and the partial next page in the split view. The user
  must disable half-page turn mode to draw or edit annotations.
- The page number indicator reflects the primary page being viewed
  (the page whose bottom half is visible in a split view, or the full
  page in a full view).
- Favorites and bookmark toggles apply to the full page currently in
  context, not to the partial page visible in the split view.
