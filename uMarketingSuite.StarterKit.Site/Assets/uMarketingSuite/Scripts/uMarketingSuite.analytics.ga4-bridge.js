(function uMarketingSuite_GA4_bridge() {
    if (window.navigator == null ||
        window.navigator.constructor.prototype.toString() !== "[object Navigator]" ||
        typeof window.navigator.sendBeacon !== "function") {
        // Navigator.sendBeacon is not implemented in this browser.
        return;
    }

    // If "Enhanced Measurement" is enabled Google sends some events by default which generates a lot of events.
    // This GA4 bridge is meant to send custom client events to UMS and we do not want all these Enhanced Measurement events to be sent.
    // https://support.google.com/analytics/answer/9234069?hl=en
    var ENHANCED_MEASUREMENT_EVENTS = [
        "click",
        "file_download",
        "form_start",
        "form_submit",
        "page_view",
        "scroll",
        "video_complete",
        "video_progress",
        "video_start",
        "view_search_results"
    ];

    var sendBeaconFn = window.navigator.sendBeacon;
    window.navigator.sendBeacon = function sendBeacon_uMarketingSuite_GA4_bridge() {
        // Call original Navigator.sendBeacon
        sendBeaconFn.apply(this, arguments);

        if (typeof window.ums !== "function" || arguments == null || arguments.length === 0) {
            // We require:
            // * window.ums to be defined, which is initialized by uMarketingSuite.analytics.js.
            // * at least 1 argument to sendBeacon since that is the URL we inspect.
            return;
        }

        var url = arguments[0];
        if (typeof url !== "string" || url.length === 0 || url.indexOf("google-analytics") === -1) {
            // Only operate on URLs containing "google-analytics"
            return;
        }

        var eventNameRe = /\ben=([^&]+)/;
        var match = url.match(eventNameRe);
        if (match == null || match.length !== 2) {
            // We require a length of exactly 2 containing:
            // * Full match
            // * Capture group 1 (event name)
            return;
        }

        var eventName = match[1];
        if (eventName == null || eventName.length === 0 || ENHANCED_MEASUREMENT_EVENTS.indexOf(eventName) > -1) {
            // Do not send empty event names or Enhanced Measurement events to UMS
            return;
        }

        var category = "GA 4 - Bridging";
        var action = eventName;
        window.ums("send", "event", category, action);
    }
})();
