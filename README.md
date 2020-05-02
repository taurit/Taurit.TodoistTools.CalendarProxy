# CalendarProxy

CalendarProxy is a proxy for calendars in iCalendar format that allows filtering and modification of events. Designed especially to work with Todoist service as an independent extension of its calendar capabilities, but can be also applied to any calendar in iCal/vCalendar format.

## The goal of the project

CalendarProxy gives you some control over calendars subscribed from a 3rd-party services.

It allows you to **filter out the events that you don't want to see** and **declutter the view of your daily agenda** in your organizer program.

Technically, it is a web service that allows you to modify iCalendar file on the fly, according to the rules you specify.

Some examples of calendars you might want to transform before you display it as another overlay on your personal calendar are:

* Todoist calendar
* Facebook events calendar
* Google Calendar
* Meetup calendar
* your company's Outlook calendar
* ... and all other calendars in the _ics_ format

## Calendar transformations you can use with CalendarProxy

Currently you can:

* Shorten events that overlap the next event on this day
* Hide all-day events
* Predict event duration based on event title
* Remove recognized event duration from event's title
* Skip tasks  containing particular tag
* Hide tasks shorter than (N) minutes
* Hide tasks from particular projects (applies to *Todoist*)
* Shorten events that are longer than (N) minutes to (M) minutes

## Example

Example instance of this service is deployed under [https://todoistcalendar.azurewebsites.net/](https://todoistcalendar.azurewebsites.net/), where you can preview how the filter behaves on an examplary calendar and generate proxified URLs for your own calendars.

![CalendarProxy example screenshot](https://github.com/taurit/CalendarProxy/blob/master/CalendarProxy/Content/CalendarProxy-example-screenshot.png)
