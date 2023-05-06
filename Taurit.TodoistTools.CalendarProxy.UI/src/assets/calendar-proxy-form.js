/// <reference path="jquery-3.4.1.min.js" />
/// <reference path="icalrender.min.js" />

/**
  * Simple generator for proxy URL's based on user's input in a form.
  */

// polyfill for browsers without location.origin, http://stackoverflow.com/a/6167979
if (!location.origin)
    location.origin = location.protocol + '//' + location.host;

// urls used in link generation form
var serviceUrl = location.origin + '/Proxy/Filter';
var demoCalendarUrl = 'https://calendar.taurit.pl/assets/Demo-2016-01-01.ics';

function replaceAll(find, replace, str) {
    /// <summary>Replace all occurrences of a pattern in a string with another string</summary>

    return str.replace(new RegExp(find, 'g'), replace);
}

function updateForm() {
    /// <summary>Updates calendar preview and generated URL based on current values in a form</summary>

    var originalCalendarUrl = window.$('#url-calendar').val();
    var hideAllDayEvents = window.$('#cb-hide-all-day-events').is(':checked');
    var hidePrivateEvents = window.$('#cb-hide-private-events').is(':checked');
    var shortenOverlapping = window.$('#cb-shorten-overlapping').is(':checked');
    var predictDuration = window.$('#cb-predict-duration').is(':checked');
    var removePredictedDuration = window.$('#cb-remove-duration-from-title').is(':checked');
    var skipText = window.$('#cb-skip-string').is(':checked') && window.$('#text-to-skip').val();
    var hideShorter = window.$('#cb-hide-shorter').is(':checked');
    var hideProjects = window.$('#cb-hide-projects').is(':checked') && window.$('#hide-projects-list').val();
    var shorten = window.$('#cb-shorten').is(':checked') && window.$('#num-longer-than').val() && window.$('#num-shorten-to').val();

    // URL recognizes following values (as in FilteringOptions.cs):
    // h: HideAllDayEvents,
    // h: HidePrivateEvents,
    // s: ShortenEvents,
    // p: PredictEventDuration
    // r: RemoveEventDurationFromTitle
    // st: SkipEventsContainingThisString
    // min: HideEventsShorterThanMinutes
    // pr: HideEventsFromThoseProjects
    // lt: ShortenEventsLongerThanThisMinutes 
    // mt: ShortenEventsToThisMinutes 
    var getResultUrlForCalendarUrl = function (calendarUrl) {
        /// <summary>Returns proxified calendar URL for a given calendar, applying user-defined rules (which are read directly from the form)</summary>

        var resultUrl = "https://filter.calendar.taurit.pl/filter" + // waiting for DNS propagation I guess, doesnt work yet
        //var resultUrl = "https://1yp7sgzz91.execute-api.eu-north-1.amazonaws.com/filter" +
        '?calendarUrl=' + encodeURIComponent(calendarUrl) +
        (hideAllDayEvents ? '&h=1' : '') +
        (hidePrivateEvents ? '&hp=1' : '') +
        (shortenOverlapping ? '&s=1' : '') +
        (predictDuration ? '&p=1' : '') +
        (removePredictedDuration ? '&r=1' : '') +
        (skipText ? '&st=' + encodeURIComponent(window.$('#text-to-skip').val()) : '') +
        (hideShorter ? '&min=' + encodeURIComponent(window.$('#num-hide-shorter-than-min').val()) : '') +
        (hideProjects ? '&pr=' + encodeURIComponent(window.$('#hide-projects-list').val()) : '') + 
        (shorten ? '&lt=' + encodeURIComponent(window.$('#num-longer-than').val()) + '&mt=' + encodeURIComponent(window.$('#num-shorten-to').val()) : '');
        return resultUrl;
    };

    var resultUrlUser = getResultUrlForCalendarUrl(originalCalendarUrl);
    var resultUrlDemo = getResultUrlForCalendarUrl(demoCalendarUrl);

    // user-provided values are encoded, but ampersands provided in code still needs to be
    var resultUrlUserHtml = replaceAll('&', '&amp;', resultUrlUser); 
    
    window.$('#calendar-generator-result-url').html(resultUrlUserHtml);

    // refresh preview
    window.$.get(resultUrlDemo, function (response) {
        var fileContent = response;

        var rendererAfter = new window.ICalRender.Renderer(fileContent, 7, 12, 52);
        rendererAfter.RenderMultidayView('#preview-after', 2016, 1, 1);
    });

}


$(document).ready(function () {
    /// <summary>Initialize proxy link generator form</summary>
    'use strict';
    $('#url-calendar').val(demoCalendarUrl);
    $('.calendar-generator .monitored').on('change', updateForm).on('keyup', updateForm);

    // show "after" form for the first time, before user triggers another preview recalculation
    updateForm();

    // show "before" preview
    $.get(demoCalendarUrl, function (response) {
        var renderer = new ICalRender.Renderer(response, 7, 12, 52);
        renderer.RenderMultidayView('#preview-before', 2016, 1, 1);
    });

});

