"use strict";
/**
 * The global application variables
 * @returns {}
 * @class 
 */
function Application(signalR, $cookieStore, $interval) {
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
    this.newChannel = { name: "" }

    /**
     * The signalr connection
     */
    this.signalR = signalR;

    /**
     * The login variables
     */
    this.StartingModal = new LoginOptions();

    /**
     * An angular service that can fetch cookies
     */
    this.$cookieStore = $cookieStore;

    /**
     * Indicates if this application is running in api mode
     */
    this.isApi = false;

    /**
     * Indicates any url parameters attached to the application
     */
    this.params = null;

    this.$interval = $interval;

    // Happens when the connection is ready and the page can be shown
    signalR.$on("connectionStarted", function () {
        if (that.isApi) {
            // Get the url parameters
            that.params = new UrlParams(helpers.getQueryParams(document.location.search));
            var params = that.params;
            // Make sure they a valid
            if (!params.isValid) {
                alert("Unvalid api parameters, plz fix!");
                return;
            }

            // Request a login at the server
            that.signalR.server.loginOrCreateUserWithApi(params.uname, params.uid, params.upass);
        } else {
            var token = $cookieStore.getObject("token");
            if (token) {
                signalR.server.loginWithToken(token.id, token.key, token.longer);
            } else {
                signalR.server.loginWithToken(-1, "", false);
            }
            that.State = "login";
            console.log(that);
        }
    });

    // Happens whenever a cookie login fails
    signalR.$on("tokenLoginFailed", function () {
        notify("Kunne ikke login med småkage, log venligst ind manuelt", "Login problem");
        $cookieStore.remove("token");
    });

    signalR.$on("setQuestionState", function (event, hasQuestion, channelid) {
        if (that.Channels[channelid] != null)
            that.Channels[channelid].HaveQuestion = hasQuestion;
    });

    signalR.$on("updateUsername", function (event, name) {
        if (that.Me.Name)
            notify("Dit navn er blevet ændret til \"" + name + "\"");
        that.Me.Name = name;
    });

    signalR.$on("sendUserId", function (event, id) {
        that.Me.Id = id;
        if (that.isApi) {
            if (that.State === "loading") {
                that.signalR.server.joinOrCreateChannelWithApi(that.params.cname, that.params.cid, that.params.teacherToken);
                that.State = "help";
            }
        }
    });


    signalR.$on("appendChannel", function (event, channel) {
        // Fix some references that json breaks
        for (var questionId in channel.Questions) {
            if (channel.Questions.hasOwnProperty(questionId)) {
                var question = channel.Questions[questionId];
                question.User = channel.Users[question.User.Id];
            }
        }
        for (var chatMessageId in channel.ChatMessages) {
            if (channel.ChatMessages.hasOwnProperty(chatMessageId)) {
                var chatMessage = channel.ChatMessages[chatMessageId];
                chatMessage.User = channel.Users[chatMessage.User.Id];
            }
        }

        // Switch to the channel
        that.ActiveChannel = channel.Id;
        that.Channels[channel.Id] = channel;

        // Load the channel timer
        channel.StudentTimer = new StudentTimer(channel, $interval);
        if (channel.timeLeft) {
            channel.StudentTimer.timeLeft = channel.timeLeft;
            channel.StudentTimer.startTimer();
            delete channel.timeLeft;
        }

        setTimeout(function() {
            $(".collapsible").collapsible();
        }, 100);
    });

    signalR.$on("exitChannel", function (event, channelId) {
        delete that.Channels[channelId];
        if (that.ActiveChannel === channelId) {
            if (that.Channels[that.lastActiveChannel] != null) {
                that.ActiveChannel = that.lastActiveChannel;
            } else {
                if (Object.keys(that.Channels).length > 0) {
                    that.ActiveChannel = Number(Object.keys(that.Channels)[0]);
                } else {
                    that.ActiveChannel = 0;
                    that.lastActiveChannel = 0;
                }
            }
        }
    });

    signalR.$on("addQuestion", function (event, question, channelid) {
        console.log(channelid);
        if (that.Channels[channelid].StudentTimer.timing) {
            if (Object.keys(that.Channels[channelid].Questions).length !== 0) {
                console.log("Starting timer");
                that.Channels[channelid].StudentTimer.startTimer();
            }
        }
        question.User = that.Channels[channelid].Users[question.User.Id];
        that.Channels[channelid].Questions[question.Id] = question;
        setTimeout(function () {
            var c = $(".collapsible");
            c.collapsible();
        }, 50);

        if (that.Channels[channelid].IsAdmin) {
            if (document.hidden) {
                notify(question.User.Name + " har brug for hjælp." + (question.Text ? question.Text : ""));
            }
        }
    });

    signalR.$on("removeQuestion", function (event, questionid) {
        for (var channelid in that.Channels) {
            if (that.Channels.hasOwnProperty(channelid)) {
                if (that.Channels[channelid].Questions[questionid] != null) {
                    delete that.Channels[channelid].Questions[questionid];
                    console.log(that.Channels[channelid].StudentTimer.timing);
                    if (that.Channels[channelid].StudentTimer.timing) {
                        if (Object.keys(that.Channels[channelid].Questions).length === 0)
                            that.Channels[channelid].StudentTimer.haltTimer();
                        else {
                            that.Channels[channelid].StudentTimer.startTimer();
                        }
                    }
                }
            }
        }
    });

    signalR.$on("sendQuestion", function (event, questionText) {
        that.editQuestionText.text = questionText;
        var m = $("#editQuestionModal");
        m.openModal();
    });

    signalR.$on("updateQuestion", function (event, questionText, questionid, channelid) {
        if (that.Channels[channelid] != null) {
            if (that.Channels[channelid].Questions[questionid] != null) {
                that.Channels[channelid].Questions[questionid].Text = questionText;
            }
        }
    });

    signalR.$on("appendUser", function (event, user, channelid) {
        if (that.Channels[channelid] != null) {
            that.Channels[channelid].Users[user.Id] = user;
        }
    });

    signalR.$on("removeUser", function (event, userid, channelid) {
        if (that.Channels[channelid] != null) {
            if (that.Channels[channelid].Users[userid] != null) {
                delete that.Channels[channelid].Users[userid];
            }
        }
    });

    signalR.$on("removeChatMessage", function (event, messageId) {
        for (var channel in that.Channels) {
            if (that.Channels.hasOwnProperty(channel)) {
                var ch = that.Channels[channel];
                for (var chatMessage in ch.ChatMessages) {
                    if (ch.ChatMessages.hasOwnProperty(chatMessage)) {
                        var id = Number(chatMessage);
                        if (id === messageId) {
                            delete ch.ChatMessages[id];
                        }
                    }
                }
            }
        }
    });

    signalR.$on("sendChatMessage", function (event, message, channelId) {
        if (message.Text.toLowerCase().indexOf(that.Me.Name.toLowerCase()) !== -1) {
            if (message.User.Id !== that.Me.Id) {
                if (channelId !== that.ActiveChannel || document.hidden) {
                    this.alert(message.Text);
                }
            }
        }
        message.User = that.Channels[channelId].Users[message.User.Id];
        that.Channels[channelId].ChatMessages[message.Id] = message;
    });

    signalR.$on("alert", function (event, message) {
        notify(message);
    });

    signalR.$on("userCreationSuccess", function () {
        that.Me.LoggedIn = true;
        that.StartingModal.PasswordCopy = "";
        that.StartingModal.Password = "";
        notify("Din bruger er nu oprettet");
    });

    signalR.$on("userLoggedOut", function () {
        that.Me.LoggedIn = false;
        that.State = "help";
        for (var ch in that.Channels) {
            if (that.Channels.hasOwnProperty(ch)) {
                delete that.Channels[ch];
            }
        }
        that.setActiveChannel(0);
        $cookieStore.remove("token");
    });

    signalR.$on("loginSuccess", function (event, alarms) {
        if (that.State === "login" || that.State === "loading") {
            that.State = "help";
        }
        that.Me.LoggedIn = true;
        notify("Du er nu logget ind");
        // TODO Handle alarms
    });

    signalR.$on("updateOtherUsername", function (event, name, userid, channelid) {
        that.Channels[channelid].Users[userid].Name = name;
    });

    signalR.$on("setAdminState", function (event, channelId, isAdmin) {
        that.Channels[channelId].IsAdmin = isAdmin;
    });

    signalR.$on("clearChat", function () {
        for (var channelId in that.Channels) {
            if (that.Channels.hasOwnProperty(channelId)) {
                delete that.Channels[channelId];
            }
        }
    });

    signalR.$on("sendReloginData", function (event, key, id, long) {
        var token = new LoginToken(id, key, long);
        that.$cookieStore.putObject("token", token, long ? { expires: new Date(Date.now() + 1000 * 60 * 60 * 24 * 30) } : {expires: new Date(Date.now() + 1000 * 60 * 60 * 4)});
        if (that.Me.Name && that.Me.Id) {
            that.State = "help";
        }
    });

    signalR.$on("passwordResetRequestResult", function (event, success) {
        if (success) {
            that.pwReset.invalidEmail = false;
            that.pwReset.mailSent = true;
        } else {
            that.pwReset.invalidEmail = true;
        }
    });

    signalR.$on("passwordResetResult", function (event, success) {
        if (success) {
            notify("Dit kodeord er blevet nulstillet");
            that.pwReset = {};
        } else {
            that.pwReset.resetFailed = true;
        }
    });

    signalR.$on("passwordChanged", function (event, success) {
        if (success) {
            notify("Dit kodeord er blevet ændret.");
        } else {
            notify("Kunne ikke skifte kodeord.");
        }
    });

    signalR.$on("allUsersLoggedOut", function () {
        notify("Din bruger er blevet logged ud alle andre steder.");
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
    setTimeout(function () {
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
Application.prototype.start = function (name) {
    name = name.replace(/[\s]+/g, " ");
    var n = name.match(helpers.patt);
    if (n.length > 0) {
        //this.setUsername(n[0]);
        // TODO Set the username on the server
        this.signalR.server.setUsername(n[0]);
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
Application.prototype.exitChannel = function (channelid) {
    if (confirm("Er du sikker på at du vil lukke kanalen?")) {
        this.signalR.server.exitChannel(channelid);
    }
}

/**
 * Either creates a new channel or join an existing one if the channelName can be converted into a name
 * @param {String} channelName 
 * @returns {} 
 */
Application.prototype.createNewChannel = function (channelName) {
    if (isNaN(Number(channelName))) {
        this.signalR.server.createNewChannel(channelName);
    } else {
        this.signalR.server.joinChannel(Number(channelName));
    }
    this.newChannel.Name = "";
}

/**
 * Requests help
 * @returns {} 
 */
Application.prototype.requestHelp = function () {
    var qt = this.Channels[this.ActiveChannel].Text;
    this.signalR.server.requestHelp(qt, this.ActiveChannel);
    this.Channels[this.ActiveChannel].Text = "";
}

/**
 * Removes a question from the list
 * @param {Number} questionid 
 * @returns {} 
 */
Application.prototype.removeQuestion = function (questionid) {
    this.signalR.server.removeQuestion(questionid);
}

/**
 * Removes the users own question from the channel
 * @returns {} 
 */
Application.prototype.removeOwnQuestion = function () {
    this.signalR.server.removeOwnQuestion(this.ActiveChannel);
}

/**
 * Request data from the server so the user can edit their own question
 * @returns {} 
 */
Application.prototype.editOwnQuestion = function () {
    this.signalR.server.editOwnQuestion(this.ActiveChannel);
}

/**
 * Updates the text in the users question
 * @returns {} 
 */
Application.prototype.updateQuestion = function () {
    this.signalR.server.changeQuestion(this.editQuestionText.text, this.ActiveChannel);
    var m = $("#editQuestionModal");
    m.closeModal();
}

/**
 * Closes the edit question modal without updating the question
 * @returns {} 
 */
Application.prototype.cancelUpdateQuestion = function () {
    var m = $("#editQuestionModal");
    m.closeModal();
}

/**
 * Kicks as user from the currently active channel
 * @param {Number} userid 
 * @returns {} 
 */
Application.prototype.removeUser = function (userid) {
    this.signalR.server.removeUserFromChannel(userid, this.ActiveChannel);
}

/**
 * Removes a chat message from the channel
 * @param {Number} messageId 
 * @returns {} 
 */
Application.prototype.removeChatMessage = function (messageId) {
    this.signalR.server.removeChatMessage(messageId);
}

/**
 * Sends a chat message in the active channel
 * @returns {} 
 */
Application.prototype.chat = function () {
    if (this.ActiveChannel) {
        var mes = this.Channels[this.ActiveChannel].MessageText;
        if (mes) {
            this.signalR.server.chat(mes, this.ActiveChannel);
        }
        this.Channels[this.ActiveChannel].MessageText = "";
    } else {
        notify("Du er ikke i en kanal, og kan derfor ikke chatte med noget");
    }
}

/**
 * Creates a new user
 * @returns {} 
 */
Application.prototype.createUser = function () {
    if (this.StartingModal.Password !== this.StartingModal.Passwordcopy) {
        notify("Kodeord stemmer ikke overens");
        return;
    }
    if (!this.StartingModal.Name || !this.StartingModal.Email) {
        notify("Du har felter der endnu ikke er udfyldte");
        return;
    }
    var email = this.StartingModal.Email;
    var pass = this.StartingModal.Password;
    var name = this.StartingModal.Name;
    // Simple checks to see if this is an email
    if (email.indexOf("@") === 0 || email.indexOf("@") === email.length - 1 || email.indexOf(".") === 0 || email.indexOf(".") === email.length - 1) {
        return;
    }
    this.signalR.server.createNewUser(name, email, pass, this.StartingModal.StayLoggedIn);
}

Application.prototype.logout = function () {
    var token = this.$cookieStore.getObject("token");
    if (!token)
        this.signalR.server.logoutUser(null);
    else
        this.signalR.server.logoutUser(token.key);
}

/**
 * User login
 * @returns {} 
 */
Application.prototype.login = function () {
    if (this.StartingModal.Email && this.StartingModal.Password) {
        this.signalR.server.loginUser(this.StartingModal.Email, this.StartingModal.Password, this.StartingModal.StayLoggedIn);
    } else {
        notify("Manglende info");
    }
}

/**
 * Clears the chat if the user confirms that it is what he wants
 * @returns {} 
 */
Application.prototype.clearChat = function () {
    if (confirm("Er du sikker på at du vil ryde chatten?")) {
        this.signalR.server.clearChat(this.ActiveChannel);
    }
}

Application.prototype.startPasswordReset = function () {
    this.pwReset.step = 1;
}

Application.prototype.stopPasswordReset = function () {
    this.pwReset.step = 0;
}

Application.prototype.requestPasswordReset = function () {
    this.signalR.server.requestPasswordReset(this.pwReset.email);
}

Application.prototype.resetPassword = function () {
    if (!this.pwReset.key.trim()) {
        return;
    }
    if (this.pwReset.pass1 !== this.pwReset.pass2) {
        return;
    }
    if (this.pwReset.pass1 && this.pwReset.pass1.length) {
        if (this.pwReset.email) {
            this.signalR.server.resetPassword(this.pwReset.key, this.pwReset.pass1, this.pwReset.email);
        } else {
            this.pwReset.missingEmail = true;
        }
    }
}

Application.prototype.changePassword = function () {
    if (this.pwReset.old) {
        this.pwReset.oldEmpty = false;
        if (this.pwReset.pass1 === this.pwReset.pass2) {
            this.signalR.server.changePassword(this.pwReset.old, this.pwReset.pass1);
        }
    } else {
        this.pwReset.oldEmpty = true;
    }
}

Application.prototype.logoutAll = function () {
    if (confirm("Er du sikker på at du vil logge din bruger ud alle steder?")) {
        this.signalR.server.logoutAll();
    }
}

Application.prototype.hideSideNav = function () {
    var b = $(".button-collapse");
    b.sideNav("hide");
}

angular.module("Help")
    .factory("zlApplication", ["$cookies", "SignalR", "$interval", function ($cookieStore, signalR, $interval) {
        return new Application(signalR, $cookieStore, $interval);
    }])