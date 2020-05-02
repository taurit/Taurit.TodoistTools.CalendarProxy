using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Taurit.TodoistTools.CalendarProxy.Library.Helpers;

namespace Taurit.TodoistTools.CalendarProxy.Library.Tests
{
    [TestClass]
    public class iCalendarTests
    {
        private const string sampleCalendarFileName = "SampleCalendar.ics";

        /// <summary>
        ///     Deserializes and serializes sample iCalendar file, expecting resulting iCalendar string to be the same
        /// </summary>
        [TestMethod]
        public void Serialization()
        {
            //string sampleCalendarContent = new System.Net.WebClient().DownloadString(sampleCalendarFileUrl);
            var sampleCalendarContent = File.ReadAllText(sampleCalendarFileName);

            var eventManager = new EventManager(sampleCalendarContent);
            var generatedIcal = eventManager.GetIcal();

            var eventManager2 = new EventManager(generatedIcal);
            var generatedIcal2 = eventManager.GetIcal();

            Assert.AreEqual(generatedIcal, generatedIcal2);
        }
    }
}