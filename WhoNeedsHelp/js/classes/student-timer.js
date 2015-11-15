﻿"use strict";
/**
 * The timer used to countdown on each question to make sure each student gets about the same amount of time. 
 * @returns {}
 * @class 
 */
function StudentTimer(channel) {

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
    this.$interval;
}

/**
 * The default time a countdown should run
 */
StudentTimer.defaultCountdownTime = 300;

/**
 * Toggles the visible clock
 * @returns {} 
 */
StudentTimer.prototype.ToggleShowClock = function() {
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
StudentTimer.prototype.haltTimer = function() {
    this.$interval.cancel(this.intervalCont);
    this.counting = false;
}

/**
 * Stops the timer completely
 * @returns {} 
 */
StudentTimer.prototype.stopTimer = function() {
    this.haltTimer();
    this.timing = false;
    this.outOfTime = false;
    // TODO Send countdown time to server
}

/**
 * Prompts the user for the new time the timer should be set to
 * @returns {} 
 */
StudentTimer.prototype.editTimer = function() {
    var n = prompt("Hvad skal den nye tid være? \n Tal i sekunder");
    var m = Number(n);
    if (m === NaN) {
        notify("Ikke et tal!");
    } else if (m <= 0) {
        notify("Tiden kan ikke være mindre end 1 sekund!");
    } else {
        this.defaultCountdownTime = m;
    }
}

/**
 * Decreses the current value of the timer by 1.
 * Should be called about every second if the timer is active. 
 * @returns {} 
 */
StudentTimer.prototype.countDown = function() {
    this.timeLeft--;
    if (this.timeLeft % 10 === 0) {
        // TODO Send countdown time to server, somehow
    }
    if (this.timeLeft <= 0) {
        this.outOfTime = true;
        if (this.alarm) this.alarm.play();
        this.haltTimer();
    }
}

/**
 * Starts the timer
 * @param {} The angular interval service. Has to be injected from the caller. 
 * @returns {} 
 */
StudentTimer.prototype.startTimer = function($interval) {
    this.timing = true;
    this.counting = true;
    this.timeLeft = this.defaultCountdownTime;
    this.outOfTime = false;

    if (this.intervalCont) {
        $interval.cancel(this.intervalCont);
    }

    this.intervalCont = $interval(this.countDown, this.defaultCountdownTime);
}