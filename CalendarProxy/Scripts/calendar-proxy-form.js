/**
  * Simple generator for proxy URL's based on user's input in a form.
  */

/// <reference path="jquery-3.4.1.min.js" />
/// <reference path="icalrender.min.js" />

// polyfill for browsers without location.origin, http://stackoverflow.com/a/6167979
if (!location.origin)
    location.origin = location.protocol + "//" + location.host;

// urls used in link generation form
var serviceUrl = location.origin + "/Proxy/Filter";
var demoCalendarUrl = location.origin + "/Content/Demo-2016-01-01.ics";

function replaceAll(find, replace, str) {
    /// <summary>Replace all occurences of a pattern in a string with another string</summary>

    return str.replace(new RegExp(find, 'g'), replace);
}

function updateForm() {
    /// <summary>Updates calendar preview and generated URL based on current values in a form</summary>

    var originalCalendarUrl = $("#url-calendar").val();
    var hideAllDayEvents = $("#cb-hide-all-day-events").is(":checked");
    var shortenOverlapping = $("#cb-shorten-overlapping").is(":checked");
    var predictDuration = $("#cb-predict-duration").is(":checked");
    var removePredictedDuration = $("#cb-remove-duration-from-title").is(":checked");
    var skipText = $("#cb-skip-string").is(":checked") && $("#text-to-skip").val();
    var hideShorter = $("#cb-hide-shorter").is(":checked");
    var hideProjects = $("#cb-hide-projects").is(":checked") && $("#hide-projects-list").val();
    var shorten = $("#cb-shorten").is(":checked") && $("#num-longer-than").val() && $("#num-shorten-to").val();

    // URL recognizes following values (as in FilteringOptions.cs):
    /// h: HideAllDayEvents,
    /// s: ShortenEvents,
    /// p: PredictEventDuration
    /// r: RemoveEventDurationFromTitle
    /// st: SkipEventsContainingThisString
    /// min: HideEventsShorterThanMinutes
    /// pr: HideEventsFromThoseProjects
    /// lt: ShortenEventsLongerThanThisMinutes 
    /// mt: ShortenEventsToThisMinutes 
    var getResultUrlForCalendarUrl = function (calendarUrl) {
        /// <summary>Returns proxified calendar URL for a given calendar, applying user-defined rules (which are read directly from the form)</summary>

        var resultUrl = serviceUrl +
        "?calendarUrl=" + encodeURIComponent(calendarUrl) +
        (hideAllDayEvents ? "&h=1" : "") +
        (shortenOverlapping ? "&s=1" : "") +
        (predictDuration ? "&p=1" : "") +
        (removePredictedDuration ? "&r=1" : "") +
        (skipText ? "&st=" + encodeURIComponent($("#text-to-skip").val()) : "") +
        (hideShorter ? "&min=" + encodeURIComponent($("#num-hide-shorter-than-min").val()) : "") +
        (hideProjects ? "&pr=" + encodeURIComponent($("#hide-projects-list").val()) : "") + 
        (shorten ? "&lt=" + encodeURIComponent($("#num-longer-than").val()) + "&mt=" + encodeURIComponent($("#num-shorten-to").val()) : "");
        return resultUrl;
    };

    var resultUrlUser = getResultUrlForCalendarUrl(originalCalendarUrl);
    var resultUrlDemo = getResultUrlForCalendarUrl(demoCalendarUrl);

    // user-provided values are encoded, but ampersands provided in code still needs to be
    var resultUrlUserHtml = replaceAll("&", "&amp;", resultUrlUser); 
    
    $("#calendar-generator-result-url").html(resultUrlUserHtml);

    // refresh preview
    $.get(resultUrlDemo, function (response) {
        var fileContent = response;

        var rendererAfter = new ICalRender.Renderer(fileContent, 7, 12, 52);
        rendererAfter.RenderMultidayView("#preview-after", 2016, 1, 1);
    });

}


$(document).ready(function () {
    /// <summary>Initialize proxy link generator form</summary>
    "use strict";
    $("#url-calendar").val(demoCalendarUrl);
    $(".calendar-generator .monitored").on('change', updateForm).on('keyup', updateForm);

    // show "after" form for the first time, before user triggers another preview recalculation
    updateForm();

    // show "before" preview
    $.get(demoCalendarUrl, function (response) {
        var renderer = new ICalRender.Renderer(response, 7, 12, 52);
        renderer.RenderMultidayView("#preview-before", 2016, 1, 1);
    });

});

