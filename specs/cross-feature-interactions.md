# Cross-Feature Interactions

## Half-Page Turn (001) + Playlist Auto-Advance (002)

Both features modify `NavigateAsync()` in `PdfViewerWindow.axaml.cs`.
When both are active simultaneously, the following priority applies.

### Interception Order in NavigateAsync

1. **Playlist boundary check FIRST**: Before half-page turn alternation,
   check whether the navigation would cross a playlist title boundary.
2. **Half-page turn alternation SECOND**: If no playlist boundary is
   crossed, apply half-page turn full/split alternation as normal.

### Specific Scenarios

| State | Action | Result |
|-------|--------|--------|
| Full page, last page of title, forward | Auto-advance to next title's first page (full view). Split state resets to full. |
| Split view showing halves of last page + next page, forward | Complete split cycle to full page of last page, then next forward triggers auto-advance. |
| Full page, first page of title, backward | Auto-retreat to previous title's last page (full view). Split state resets to full. |

### Implementation Rule

When a playlist boundary transition occurs (auto-advance or auto-retreat):
- `_isInSplitView` MUST be reset to `false`
- The target page MUST display in full-page view
- This matches FR-007 (001): "page jumps reset to full-page view"

### Task Impact

- **001/T008** (NavigateAsync modification): Must yield to playlist
  boundary check when `_playlistContext` is active
- **002/T013** (NavigateAsync interception): Must reset `_isInSplitView`
  to `false` when triggering a title transition
- **002/T029**: Implements this cross-feature interaction
- **Merge order**: Implement 001 first, then 002 on top. Feature 002's
  NavigateAsync changes wrap around 001's alternation logic.
