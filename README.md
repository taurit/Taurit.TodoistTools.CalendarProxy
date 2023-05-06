# Calendar Filter

Calendar Filter is a proxy for calendars in iCalendar format that allows filtering and modification of events. It can be also applied to any calendar in iCal/vCalendar format.

## The goal of the project

Calendar Filter gives you some control over calendars subscribed from a 3rd-party services.

It allows you to **filter out the events that you don't want to see** and **declutter the view of your daily agenda** in the calendar software or service you use.

![Calendar Filter example screenshot](https://raw.githubusercontent.com/taurit/Taurit.TodoistTools.CalendarProxy/master/screenshot.png)

Technically, the proxy is an AWS Lambda that modifies iCalendar file on the fly, according to the rules you specify.

Some examples of calendars you might want to transform before you display it as another overlay on your personal calendar are:

* Google Calendar
* Your company's Outlook calendar
* Facebook events calendar
* Meetup calendar
* Todoist calendar

* ... and all other calendars in the _ics_ format

## Calendar transformations you can use with CalendarProxy

Currently you can:

* Shorten events that overlap the next event on this day
* Hide all-day events
* Predict event duration based on event title and override the duration based on it (useful with *Todoist*)
* Remove recognized event duration from event's title
* Skip tasks containing particular substring
* Hide tasks shorter than (N) minutes
* Hide tasks from particular projects (useful with *Todoist*)
* Shorten events that are longer than (N) minutes to (M) minutes

## Where to find it?

An example instance of this service is deployed at [https://calendar.taurit.pl/](https://calendar.taurit.pl/). There, you can preview how the filter behaves on a sample calendar and generate proxified URLs for your own calendars.
