using EWSoftware.PDI.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Taurit.TodoistTools.CalendarProxy.Library.Helpers;

namespace Taurit.TodoistTools.CalendarProxy.Library.Tests
{
    [TestClass]
    public class FilteringOptionsTests
    {
        [TestMethod]
        public void FilterAllDayEvents1()
        {
            VCalendarParser calendar = new VCalendarParser();
            calendar.ParseFile("SampleCalendar.ics");

            EWSoftware.PDI.Objects.VEvent ev_allday = calendar.VCalendar.Events.AddNew();
            ev_allday.UniqueId.Value = Guid.NewGuid().ToString();
            ev_allday.StartDateTime.DateTimeValue = new DateTime(2015, 01, 01);
            ev_allday.EndDateTime.DateTimeValue = ev_allday.StartDateTime.DateTimeValue.AddDays(1);

            EWSoftware.PDI.Objects.VEvent ev_notallday = calendar.VCalendar.Events.AddNew();
            ev_notallday.UniqueId.Value = Guid.NewGuid().ToString();
            ev_notallday.StartDateTime.DateTimeValue = new DateTime(2015, 01, 01).AddHours(2);
            ev_notallday.EndDateTime.DateTimeValue = ev_notallday.StartDateTime.DateTimeValue.AddDays(1).AddHours(-2);


            Assert.AreEqual(calendar.VCalendar.Events.Where(ev => ev.UniqueId == ev_allday.UniqueId).Count(), 1);
            Assert.AreEqual(calendar.VCalendar.Events.Where(ev => ev.UniqueId == ev_notallday.UniqueId).Count(), 1);

            // filter out all-day events
            EventManager em = new EventManager(calendar);
            em.Filter(new FilteringOptions("h=true"));
            System.Collections.Generic.IList<EWSoftware.PDI.Objects.VEvent> eventsAfterFiltering = em.GetEventList();

            Assert.AreEqual(eventsAfterFiltering.Where(ev => ev.UniqueId == ev_allday.UniqueId).Count(), 0);
            Assert.AreEqual(eventsAfterFiltering.Where(ev => ev.UniqueId == ev_notallday.UniqueId).Count(), 1);
        }

        [TestMethod]
        public void SkipEventsFromProjectX()
        {
            VCalendarParser calendar = new VCalendarParser();
            calendar.ParseFile("SampleCalendar.ics");

            EWSoftware.PDI.Objects.VEvent ev_projectX = calendar.VCalendar.Events.AddNew();
            ev_projectX.UniqueId.Value = Guid.NewGuid().ToString();
            ev_projectX.StartDateTime.DateTimeValue = new DateTime(2015, 01, 01);
            ev_projectX.EndDateTime.DateTimeValue = ev_projectX.StartDateTime.DateTimeValue.AddDays(1);
            ev_projectX.Description.Value =
                @"Proyecto: Project X\n\nCompletar este elemento: \nhttp://todoist.com/#project/999999999";

            EWSoftware.PDI.Objects.VEvent ev_projectY = calendar.VCalendar.Events.AddNew();
            ev_projectY.UniqueId.Value = Guid.NewGuid().ToString();
            ev_projectY.StartDateTime.DateTimeValue = new DateTime(2015, 01, 01).AddHours(2);
            ev_projectY.EndDateTime.DateTimeValue = ev_projectY.StartDateTime.DateTimeValue.AddDays(1).AddHours(-2);
            ev_projectY.Description.Value =
                @"Proyecto: Project Y\n\nCompletar este elemento: \nhttp://todoist.com/#project/999999999";

            EWSoftware.PDI.Objects.VEvent ev_projectZ = calendar.VCalendar.Events.AddNew();
            ev_projectZ.UniqueId.Value = Guid.NewGuid().ToString();
            ev_projectZ.StartDateTime.DateTimeValue = new DateTime(2015, 01, 01).AddHours(2);
            ev_projectZ.EndDateTime.DateTimeValue = ev_projectZ.StartDateTime.DateTimeValue.AddDays(1).AddHours(-2);
            ev_projectZ.Description.Value =
                @"Proyecto: Project\n\nCompletar este elemento: \nhttp://todoist.com/#project/999999999";


            Assert.AreEqual(calendar.VCalendar.Events.Where(ev => ev.UniqueId == ev_projectX.UniqueId).Count(), 1);
            Assert.AreEqual(calendar.VCalendar.Events.Where(ev => ev.UniqueId == ev_projectY.UniqueId).Count(), 1);
            Assert.AreEqual(calendar.VCalendar.Events.Where(ev => ev.UniqueId == ev_projectZ.UniqueId).Count(), 1);

            // filter out all-day events
            EventManager em = new EventManager(calendar);
            em.Filter(new FilteringOptions("&pr=Project X"));
            System.Collections.Generic.IList<EWSoftware.PDI.Objects.VEvent> eventsAfterFiltering = em.GetEventList();

            Assert.AreEqual(eventsAfterFiltering.Where(ev => ev.UniqueId == ev_projectX.UniqueId).Count(), 0);
            Assert.AreEqual(eventsAfterFiltering.Where(ev => ev.UniqueId == ev_projectY.UniqueId).Count(), 1);
            Assert.AreEqual(eventsAfterFiltering.Where(ev => ev.UniqueId == ev_projectZ.UniqueId).Count(), 1);
        }

