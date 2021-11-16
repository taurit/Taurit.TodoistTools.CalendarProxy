using Microsoft.VisualStudio.TestTools.UnitTesting;
using Taurit.TodoistTools.CalendarProxy.Library.Helpers;
// ReSharper disable StringLiteralTypo

namespace Taurit.TodoistTools.CalendarProxy.Library.Tests;

[TestClass]
public class EventLengthFinderTests
{
    [TestMethod]
    public void RegexShouldMatch1()
    {
        EventLengthFinder elf = new EventLengthFinder("Review english lesson @home 20m");
        Assert.AreEqual(elf.PatternFound, true);
    }

    [TestMethod]
    public void CalculateTime1()
    {
        EventLengthFinder elf = new EventLengthFinder("Review english lesson @home 20m");
        Assert.AreEqual(elf.TotalMinutes, 20);
    }

    [TestMethod]
    public void CutRegex1()
    {
        EventLengthFinder elf = new EventLengthFinder("Review english lesson @home 20m");
        Assert.AreEqual(elf.TaskSummaryWithoutPattern, "Review english lesson @home");
    }


    [TestMethod]
    public void RegexShouldMatch2()
    {
        EventLengthFinder elf = new EventLengthFinder("@home Read for 45 minutes");
        Assert.AreEqual(elf.PatternFound, true);
    }

    [TestMethod]
    public void CalculateTime2()
    {
        EventLengthFinder elf = new EventLengthFinder("@home Read for 45 minutes");
        Assert.AreEqual(elf.TotalMinutes, 45);
    }

    [TestMethod]
    public void CutRegex2()
    {
        EventLengthFinder elf = new EventLengthFinder("@home Read for 45 minutes");
        Assert.AreEqual(elf.TaskSummaryWithoutPattern, "@home Read for");
    }


    [TestMethod]
    public void RegexShouldMatch3()
    {
        EventLengthFinder elf = new EventLengthFinder("Exercise @gym 1 H");
        Assert.AreEqual(elf.PatternFound, true);
    }

    [TestMethod]
    public void CalculateTime3()
    {
        EventLengthFinder elf = new EventLengthFinder("Exercise @gym 1 H");
        Assert.AreEqual(elf.TotalMinutes, 60);
    }

    [TestMethod]
    public void CutRegex3()
    {
        EventLengthFinder elf = new EventLengthFinder("Exercise @gym 1 H");
        Assert.AreEqual(elf.TaskSummaryWithoutPattern, "Exercise @gym");
    }


    [TestMethod]
    public void RegexShouldMatch4()
    {
        EventLengthFinder elf = new EventLengthFinder("Meditate 0.5h @home");
        Assert.AreEqual(elf.PatternFound, true);
    }

    [TestMethod]
    public void CalculateTime4()
    {
        EventLengthFinder elf = new EventLengthFinder("Meditate 0.5h @home");
        Assert.AreEqual(elf.TotalMinutes, 30);
    }

    [TestMethod]
    public void CutRegex4()
    {
        EventLengthFinder elf = new EventLengthFinder("Meditate 0.5h @home");
        Assert.AreEqual(elf.TaskSummaryWithoutPattern, "Meditate @home");
    }

    [TestMethod]
    public void RegexShouldMatch5()
    {
        EventLengthFinder elf = new EventLengthFinder("Play drums (2h 30 min) @home");
        Assert.AreEqual(elf.PatternFound, true);
    }

    [TestMethod]
    public void CalculateTime5()
    {
        EventLengthFinder elf = new EventLengthFinder("Play drums (2h 30 min) @home");
        Assert.AreEqual(elf.TotalMinutes, 2 * 60 + 30);
    }

    [TestMethod]
    public void CutRegex5()
    {
        EventLengthFinder elf = new EventLengthFinder("Play drums (2h 30 min) @home");
        Assert.AreEqual(elf.TaskSummaryWithoutPattern, "Play drums @home");
    }

    [TestMethod]
    public void RegexShouldMatch6()
    {
        EventLengthFinder elf = new EventLengthFinder("Czas po polsku (5 minut) @home");
        Assert.AreEqual(elf.PatternFound, true);
    }

    [TestMethod]
    public void CalculateTime6()
    {
        EventLengthFinder elf = new EventLengthFinder("Czas po polsku (5 minut) @home");
        Assert.AreEqual(elf.TotalMinutes, 5);
    }

    [TestMethod]
    public void CutRegex6()
    {
        EventLengthFinder elf = new EventLengthFinder("Czas po polsku (5 minut) @home");
        Assert.AreEqual(elf.TaskSummaryWithoutPattern, "Czas po polsku @home");
    }

    [TestMethod]
    public void RegexShouldMatch7()
    {
        EventLengthFinder elf = new EventLengthFinder("Some short thing (5m)");
        Assert.AreEqual(elf.PatternFound, true);
    }

    [TestMethod]
    public void CalculateTime7()
    {
        EventLengthFinder elf = new EventLengthFinder("Some short thing (5m)");
        Assert.AreEqual(elf.TotalMinutes, 5);
    }

    [TestMethod]
    public void CutRegex7()
    {
        EventLengthFinder elf = new EventLengthFinder("Some short thing (5m)");
        Assert.AreEqual(elf.TaskSummaryWithoutPattern, "Some short thing");
    }

    [TestMethod]
    public void RegexShouldMatch8()
    {
        EventLengthFinder elf = new EventLengthFinder("Some short thing [5m]");
        Assert.AreEqual(elf.PatternFound, true);
    }

    [TestMethod]
    public void CalculateTime8()
    {
        EventLengthFinder elf = new EventLengthFinder("Some short thing [5m]");
        Assert.AreEqual(elf.TotalMinutes, 5);
    }

    [TestMethod]
    public void CutRegex8()
    {
        EventLengthFinder elf = new EventLengthFinder("Some short thing [5m]");
        Assert.AreEqual(elf.TaskSummaryWithoutPattern, "Some short thing");
    }


    [TestMethod]
    public void RegexShouldNotMatch1()
    {
        EventLengthFinder elf = new EventLengthFinder("@market Buy a car");
        Assert.AreEqual(elf.PatternFound, false);
    }


    [TestMethod]
    public void RegexShouldNotMatch2()
    {
        EventLengthFinder elf = new EventLengthFinder("Buy 3 milk bottles @market");
        Assert.AreEqual(elf.PatternFound, false);
    }

    [TestMethod]
    public void RegexShouldNotMatch3()
    {
        EventLengthFinder elf = new EventLengthFinder("123 45 6");
        Assert.AreEqual(elf.PatternFound, false);
    }

    [TestMethod]
    public void RegexShouldNotMatch4()
    {
        EventLengthFinder elf = new EventLengthFinder("Check Philippians 4:11 - 4:12");
        Assert.AreEqual(elf.PatternFound, false);
    }
    
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void NoTimeSpecified1()
    {
        EventLengthFinder elf = new EventLengthFinder("No time specified in this string");
        int totalMinutes = elf.TotalMinutes;
    }
}
