"use strict";
function SignalRFactory($rootScope, $timeout) {
    var self = $rootScope.$new();

    var helpHub = $.connection.centralHub;

    // Bind all the client events to make sure they are emited
    var client = helpHub.client;

    client.appendChannel = function (channel) {
        $timeout(function() {
            self.$emit("appendChannel", channel);
        });
    }
    client.addQuestion = function(question, channelid) {
        self.$emit("addQuestion", question, channelid);
    }
    client.removeQuestion = function(questionid) {
        self.$emit("removeQuestion", questionid);
        self.$apply();
    }
    client.exitChannel = function(channelid) {
        self.$emit("exitChannel", channelid);
        self.$apply();
    }
    client.sendQuestion = function(question) {
        self.$emit("sendQuestion", question);
        self.$apply();
    }
    client.updateQuestion = function(questionText, questionid, channelid) {
        self.$emit("updateQuestion", questionText, questionid, channelid);
        self.$apply();
    }
    client.sendChatMessage = function(message, channelid) {
        self.$emit("sendChatMessage", message, channelid);
        self.$apply();
    }
    client.checkVersion = function(version) {
        self.$emit("checkVersion", version);
        self.$apply();
    }
    client.removeChatMessage = function(messageid) {
        self.$emit("removeChatMessage", messageid);
        self.$apply();
    }
    client.clearChat = function(channelid) {
        self.$emit("clearChat", channelid);
        self.$apply();
    }
    client.loginSuccess = function() {
        self.$emit("loginSuccess");
        self.$apply();
    }
    client.userCreationSuccess = function() {
        self.$emit("userCreationSuccess");
        self.$apply();
    }
    client.userLoggedOut = function() {
        self.$emit("userLoggedOut");
        self.$apply();
    }
    client.updateUsername = function(name) {
        self.$emit("updateUsername", name);
        self.$apply();
    }
    client.appendUser = function(user, channelid) {
        self.$emit("appendUser", user, channelid);
        self.$apply();
    }
    client.removeUser = function(id, channelid) {
        self.$emit("removeUser", id, channelid);
        self.$apply();
    }
    client.alert = function(message) {
        self.$emit("alert", message);
        self.$apply();
    }
    client.setQuestionState = function(hasquestion, channelid) {
        self.$emit("setQuestionState", hasquestion, channelid);
        self.$apply();
    }
    client.sendUserId = function(id) {
        self.$emit("sendUserId", id);
        self.$apply();
    }
    client.updateOtherUsername = function(name, userid, channelid) {
        self.$emit("updateOtherUsername", name, userid, channelid);
        self.$apply();
    }
    client.setAdminState = function(id, isadmin) {
        self.$emit("setAdminState", id, isadmin);
        self.$apply();
    }
    client.clearChannels = function() {
        self.$emit("clearChannels");
        self.$apply();
    }
    client.sendReloginData = function(key, id) {
        self.$emit("sendReloginData", key, id);
        self.$apply();
    }
    client.tokenLoginFailed = function() {
        self.$emit("tokenLoginFailed");
        self.$apply();
    }
    client.passwordResetRequestResult = function(success) {
        self.$emit("passwordResetRequestResult", success);
        self.$apply();
    }
    client.passwordResetResult = function(success) {
        self.$emit("passwordResetResult", success);
        self.$apply();
    }
    client.passwordChanged = function(success) {
        self.$emit("passwordChanged", success);
        self.$apply();
    }
    client.allUsersLoggedOut = function() {
        self.$emit("allUsersLoggedOut");
        self.$apply();
    }
    client.setChannel = function(id) {
        self.$emit("setChannel", id);
        self.$apply();
    }
    client.cleanupTime = function() {
        self.$emit("cleanupTime");
        self.$apply();
    }

    $.connection.hub.start().done(function() {
        self.$emit("connectionStarted");
        self.$apply();
    });

    // Bind the server to the signalr service
    self.server = helpHub.server;

    return self;
}

angular.module("zlSignalR", [])
    .factory("SignalR", ["$rootScope", "$timeout", SignalRFactory]);