(function() {
    var domain = window.location.protocol + "//" + window.location.hostname;
    if (document.referrer == null || document.referrer.indexOf(domain) !== 0) {
        var img = document.createElement("img");
        img.src = "https://www.google-analytics.com/__utm.gif";

        img.addEventListener("load", success);
        img.addEventListener("error", failure);

        document.body.appendChild(img);

        function failure() {
            ums("send", "event", "Tracking", "Blocked", "Google Analytics");
            cleanup();
        }

        function success() {
            ums("send", "event", "Tracking", "Allowed", "Google Analytics");
            cleanup();
        }

        function cleanup() {
            document.body.removeChild(img);
        }
    }
})();
