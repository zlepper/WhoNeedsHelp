/// <reference path="../Scripts/typings/jquery/jquery.d.ts" />
/// <reference path="../Scripts/typings/signalr/signalr.d.ts" />
/// <reference path="../Scripts/typings/jqueryui/jqueryui.d.ts"/>
/// <reference path="../Scripts/typings/jquery.pnotify/jquery.pnotify.d.ts"/>
/// <reference path="../scripts/typings/angularjs/angular.d.ts" />
/// <reference path="../scripts/typings/angularjs/angular-animate.d.ts" />
/// <reference path="../scripts/typings/angular-ui-bootstrap/angular-ui-bootstrap.d.ts" />
/// <reference path="../scripts/typings/angularjs/angular-cookies.d.ts" />
var Help;
(function (Help) {
    var ServerActions = (function () {
        function ServerActions() {
        }
        ServerActions.prototype.send = function (action, parameters) {
            return this.helper.server.send(action, parameters);
        };
        ServerActions.prototype.getData = function (action) {
            return this.helper.server.getData(action);
        };
        ServerActions.prototype.setUsername = function (name) {
            return this.helper.server.setUsername(name);
        };
        ServerActions.prototype.createNewChannel = function (channelName) {
            return this.helper.server.createNewChannel(channelName);
        };
        ServerActions.prototype.loadHearbyChannels = function () {
            return this.helper.server.loadNearbyChannels();
        };
        ServerActions.prototype.exitChannel = function (channelId) {
            return this.helper.server.exitChannel(channelId);
        };
        ServerActions.prototype.joinChannel = function (channelId) {
            return this.helper.server.joinChannel(channelId);
        };
        ServerActions.prototype.removeQuestion = function (channelId) {
            return this.helper.server.removeQuestion(channelId);
        };
        ServerActions.prototype.removeChatMessage = function (messageId) {
            return this.helper.server.removeChatMessage(messageId);
        };
        ServerActions.prototype.chat = function (message, channelid) {
            return this.helper.server.chat(message, channelid);
        };
        ServerActions.prototype.clearChat = function (channelId) {
            return this.helper.server.clearChat(channelId);
        };
        ServerActions.prototype.createNewUser = function (username, email, password, stay) {
            return this.helper.server.createNewUser(username, email, password, stay);
        };
        ServerActions.prototype.requestActiveChannel = function () {
            return this.helper.server.requestActiveChannel();
        };
        ServerActions.prototype.requestHelp = function (question, channelid) {
            return this.helper.server.requestHelp(question, channelid);
        };
        ServerActions.prototype.loginUser = function (mail, pass, stay) {
            return this.helper.server.loginUser(mail, pass, stay);
        };
        ServerActions.prototype.logoutUser = function (key) {
            return this.helper.server.logoutUser(key);
        };
        ServerActions.prototype.removeUserFromChannel = function (id, channelid) {
            return this.helper.server.removeUserFromChannel(id, channelid);
        };
        ServerActions.prototype.removeOwnQuestion = function (channelid) {
            return this.helper.server.removeOwnQuestion(channelid);
        };
        ServerActions.prototype.editOwnQuestion = function (channelId) {
            return this.helper.server.editOwnQuestion(channelId);
        };
        ServerActions.prototype.changeQuestion = function (questionText, channelId) {
            return this.helper.server.changeQuestion(questionText, channelId);
        };
        ServerActions.prototype.loginWithToken = function (id, key) {
            return this.helper.server.loginWithToken(id, key);
        };
        ServerActions.prototype.sendCountdownTime = function (time, channelid) {
            return this.helper.server.sendCountdownTime(time, channelid);
        };
        ServerActions.prototype.requestPasswordReset = function (email) {
            return this.helper.server.requestPasswordReset(email);
        };
        ServerActions.prototype.resetPassword = function (key, pass, email) {
            return this.helper.server.resetPassword(key, pass, email);
        };
        ServerActions.prototype.changePassword = function (oldpass, newpass) {
            return this.helper.server.changePassword(oldpass, newpass);
        };
        ServerActions.prototype.logoutAll = function () {
            return this.helper.server.logoutAll();
        };
        ServerActions.prototype.syncChannels = function (chs) {
            return this.helper.server.syncChannels(chs);
        };
        ServerActions.prototype.loginOrCreateUserWithApi = function (username, userid, password) {
            return this.helper.server.loginOrCreateUserWithApi(username, userid, password);
        };
        ServerActions.prototype.joinOrCreateChannelWithApi = function (channelname, channelid, teacherKey) {
            return this.helper.server.joinOrCreateChannelWithApi(channelname, channelid, teacherKey);
        };
        ServerActions.prototype.alert = function (text) {
            notify(text, "");
        };
        ServerActions.prototype.confirm = function (text, title, callback) {
            if (confirmNotice == null)
                confirmNotice = new PNotify({
                    title: title,
                    text: text,
                    icon: "glyphicon glyphicon-question-sign",
                    mouse_reset: false,
                    hide: false,
                    confirm: {
                        confirm: true,
                        buttons: [
                            {
                                text: "Ok",
                                click: function (n) {
                                    n.remove();
                                    callback();
                                    confirmNotice = null;
                                }
                            },
                            {
                                text: "Annuller",
                                click: function (n) {
                                    n.remove();
                                    confirmNotice = null;
                                }
                            }
                        ]
                    },
                    buttons: {
                        closer: false,
                        sticker: false
                    },
                    history: {
                        history: false
                    }
                });
        };
        return ServerActions;
    })();
    Help.ServerActions = ServerActions;
})(Help || (Help = {}));
