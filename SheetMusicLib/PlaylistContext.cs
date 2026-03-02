using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace SheetMusicLib
{
    public readonly record struct TitlePageRange(int FirstPage, int LastPage);

    /// <summary>
    /// Runtime context for navigating within an active playlist.
    /// </summary>
    public class PlaylistContext
    {
        public PlaylistContext(
            Playlist playlist,
            List<PdfMetaDataReadResult> pdfMetadataList,
            int currentEntryIndex = 0)
        {
            Playlist = playlist ?? throw new ArgumentNullException(nameof(playlist));
            PdfMetadataList = pdfMetadataList ?? throw new ArgumentNullException(nameof(pdfMetadataList));

            if (Playlist.Entries.Count == 0)
            {
                throw new ArgumentException("Playlist must contain at least one entry.", nameof(playlist));
            }

            CurrentEntryIndex = Math.Clamp(currentEntryIndex, 0, Playlist.Entries.Count - 1);
            AutoAdvanceEnabled = true;
        }

        public Playlist Playlist { get; }

        public List<PdfMetaDataReadResult> PdfMetadataList { get; }

        public int CurrentEntryIndex { get; private set; }

        public bool AutoAdvanceEnabled { get; set; }

        public PlaylistEntry CurrentEntry => Playlist.Entries[CurrentEntryIndex];

        public int TotalEntries => Playlist.Entries.Count;

        public bool HasNextEntry => CurrentEntryIndex < TotalEntries - 1;

        public bool HasPreviousEntry => CurrentEntryIndex > 0;

        public string PositionText => $"{CurrentEntryIndex + 1} / {TotalEntries}";

        public TitlePageRange GetTitlePageRange(PdfMetaDataReadResult metadata)
        {
            var fallback = new TitlePageRange(metadata.PageNumberOffset, metadata.MaxPageNum - 1);
            if (metadata.TocEntries == null || metadata.TocEntries.Count == 0)
            {
                return fallback;
            }

            var tocIndex = metadata.TocEntries.FindIndex(t => t.PageNo == CurrentEntry.PageNo);
            if (tocIndex < 0)
            {
                return fallback;
            }

            var firstPage = metadata.TocEntries[tocIndex].PageNo;
            var lastPage = metadata.GetLastPageOfTocEntry(tocIndex);
            return new TitlePageRange(firstPage, lastPage);
        }

        public PlaylistEntry? AdvanceToNext()
        {
            if (!HasNextEntry)
            {
                return null;
            }

            CurrentEntryIndex++;
            return CurrentEntry;
        }

        public PlaylistEntry? GoToPrevious()
        {
            if (!HasPreviousEntry)
            {
                return null;
            }

            CurrentEntryIndex--;
            return CurrentEntry;
        }

        public void UpdateContextForPage(int pageNo, PdfMetaDataReadResult metadata)
        {
            var currentBookName = NormalizeBookName(metadata.GetBookName());
            for (int i = 0; i < Playlist.Entries.Count; i++)
            {
                var entry = Playlist.Entries[i];
                if (!string.Equals(NormalizeBookName(entry.BookName), currentBookName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var range = GetTitlePageRangeForEntry(entry, metadata);
                if (pageNo >= range.FirstPage && pageNo <= range.LastPage)
                {
                    CurrentEntryIndex = i;
                    return;
                }
            }
        }

        private static string NormalizeBookName(string? name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            var normalized = name.Replace('\\', '/').Trim();
            if (normalized.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                normalized = normalized[..^4];
            }

            return Path.GetFileName(normalized);
        }

        private static TitlePageRange GetTitlePageRangeForEntry(PlaylistEntry entry, PdfMetaDataReadResult metadata)
        {
            var fallback = new TitlePageRange(metadata.PageNumberOffset, metadata.MaxPageNum - 1);
            if (metadata.TocEntries == null || metadata.TocEntries.Count == 0)
            {
                return fallback;
            }

            var tocIndex = metadata.TocEntries.FindIndex(t => t.PageNo == entry.PageNo);
            if (tocIndex < 0)
            {
                return fallback;
            }

            var firstPage = metadata.TocEntries[tocIndex].PageNo;
            var lastPage = metadata.GetLastPageOfTocEntry(tocIndex);
            return new TitlePageRange(firstPage, lastPage);
        }
    }
}
