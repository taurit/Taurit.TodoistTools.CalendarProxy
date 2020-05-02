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
        /// Deserializes and serializes sample iCalendar file, expecting resulting iCalendar string to be the same
        /// </summary>
        [TestMethod]
        public void Serialization()
        {
            //string sampleCalendarContent = new System.Net.WebClient().DownloadString(sampleCalendarFileUrl);
            string sampleCalendarContent = File.ReadAllText(sampleCalendarFileName);

            EventManager eventManager = new EventManager(sampleCalendarContent);
            string generatedIcal = eventManager.GetIcal();

            EventManager eventManager2 = new EventManager(generatedIcal);
            string generatedIcal2 = eventManager.GetIcal();

            Assert.AreEqual(generatedIcal, generatedIcal2);
        }
    }
}
