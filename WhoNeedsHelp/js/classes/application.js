"use strict";
/**
 * The global application variables
 * @returns {}
 * @class 
 */
function Application(signalR, $cookieStore) {
    var that = this;
    /**
     * Descripes the state the application is currently in
     * @type {String}
     */
    this.State = "loading";

    /**
     * Contains a dictionary of all the channels currently loaded in the application
     * @type {Object.<Number, Channel>}
     */
    this.Channels = {};

    /**
     * The id of the currently displayed an active channel
     * @type {Number}
     */
    this.ActiveChannel = 0;

    /**
     * The user himself, who is using the machine
     * @type {Me}
     */
    this.Me = new Me();

    /**
     * The text currently being edited
     */
    this.editQuestionText = { text: "" }

    /**
     * The current step in the password reset progress
     */
    this.pwReset = { step: 0 }

    /**
     * The information used to create a new channel
     */
    this.newChannel = {name: ""}

    /**
     * The signalr connection
     */
    this.signalR = signalR;

    // Happens when the connection is ready and the page can be shown
    signalR.$on("connectionStarted", function () {
        /**
         * @type {LoginToken}
         */
        var token = $cookieStore.get("token");
        if (token) {
            signalR.server.loginWithToken(token.id, token.key);
        }
        that.State = "login";
    });

    // Happens whenever a cookie login fails
    signalR.$on("tokenLoginFailed", function() {
        notify("Kunne ikke login med småkage, log venligst ind manuelt", "Login problem");
    });
}

/**
 * Changes the active channel
 * @param {Number} channelid The channel id to change to
 * @returns {} 
 */
Application.prototype.setActiveChannel = function (channelid) {
    // Set the active channel
    this.ActiveChannel = channelid;
    
    // Wait a bit and then make sure the Materialize Collapsibles are activated
    setTimeout(function() {
        var c = $(".collapsible");
        c.collapsible();
    }, 100);
}

/**
 * Starts the application
 * TODO Overwrite this method in the api
 * @param {String} name The name of the user
 * @returns {} 
 */
Application.prototype.start = function(name) {
    name = name.replace(/[\s]+/g, " ");
    var n = name.match(helpers.patt);
    if (n.length > 0) {
        //this.setUsername(n[0]);
        // TODO Set the username on the server

        // The name can be changed in usermanage too, so if it was changed there, then we shouldn't change the application state
        if (this.State !== "usermanage") {
            this.State = "help";
        }
    }
}

/**
 * Leaves the specified channel
 * @param {Number} channelid The id of the channel to leave
 * @returns {} 
 */
Application.prototype.exitChannel = function(channelid) {
    if (confirm("Er du sikker på at du vil lukke kanalen?")) {
        this.signalR.server.exitChannel(channelid);
    }
}

/**
 * Either creates a new channel or join an existing one if the channelName can be converted into a name
 * @param {String} channelName 
 * @returns {} 
 */
Application.prototype.createNewChannel = function(channelName) {
    if (isNaN(Number(channelName))) {
        this.createNewChannel(channelName);
    } else {
        this.joinChannel(Number(channelName));
    }
    this.newChannel.Name = "";
}

/**
 * Requests help
 * @returns {} 
 */
Application.prototype.requestHelp = function() {
    var qt = this.Channels[this.ActiveChannel].Text;
    this.signalR.server.requestHelp(qt);
    this.Channels[this.ActiveChannel].Text = "";
}

Application.prototype.removeQuestion = function(questionid) {
    this.signalR.server.removeQuestion(questionid);
}