"use strict";
/**
 * Defines a channel which containes all the helplists
 * @param {Number} id The id of the Channel on the server
 * @param {String} name The name of the Channel
 * @param {Application} application The application that is controlling everything
 * @returns {Channel} 
 */
function Channel(id, name, application) {
    /**
     * The id of the Channel
     * @type {Number}
     */
    this.Id = id;

    /**
     * The chatmessages in the Channel
     * @type {Object.<number, ChatMessage>}
     */
    this.ChatMessages = {};

    /**
     * The questions in the Channel
     * @type {Object.<number, Question>}
     */
    this.Questions = {};

    /**
     * The name of the Channel
     * @type {String}
     */
    this.ChannelName = name;

    /**
     * The users in the Channel
     * @type {Object.<string, User>}
     */
    this.Users = {};

    /**
     * Indicates if the user has a question in the Channel already
     * @type {Boolean}
     */
    this.HaveQuestion = false;

    /**
     * Indicates if the user is admin in the Channel
     * @type {Boolean}
     */
    this.IsAdmin = false;

    /**
     * Indicates the text of the question the user is making
     * @type {String}
     */
    this.Text = "";

    /**
     * Indicates the text of the message the user is currently typing into the chat
     * @type {String}
     */
    this.MessageText = "";

    /**
     * The timer used to indicate how much time if left for the next student, and which controls the time.
     * @type {StudentTimer}
     */
    this.StudentTimer = new StudentTimer(this, angular.$interval);

    /**
     * The appliation that controls everything
     * @type {Application}
     */
    this.application = application;
}