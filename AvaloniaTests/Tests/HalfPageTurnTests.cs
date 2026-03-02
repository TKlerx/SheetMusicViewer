using Microsoft.VisualStudio.TestTools.UnitTesting;
using SheetMusicLib;
using System;
using System.IO;

namespace AvaloniaTests.Tests;

[TestClass]
public class HalfPageTurnTests : TestBase
{
    private readonly record struct NavState(int PageNo, bool IsInSplitView);

    [TestMethod]
    [TestCategory("Unit")]
    public void HalfPageTurnLayout_EnumValues_AreStable()
    {
        Assert.AreEqual(0, (int)HalfPageTurnLayout.Preview);
        Assert.AreEqual(1, (int)HalfPageTurnLayout.ReadingFlow);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void HalfPageTurnLayout_Parse_RoundTrips()
    {
        Assert.IsTrue(Enum.TryParse<HalfPageTurnLayout>("Preview", out var preview));
        Assert.AreEqual(HalfPageTurnLayout.Preview, preview);

        Assert.IsTrue(Enum.TryParse<HalfPageTurnLayout>("ReadingFlow", out var readingFlow));
        Assert.AreEqual(HalfPageTurnLayout.ReadingFlow, readingFlow);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void HalfPageNavigation_AlternatesAsExpected()
    {
        var offset = 0;
        var lastPage = 9;

        var state = new NavState(2, false);
        state = NavigateHalfPage(state, +1, offset, lastPage);
        Assert.IsTrue(state.IsInSplitView);
        Assert.AreEqual(2, state.PageNo);

        state = NavigateHalfPage(state, +1, offset, lastPage);
        Assert.IsFalse(state.IsInSplitView);
        Assert.AreEqual(3, state.PageNo);

        state = NavigateHalfPage(state, -1, offset, lastPage);
        Assert.IsTrue(state.IsInSplitView);
        Assert.AreEqual(2, state.PageNo);

        state = NavigateHalfPage(state, -1, offset, lastPage);
        Assert.IsFalse(state.IsInSplitView);
        Assert.AreEqual(2, state.PageNo);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void HalfPageLayout_OrderIsCorrectForBothVariants()
    {
        var preview = ResolveLayout(HalfPageTurnLayout.Preview);
        Assert.AreEqual("next-top", preview.Top);
        Assert.AreEqual("current-bottom", preview.Bottom);

        var readingFlow = ResolveLayout(HalfPageTurnLayout.ReadingFlow);
        Assert.AreEqual("current-bottom", readingFlow.Top);
        Assert.AreEqual("next-top", readingFlow.Bottom);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void AppSettings_PersistsHalfPageTurnSettings()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"HalfPageTurnTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        var localPath = Path.Combine(tempDir, "settings.json");
        try
        {
            AppSettings.ResetForTesting(localPath);
            var settings = AppSettings.Instance;
            settings.HalfPageTurnEnabled = true;
            settings.HalfPageTurnLayout = HalfPageTurnLayout.ReadingFlow.ToString();
            settings.HalfPageBackwardHalfStep = true;
            settings.SaveLocal();

            AppSettings.ResetForTesting(localPath);
            var loaded = AppSettings.Instance;

            Assert.IsTrue(loaded.HalfPageTurnEnabled);
            Assert.AreEqual("ReadingFlow", loaded.HalfPageTurnLayout);
            Assert.IsTrue(loaded.HalfPageBackwardHalfStep);
        }
        finally
        {
            AppSettings.ResetForTesting();
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    private static NavState NavigateHalfPage(NavState state, int delta, int offset, int lastPage)
    {
        if (delta > 0)
        {
            if (!state.IsInSplitView)
            {
                return state.PageNo >= lastPage ? new NavState(state.PageNo, false) : new NavState(state.PageNo, true);
            }

            return new NavState(Math.Min(state.PageNo + 1, lastPage), false);
        }

        if (delta < 0)
        {
            if (state.IsInSplitView)
            {
                return new NavState(state.PageNo, false);
            }

            return state.PageNo <= offset ? new NavState(state.PageNo, false) : new NavState(state.PageNo - 1, true);
        }

        return state;
    }

    private static (string Top, string Bottom) ResolveLayout(HalfPageTurnLayout layout)
    {
        return layout == HalfPageTurnLayout.Preview
            ? ("next-top", "current-bottom")
            : ("current-bottom", "next-top");
    }
}
