using Microsoft.VisualStudio.TestTools.UnitTesting;
using SheetMusicLib;
using System.Collections.Generic;

namespace AvaloniaTests.Tests;

[TestClass]
public class PlaylistAutoAdvanceTests : TestBase
{
    [TestMethod]
    [TestCategory("Unit")]
    public void AdvanceToNext_AdvancesAndStopsAtEnd()
    {
        var context = CreateContext(currentIndex: 0);

        var next = context.AdvanceToNext();
        Assert.IsNotNull(next);
        Assert.AreEqual(1, context.CurrentEntryIndex);

        next = context.AdvanceToNext();
        Assert.IsNotNull(next);
        Assert.AreEqual(2, context.CurrentEntryIndex);

        next = context.AdvanceToNext();
        Assert.IsNull(next);
        Assert.AreEqual(2, context.CurrentEntryIndex);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void GoToPrevious_RetreatsAndStopsAtStart()
    {
        var context = CreateContext(currentIndex: 2);

        var previous = context.GoToPrevious();
        Assert.IsNotNull(previous);
        Assert.AreEqual(1, context.CurrentEntryIndex);

        previous = context.GoToPrevious();
        Assert.IsNotNull(previous);
        Assert.AreEqual(0, context.CurrentEntryIndex);

        previous = context.GoToPrevious();
        Assert.IsNull(previous);
        Assert.AreEqual(0, context.CurrentEntryIndex);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void GetTitlePageRange_MiddleTocEntry_ReturnsExpectedRange()
    {
        var metadata = CreateMetadata("BookA", tocPages: new[] { 0, 5, 10 }, totalPages: 15);
        var context = CreateContext(currentIndex: 1);

        var range = context.GetTitlePageRange(metadata);
        Assert.AreEqual(5, range.FirstPage);
        Assert.AreEqual(9, range.LastPage);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void GetTitlePageRange_LastTocEntry_EndsAtMaxPageMinusOne()
    {
        var metadata = CreateMetadata("BookA", tocPages: new[] { 0, 5, 10 }, totalPages: 15);
        var context = CreateContext(currentIndex: 2);

        var range = context.GetTitlePageRange(metadata);
        Assert.AreEqual(10, range.FirstPage);
        Assert.AreEqual(14, range.LastPage);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void GetTitlePageRange_NoMatchingToc_FallsBackToWholeBook()
    {
        var metadata = CreateMetadata("BookA", tocPages: new[] { 0, 5, 10 }, totalPages: 15);
        var playlist = new Playlist
        {
            Entries = new List<PlaylistEntry>
            {
                new() { SongName = "Unknown", BookName = "BookA", PageNo = 99 }
            }
        };
        var context = new PlaylistContext(playlist, new List<PdfMetaDataReadResult> { metadata });

        var range = context.GetTitlePageRange(metadata);
        Assert.AreEqual(0, range.FirstPage);
        Assert.AreEqual(14, range.LastPage);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void UpdateContextForPage_UpdatesIndexForMatchingBookAndRange()
    {
        var metadata = CreateMetadata("BookA", tocPages: new[] { 0, 5, 10 }, totalPages: 15);
        var context = CreateContext(currentIndex: 0);

        context.UpdateContextForPage(11, metadata);

        Assert.AreEqual(2, context.CurrentEntryIndex);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void AutoAdvanceToggle_PreservesContextIndex()
    {
        var context = CreateContext(currentIndex: 1);
        Assert.AreEqual(1, context.CurrentEntryIndex);

        context.AutoAdvanceEnabled = false;
        context.AutoAdvanceEnabled = true;

        Assert.IsTrue(context.AutoAdvanceEnabled);
        Assert.AreEqual(1, context.CurrentEntryIndex);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void GetLastPageOfTocEntry_HandlesMiddleLastAndFallback()
    {
        var metadata = CreateMetadata("BookA", tocPages: new[] { 0, 5, 10 }, totalPages: 15);
        Assert.AreEqual(4, metadata.GetLastPageOfTocEntry(0));
        Assert.AreEqual(14, metadata.GetLastPageOfTocEntry(2));
        Assert.AreEqual(14, metadata.GetLastPageOfTocEntry(99));
    }

    private static PlaylistContext CreateContext(int currentIndex)
    {
        var metadata = CreateMetadata("BookA", tocPages: new[] { 0, 5, 10 }, totalPages: 15);
        var playlist = new Playlist
        {
            Entries = new List<PlaylistEntry>
            {
                new() { SongName = "A", BookName = "BookA", PageNo = 0 },
                new() { SongName = "B", BookName = "BookA", PageNo = 5 },
                new() { SongName = "C", BookName = "BookA", PageNo = 10 }
            }
        };
        return new PlaylistContext(playlist, new List<PdfMetaDataReadResult> { metadata }, currentIndex);
    }

    private static PdfMetaDataReadResult CreateMetadata(string bookName, int[] tocPages, int totalPages)
    {
        var metadata = new PdfMetaDataReadResult
        {
            FullPathFile = $@"C:\music\{bookName}.pdf",
            PageNumberOffset = 0,
        };

        metadata.VolumeInfoList.Add(new PdfVolumeInfoBase
        {
            FileNameVolume = $"{bookName}.pdf",
            NPagesInThisVolume = totalPages
        });

        foreach (var tocPage in tocPages)
        {
            metadata.TocEntries.Add(new TOCEntry
            {
                SongName = $"Song {tocPage}",
                PageNo = tocPage
            });
        }

        return metadata;
    }
}
