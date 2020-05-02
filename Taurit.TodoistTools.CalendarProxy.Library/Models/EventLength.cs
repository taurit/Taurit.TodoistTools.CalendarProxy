using System;

namespace Taurit.TodoistTools.CalendarProxy.Library.Models
{
    public class EventLength
    {
        private decimal hours;

        private decimal minutes;

        public decimal Hours
        {
            get => hours;
            set
            {
                if (value > 0)
                    hours = value;
                else
                    throw new ArgumentException("Invalid parameter");
            }
        }

        public decimal Minutes
        {
            get => minutes;

            set
            {
                if (value > 0)
                    minutes = value;
                else
                    throw new ArgumentException("Invalid parameter");
            }
        }
    }
}