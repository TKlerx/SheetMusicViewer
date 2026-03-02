<!--
  Sync Impact Report
  ==================
  Version change: N/A → 1.0.0 (initial ratification)

  Added principles:
    - I. Performance-First
    - II. Cross-Platform
    - III. Non-Destructive
    - IV. Library-First
    - V. Test-First

  Added sections:
    - Core Principles (5 principles)
    - Technical Constraints
    - Development Workflow & Quality Gates
    - Governance

  Templates reviewed:
    - .specify/templates/plan-template.md ✅ aligned (Constitution Check
      section supports gate validation against these principles)
    - .specify/templates/spec-template.md ✅ aligned (priority-based user
      stories and independent testability match Test-First principle)
    - .specify/templates/tasks-template.md ✅ aligned (phase structure and
      parallel execution compatible with all principles)
    - .specify/templates/commands/*.md — no files present, nothing to update

  Deferred items: None
-->

# SheetMusicViewer Constitution

## Core Principles

### I. Performance-First

Instant page turns are non-negotiable. Musicians MUST never wait for a
page to render during a performance.

- All page rendering MUST use intelligent caching of upcoming pages
- Page turn latency MUST be imperceptible to the user (<50ms perceived)
- Background pre-rendering of adjacent pages MUST be active whenever a
  document is open
- New features MUST NOT degrade page-turn performance; any feature that
  adds rendering latency MUST be profiled and justified

**Rationale**: The primary user (a performing musician) cannot pause to
wait for software. A slow page turn can disrupt a live performance.

### II. Cross-Platform

The application MUST run on Windows, macOS, and Linux using a single
shared codebase built on Avalonia UI.

- All UI MUST be implemented with Avalonia 11.x controls and layouts
- Platform-specific code MUST be isolated behind abstractions and
  justified in writing
- Every feature MUST be validated on all three target platforms via CI
- Self-contained single-file executables MUST be produced for each
  platform (Windows x64/x86/ARM, macOS arm64, Linux x64)

**Rationale**: Musicians use diverse hardware. A single cross-platform
codebase reduces maintenance burden and ensures consistent behavior.

### III. Non-Destructive

Original PDF files MUST never be modified by the application.

- Annotations (ink, highlights, bookmarks) MUST be stored in separate
  sidecar files, never written back into the PDF
- Metadata (favorites, last page, TOC overrides) MUST be persisted
  independently of the source PDF
- Any operation that could alter a PDF MUST be blocked at the API level
- Users MUST be able to delete all application data without affecting
  their original music files

**Rationale**: Sheet music PDFs are often irreplaceable scans or
purchased downloads. Data loss from accidental modification is
unacceptable.

### IV. Library-First

Core business logic MUST reside in `SheetMusicLib`; the UI layer MUST
be a thin presentation layer.

- Domain logic (PDF metadata, bookmarks, settings, serialization) MUST
  live in `SheetMusicLib` with no UI dependencies
- `SheetMusicLib` MUST be independently testable without launching a UI
- The desktop project (`SheetMusicViewer.Desktop`) MUST only contain
  view models, controls, and platform integration code
- New features MUST place logic in `SheetMusicLib` first and expose it
  to the UI through well-defined interfaces

**Rationale**: Separating logic from UI enables thorough unit testing,
supports potential future UI targets, and keeps the codebase navigable.

### V. Test-First

Tests MUST accompany all new features and bug fixes.

- Unit tests MUST cover core logic in `SheetMusicLib`
- Integration tests MUST cover interactions between components (PDF
  loading, file I/O, multi-volume handling)
- All tests MUST pass before a PR can be merged
- Test failures MUST be investigated and resolved, never skipped or
  suppressed

**Rationale**: The application handles user data (annotations,
bookmarks) and must perform reliably during live use. Regressions in
page rendering or data persistence are high-severity issues.

## Technical Constraints

- **Language/Runtime**: C# on .NET 10.0
- **UI Framework**: Avalonia 11.x with Fluent theme
- **MVVM**: CommunityToolkit.MVVM for observable properties and commands
- **PDF Rendering**: PDFtoImage 5.x backed by SkiaSharp 3.x
- **Serialization**: System.Text.Json for bookmark/metadata persistence
- **Build Output**: Self-contained, single-file executables per platform
- **CI/CD**: GitHub Actions building on Windows, macOS, and Linux runners

New dependencies MUST be justified and MUST NOT conflict with
cross-platform or single-file deployment requirements.

## Development Workflow & Quality Gates

- **Spec-Driven**: Features follow the Spec → Plan → Tasks →
  Implementation flow using the speckit workflow
- **Feature Branches**: All work happens on feature branches; direct
  commits to `master` are prohibited
- **CI Gate**: GitHub Actions MUST pass (build + test on all platforms)
  before merge
- **Code Review**: PRs MUST be reviewed before merge
- **No PDF Mutation**: Any code path that opens a PDF for writing MUST
  be flagged and rejected in review
- **Performance Check**: Features touching rendering or page navigation
  MUST include before/after performance notes in the PR description

## Governance

This constitution is the authoritative guide for development decisions
in SheetMusicViewer. All contributors and AI agents MUST comply.

- **Amendments**: Changes to this constitution require documentation of
  the rationale, a version bump following semantic versioning, and
  review by the project maintainer.
- **Versioning**: MAJOR for principle removals or redefinitions, MINOR
  for new principles or material expansions, PATCH for clarifications
  and typo fixes.
- **Compliance**: PRs and code reviews MUST verify alignment with these
  principles. The plan template's "Constitution Check" section MUST
  reference the active principles by number.
- **Conflict Resolution**: When ambiguity arises, Performance-First
  (Principle I) and Non-Destructive (Principle III) take precedence.

**Version**: 1.0.0 | **Ratified**: 2026-03-01 | **Last Amended**: 2026-03-01
