function notify(text, title, time) {
    var options = {
        body: text
    }

    if (!document.hidden) {
        webpageNotification(text, time);
    }

    // Let's check if the browser supports notifications
    else if (!("Notification" in window)) {
        webpageNotification(text, time);
    }
        
        // Let's check whether notification permissions have already been granted
    else if (Notification.permission === "granted") {
        // If it's okay let's create a notification
        var notification = new Notification(title, options);
    }

        // Otherwise, we need to ask the user for permission
    else if (Notification.permission !== 'denied') {
        Notification.requestPermission(function(permission) {
            // If the user accepts, let's create a notification
            if (permission === "granted") {
                var notification = new Notification(title, options);
            }
        });
    }
    // At last, if the user has denied notifications, and you 
    // want to be respectful there is no need to bother them any more.
    else {
        webpageNotification(text, time);
    }
}

function webpageNotification(text, time) {
    Materialize.toast(text, time ? time : 6000);
}