        [TestMethod]
        public void HideShorterThan10Minutes()
        {
            VCalendarParser calendar = new VCalendarParser();
            calendar.ParseFile("SampleCalendar.ics");

            EWSoftware.PDI.Objects.VEvent ev_5minutes = calendar.VCalendar.Events.AddNew();
            ev_5minutes.UniqueId.Value = Guid.NewGuid().ToString();
            ev_5minutes.StartDateTime.DateTimeValue = new DateTime(2015, 01, 01);
            ev_5minutes.EndDateTime.DateTimeValue = ev_5minutes.StartDateTime.DateTimeValue.AddMinutes(5);

            EWSoftware.PDI.Objects.VEvent ev_10minutes = calendar.VCalendar.Events.AddNew();
            ev_10minutes.UniqueId.Value = Guid.NewGuid().ToString();
            ev_10minutes.StartDateTime.DateTimeValue = new DateTime(2015, 01, 01);
            ev_10minutes.EndDateTime.DateTimeValue = ev_10minutes.StartDateTime.DateTimeValue.AddMinutes(10);

            EWSoftware.PDI.Objects.VEvent ev_15minutes = calendar.VCalendar.Events.AddNew();
            ev_15minutes.UniqueId.Value = Guid.NewGuid().ToString();
            ev_15minutes.StartDateTime.DateTimeValue = new DateTime(2015, 01, 01).AddHours(2);
            ev_15minutes.EndDateTime.DateTimeValue = ev_15minutes.StartDateTime.DateTimeValue.AddMinutes(15);

            Assert.AreEqual(calendar.VCalendar.Events.Where(ev => ev.UniqueId == ev_5minutes.UniqueId).Count(), 1);
            Assert.AreEqual(calendar.VCalendar.Events.Where(ev => ev.UniqueId == ev_10minutes.UniqueId).Count(), 1);
            Assert.AreEqual(calendar.VCalendar.Events.Where(ev => ev.UniqueId == ev_15minutes.UniqueId).Count(), 1);

            // filter out all-day events
            EventManager em = new EventManager(calendar);
            em.Filter(new FilteringOptions("min=10"));
            System.Collections.Generic.IList<EWSoftware.PDI.Objects.VEvent> eventsAfterFiltering = em.GetEventList();

            Assert.AreEqual(eventsAfterFiltering.Where(ev => ev.UniqueId == ev_5minutes.UniqueId).Count(), 0);
            Assert.AreEqual(eventsAfterFiltering.Where(ev => ev.UniqueId == ev_10minutes.UniqueId).Count(), 1);
            Assert.AreEqual(eventsAfterFiltering.Where(ev => ev.UniqueId == ev_15minutes.UniqueId).Count(), 1);
        }

