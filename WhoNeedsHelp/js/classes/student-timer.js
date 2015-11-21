"use strict";
/**
 * The timer used to countdown on each question to make sure each student gets about the same amount of time. 
 * @returns {}
 * @class 
 */
function StudentTimer(channel, $interval) {

    /**
     * The channel this timer is attached to.
     * @type {Channel}
     */
    this.channel = channel;
    /**
     * Indicates if we are currently counting down in the Channel
     * @type {Boolean}
     */
    this.counting = false;

    /**
     * Indicates if the current top question is out of time. 
     * @type {Boolean}
     */
    this.outOfTime = false;

    /**
     * Indicates if the Channel should countdown whenever there are questions
     * @type {Boolean}
     */
    this.timing = false;

    /**
     * Indicates the time left until the alarm will sound
     * @type {Number}
     */
    this.timeLeft = 300;

    /**
     * Contains the interval counter
     * @type {Object}
     */
    this.intervalCont = null;

    /**
     * Indicates if the timer should be shown
     * @type {Boolean}
     */
    this.showingTimer = false;

    /**
     * The alarm that will ring whenever the student timer runs out
     * @type {Audio}
     */
    this.alarm = new Audio("/alarm.mp3");

    /**
     * The angular interval service
     */
    this.$interval = $interval;
}

/**
 * The default time a countdown should run
 */
StudentTimer.defaultCountdownTime = 300;

/**
 * Toggles the visible clock
 * @returns {} 
 */
StudentTimer.prototype.toggleShowClock = function () {
    if (this.showingTimer) {
        jQuery("#timerClock").stop().removeClass("active", 1000);
    } else {
        jQuery("#timerClock").stop().addClass("active", 1000);
    }
    this.showingTimer = !this.showingTimer;
}

/**
 * Temporary pauses the timer
 * @returns {} 
 */
StudentTimer.prototype.haltTimer = function () {
    this.$interval.cancel(this.intervalCont);
    this.counting = false;
}

/**
 * Stops the timer completely
 * @returns {} 
 */
StudentTimer.prototype.stopTimer = function () {
    this.haltTimer();
    this.timing = false;
    this.outOfTime = false;
    // TODO Send countdown time to server
}

/**
 * Prompts the user for the new time the timer should be set to
 * @returns {} 
 */
StudentTimer.prototype.editTimer = function () {
    var n = prompt("Hvad skal den nye tid være? \n Tal i sekunder");
    var m = Number(n);
    if (m === NaN) {
        notify("Ikke et tal!");
    } else if (m <= 0) {
        notify("Tiden kan ikke være mindre end 1 sekund!");
    } else {
        StudentTimer.defaultCountdownTime = m;
    }
}

/**
 * Decreses the current value of the timer by 1.
 * Should be called about every second if the timer is active. 
 * @returns {} 
 */
StudentTimer.prototype.countDown = function (that) {
    console.log("here");
    console.log(this);
    that.timeLeft = that.timeLeft - 1;
    if (that.timeLeft % 10 === 0) {
        // TODO Send countdown time to server, somehow
    }
    if (that.timeLeft <= 0) {
        that.outOfTime = true;
        if (that.alarm) that.alarm.play();
        that.haltTimer();
    }
}

/**
 * Starts the timer
 * @returns {} 
 */
StudentTimer.prototype.startTimer = function () {
    this.timing = true;
    this.counting = true;
    this.timeLeft = StudentTimer.defaultCountdownTime;
    this.outOfTime = false;

    if (this.intervalCont) {
        this.$interval.cancel(this.intervalCont);
    }

    this.intervalCont = this.$interval(this.countDown, 1000, 0, true, this);
}