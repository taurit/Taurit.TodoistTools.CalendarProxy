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
        Assert.IsTrue(elf.PatternFound);
    }

    [TestMethod]
    public void CalculateTime1()
    {
        EventLengthFinder elf = new EventLengthFinder("Review english lesson @home 20m");
        Assert.AreEqual(20, elf.TotalMinutes);
    }

    [TestMethod]
    public void CutRegex1()
    {
        EventLengthFinder elf = new EventLengthFinder("Review english lesson @home 20m");
        Assert.AreEqual("Review english lesson @home", elf.TaskSummaryWithoutPattern);
    }


    [TestMethod]
    public void RegexShouldMatch2()
    {
        EventLengthFinder elf = new EventLengthFinder("@home Read for 45 minutes");
        Assert.IsTrue(elf.PatternFound);
    }

    [TestMethod]
    public void CalculateTime2()
    {
        EventLengthFinder elf = new EventLengthFinder("@home Read for 45 minutes");
        Assert.AreEqual(45, elf.TotalMinutes);
    }

    [TestMethod]
    public void CutRegex2()
    {
        EventLengthFinder elf = new EventLengthFinder("@home Read for 45 minutes");
        Assert.AreEqual("@home Read for", elf.TaskSummaryWithoutPattern);
    }


    [TestMethod]
    public void RegexShouldMatch3()
    {
        EventLengthFinder elf = new EventLengthFinder("Exercise @gym 1 H");
        Assert.IsTrue(elf.PatternFound);
    }

    [TestMethod]
    public void CalculateTime3()
    {
        EventLengthFinder elf = new EventLengthFinder("Exercise @gym 1 H");
        Assert.AreEqual(60, elf.TotalMinutes);
    }

    [TestMethod]
    public void CutRegex3()
    {
        EventLengthFinder elf = new EventLengthFinder("Exercise @gym 1 H");
        Assert.AreEqual("Exercise @gym", elf.TaskSummaryWithoutPattern);
    }


    [TestMethod]
    public void RegexShouldMatch4()
    {
        EventLengthFinder elf = new EventLengthFinder("Meditate 0.5h @home");
        Assert.IsTrue(elf.PatternFound);
    }

    [TestMethod]
    public void CalculateTime4()
    {
        EventLengthFinder elf = new EventLengthFinder("Meditate 0.5h @home");
        Assert.AreEqual(30, elf.TotalMinutes);
    }

    [TestMethod]
    public void CutRegex4()
    {
        EventLengthFinder elf = new EventLengthFinder("Meditate 0.5h @home");
        Assert.AreEqual("Meditate @home", elf.TaskSummaryWithoutPattern);
    }

    [TestMethod]
    public void RegexShouldMatch5()
    {
        EventLengthFinder elf = new EventLengthFinder("Play drums (2h 30 min) @home");
        Assert.IsTrue(elf.PatternFound);
    }

    [TestMethod]
    public void CalculateTime5()
    {
        EventLengthFinder elf = new EventLengthFinder("Play drums (2h 30 min) @home");
        Assert.AreEqual(2 * 60 + 30, elf.TotalMinutes);
    }

    [TestMethod]
    public void CutRegex5()
    {
        EventLengthFinder elf = new EventLengthFinder("Play drums (2h 30 min) @home");
        Assert.AreEqual("Play drums @home", elf.TaskSummaryWithoutPattern);
    }

    [TestMethod]
    public void RegexShouldMatch6()
    {
        EventLengthFinder elf = new EventLengthFinder("Czas po polsku (5 minut) @home");
        Assert.IsTrue(elf.PatternFound);
    }

    [TestMethod]
    public void CalculateTime6()
    {
        EventLengthFinder elf = new EventLengthFinder("Czas po polsku (5 minut) @home");
        Assert.AreEqual(5, elf.TotalMinutes);
    }

    [TestMethod]
    public void CutRegex6()
    {
        EventLengthFinder elf = new EventLengthFinder("Czas po polsku (5 minut) @home");
        Assert.AreEqual("Czas po polsku @home", elf.TaskSummaryWithoutPattern);
    }

    [TestMethod]
    public void RegexShouldMatch7()
    {
        EventLengthFinder elf = new EventLengthFinder("Some short thing (5m)");
        Assert.IsTrue(elf.PatternFound);
    }

    [TestMethod]
    public void CalculateTime7()
    {
        EventLengthFinder elf = new EventLengthFinder("Some short thing (5m)");
        Assert.AreEqual(5, elf.TotalMinutes);
    }

    [TestMethod]
    public void CutRegex7()
    {
        EventLengthFinder elf = new EventLengthFinder("Some short thing (5m)");
        Assert.AreEqual("Some short thing", elf.TaskSummaryWithoutPattern);
    }

    [TestMethod]
    public void RegexShouldMatch8()
    {
        EventLengthFinder elf = new EventLengthFinder("Some short thing [5m]");
        Assert.IsTrue(elf.PatternFound);
    }

    [TestMethod]
    public void CalculateTime8()
    {
        EventLengthFinder elf = new EventLengthFinder("Some short thing [5m]");
        Assert.AreEqual(5, elf.TotalMinutes);
    }

    [TestMethod]
    public void CutRegex8()
    {
        EventLengthFinder elf = new EventLengthFinder("Some short thing [5m]");
        Assert.AreEqual("Some short thing", elf.TaskSummaryWithoutPattern);
    }


    [TestMethod]
    public void RegexShouldNotMatch1()
    {
        EventLengthFinder elf = new EventLengthFinder("@market Buy a car");
        Assert.IsFalse(elf.PatternFound);
    }


    [TestMethod]
    public void RegexShouldNotMatch2()
    {
        EventLengthFinder elf = new EventLengthFinder("Buy 3 milk bottles @market");
        Assert.IsFalse(elf.PatternFound);
    }

    [TestMethod]
    public void RegexShouldNotMatch3()
    {
        EventLengthFinder elf = new EventLengthFinder("123 45 6");
        Assert.IsFalse(elf.PatternFound);
    }

    [TestMethod]
    public void RegexShouldNotMatch4()
    {
        EventLengthFinder elf = new EventLengthFinder("Check Philippians 4:11 - 4:12");
        Assert.IsFalse(elf.PatternFound);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void NoTimeSpecified1()
    {
        EventLengthFinder elf = new EventLengthFinder("No time specified in this string");
        int totalMinutes = elf.TotalMinutes;
    }
}