        [TestMethod]
        public void HideIfTitleContainsString()
        {
            VCalendarParser calendar = new VCalendarParser();
            calendar.ParseFile("SampleCalendar.ics");

            EWSoftware.PDI.Objects.VEvent ev_str1 = calendar.VCalendar.Events.AddNew();
            ev_str1.UniqueId.Value = Guid.NewGuid().ToString();
            ev_str1.StartDateTime.DateTimeValue = new DateTime(2015, 01, 01);
            ev_str1.EndDateTime.DateTimeValue = ev_str1.StartDateTime.DateTimeValue.AddDays(1);
            ev_str1.Summary.Value = "this is a [hidden] event";

            EWSoftware.PDI.Objects.VEvent ev_str2 = calendar.VCalendar.Events.AddNew();
            ev_str2.UniqueId.Value = Guid.NewGuid().ToString();
            ev_str2.StartDateTime.DateTimeValue = new DateTime(2015, 01, 01);
            ev_str2.EndDateTime.DateTimeValue = ev_str2.StartDateTime.DateTimeValue.AddDays(1);
            ev_str2.Summary.Value = "[hidden] event too";

            EWSoftware.PDI.Objects.VEvent ev_nostr = calendar.VCalendar.Events.AddNew();
            ev_nostr.UniqueId.Value = Guid.NewGuid().ToString();
            ev_nostr.StartDateTime.DateTimeValue = new DateTime(2015, 01, 01).AddHours(2);
            ev_nostr.EndDateTime.DateTimeValue = ev_nostr.StartDateTime.DateTimeValue.AddDays(1).AddHours(-2);
            ev_nostr.Summary.Value = "this is not hidden";

            Assert.AreEqual(calendar.VCalendar.Events.Where(ev => ev.UniqueId == ev_str1.UniqueId).Count(), 1);
            Assert.AreEqual(calendar.VCalendar.Events.Where(ev => ev.UniqueId == ev_str2.UniqueId).Count(), 1);
            Assert.AreEqual(calendar.VCalendar.Events.Where(ev => ev.UniqueId == ev_nostr.UniqueId).Count(), 1);

            // filter out all-day events
            EventManager em = new EventManager(calendar);
            em.Filter(new FilteringOptions("st=[hidden]"));
            System.Collections.Generic.IList<EWSoftware.PDI.Objects.VEvent> eventsAfterFiltering = em.GetEventList();

            Assert.AreEqual(eventsAfterFiltering.Where(ev => ev.UniqueId == ev_str1.UniqueId).Count(), 0);
            Assert.AreEqual(eventsAfterFiltering.Where(ev => ev.UniqueId == ev_str2.UniqueId).Count(), 0);
            Assert.AreEqual(eventsAfterFiltering.Where(ev => ev.UniqueId == ev_nostr.UniqueId).Count(), 1);
        }

        [TestMethod]
        public void ShortenEvents()
        {
            VCalendarParser calendar = new VCalendarParser();
            calendar.ParseFile("SampleCalendar.ics");

            EWSoftware.PDI.Objects.VEvent ev_1h = calendar.VCalendar.Events.AddNew();
            ev_1h.UniqueId.Value = Guid.NewGuid().ToString();
            ev_1h.StartDateTime.DateTimeValue = new DateTime(2015, 01, 01);
            ev_1h.EndDateTime.DateTimeValue = ev_1h.StartDateTime.DateTimeValue.AddHours(1);

            EWSoftware.PDI.Objects.VEvent ev_2h = calendar.VCalendar.Events.AddNew();
            ev_2h.UniqueId.Value = Guid.NewGuid().ToString();
            ev_2h.StartDateTime.DateTimeValue = new DateTime(2015, 01, 01);
            ev_2h.EndDateTime.DateTimeValue = ev_2h.StartDateTime.DateTimeValue.AddHours(2);

            EWSoftware.PDI.Objects.VEvent ev_3h = calendar.VCalendar.Events.AddNew();
            ev_3h.UniqueId.Value = Guid.NewGuid().ToString();
            ev_3h.StartDateTime.DateTimeValue = new DateTime(2015, 01, 01).AddHours(2);
            ev_3h.EndDateTime.DateTimeValue = ev_3h.StartDateTime.DateTimeValue.AddHours(3);

            Assert.AreEqual(
                calendar.VCalendar.Events.Where(ev => ev.UniqueId == ev_1h.UniqueId).First().DurationBasedOnDates()
                    .TotalHours, 1);
            Assert.AreEqual(
                calendar.VCalendar.Events.Where(ev => ev.UniqueId == ev_2h.UniqueId).First().DurationBasedOnDates()
                    .TotalHours, 2);
            Assert.AreEqual(
                calendar.VCalendar.Events.Where(ev => ev.UniqueId == ev_3h.UniqueId).First().DurationBasedOnDates()
                    .TotalHours, 3);

            // filter out all-day events
            EventManager em = new EventManager(calendar);
            em.Filter(new FilteringOptions("lt=60&mt=60"));
            System.Collections.Generic.IList<EWSoftware.PDI.Objects.VEvent> eventsAfterFiltering = em.GetEventList();

            Assert.AreEqual(
                calendar.VCalendar.Events.Where(ev => ev.UniqueId == ev_1h.UniqueId).First().DurationBasedOnDates()
                    .TotalHours, 1);
            Assert.AreEqual(
                calendar.VCalendar.Events.Where(ev => ev.UniqueId == ev_2h.UniqueId).First().DurationBasedOnDates()
                    .TotalHours, 1);
            Assert.AreEqual(
                calendar.VCalendar.Events.Where(ev => ev.UniqueId == ev_3h.UniqueId).First().DurationBasedOnDates()
                    .TotalHours, 1);
        }
    }
}