// The alarms that can sound
function Alarm() {

    /**
     * The id of the alarm in the database
     */
    this.id = null;
    /**
     * The days at which the alarm should sound
     */
    this.days = [];
    /**
     * The time at which the alarm should sound
     */
    this.time = null;

    this.untilCanBePlayedAgain = 0;

    /**
     * Checks if the alarm should sound
     * @returns {boolean} 
     */
    this.isTime = function() {
        // If a time hasn't been set, then we can't really check it
        if (!this.time) return false;

        // Make sure the alarm doesn't sound several times within the same minute
        if (this.untilCanBePlayedAgain !== 0) {
            this.untilCanBePlayedAgain--;
            return false;
        } 

        // Get the current day as a number
        var now = new Date();
        var day = now.getDay();
        var isToday = false;

        // Check if today is one of the day the alarm should sound
        for (var i = 0; i < days.length; i++) {
            if (days[i] === day) {
                isToday = true;
            }
        }
        if (!isToday) return false;

        // Check if the time is now
        if (now.getHours() === time.getHours() && now.getMinutes() === time.getMinutes()) {
            return true;
        }
        return false;
    }

    /**
     * Set the time this alarm should ring
     * @param {number} hour The hour at which the alarm should ring
     * @param {number} minute The minute at which the alarm should ring
     * @returns {} 
     */
    this.setTime = function (hour, minute) {
        // Make sure no values are invalid
        if (hour === undefined || hour === null) {
            throw new Error("You have to supply an hour");
        }
        if (minute === undefined || minute === null) {
            throw new Error("You have to supply a minute");
        }

        // Set the time
        var set = new Date();
        set.setHours(hour);
        set.setMinutes(minute);
    }

    /**
     * Add a day to the list of days to sound on
     * @param {Number} day The day of the week to sound the alarm 
     * @returns {} 
     */
    this.addDay = function (day) {
        // Make sure the day is formatted as a number
        if (typeof (day) === "number") {
            // Make sure the day isn't already in the list
            for (var i = 0; i < this.days.length; i++) {
                if (days[i] === day) {
                    return;
                }
            }

            // Add the day to the alarming days
            days.push(day);
        } else {
            throw new Error("Invalid day type");
        }
    }

    /**
     * Removes a specific day from the alarm
     * @param {Number} day The day to remove
     * @returns {} 
     */
    this.removeDay = function(day) {
        for (var i = 0; i < this.days.length; i++) {
            if (this.days[i] === day) {
                this.days = removeFromArray(this.days, i);
                return;
            }
        }
    }
}

// The object that will handle all the alarms
function CleanupTimer() {
    /**
     * The alarms currently known
     */
    this.alarms = [];

    /**
     * Used to reference this inside other scopes
     */
    var dis = this;

    /**
     * The alarm to play once time is up
     */
    this.audio = new Audio("/alarm.mp3");

    /**
     * Creates a new alarm
     * @param {number} hour The hour at which the alarm should sound 
     * @param {number} minute The minute at which the alarm should sound
     * @returns {} 
     */
    this.createNewAlarm = function(hour, minute, days) {
        var alarm = new Alarm();
        alarm.setTime(hour, minute);
        alarm.days = days;
        alarms.push(alarm);
    }

    /**
     * Checks if it's time for any of the alarms to sound
     * @returns {} 
     */
    this.isAlarmTime = function() {
        for (var i = 0; i < alarms.length; i++) {
            if (alarms[i].isTime()) return alarms[i];
        }
        return null;
    }

    /**
     * Run every 10 second, checks if any of the alarms should ring
     */
    this.interval = setInterval(function() {
        var alarm;
        if ((alarm = dis.isAlarmTime()) != null) {
            dis.audio.play();
            $.connection.centralHub.server.cleaupTime(alarm.id);
        }
    }, 10000);

    $.connection.centralHub.client.cleanupTime = function()
    {
        dis.audio.play();

    }
}

var app = angular.module("Help");
app.factory("cleanupTimer", [
    function() {
        return new CleanupTimer();
    }
]);