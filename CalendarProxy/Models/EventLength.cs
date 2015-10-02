using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CalendarProxy.Helpers
{
    public class EventLength
    {
        private decimal hours;
        public decimal Hours
        {
            get
            {
                return this.hours;
            }
            set
            {
                if (value > 0)
                {
                    this.hours = value;
                }
                else
                {
                    throw new ArgumentException("Invalid parameter");
                }
            }
        }

        private decimal minutes;
        public decimal Minutes
        {
            get
            {
                return this.minutes;
            }

            set
            {
                if (value > 0)
                {
                    this.minutes = value;
                }
                else
                {
                    throw new ArgumentException("Invalid parameter");
                }
            }
        }


    }
}