# CalendarProxy
CalendarProxy is a proxy for calendars in iCalendar format that allows filtering and modification of events. Designed especially to work with Todoist service as an independent extension of its calendar capabilities, but can be also aplied to any calendar in iCal/vCalendar format.

## Description

CalendarProxy is a web service written in ASP.NET MVC, that allows you to modify iCalendar file (e.g. the one provided by Todoist, Facebook, Google Calendar, Meetup etc.) on the fly, according to the rules you specify.

It's intention is to give you some control over calendars subscribed from a 3rd-party service, especially filter out the events that you don't want to see and declutter the view of your daily agenda in your organizer program.

## Available filters

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

Example instance of this service is deployed under [https://todoistcalendar.azurewebsites.net/](), where you can preview how the filter behaves on an examplary calendar and generate proxified URLs for your own calendars.



![CalendarProxy example screenshot](https://github.com/taurit/CalendarProxy/blob/master/CalendarProxy/Content/CalendarProxy-example-screenshot.png)

