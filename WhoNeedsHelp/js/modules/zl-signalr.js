"use strict";
function SignalRFactory($rootScope) {
    var self = $rootScope.$new();

    var helpHub = $.connection.centralHub;

    // Bind all the client events to make sure they are emited
    var client = helpHub.client;
    
    client.appendChannel = function() {
        self.$emit("appendChannel", arguments);
    }
    client.addQuestion = function() {
        self.$emit("addQuestion", arguments);
    }
    client.removeQuestion = function() {
        self.$emit("removeQuestion", arguments);
    }
    client.exitChannel = function() {
        self.$emit("exitChannel", arguments);
    }
    client.sendQuestion = function() {
        self.$emit("sendQuestion", arguments);
    }
    client.updateQuestion = function() {
        self.$emit("updateQuestion", arguments);
    }
    client.sendChatMessage = function() {
        self.$emit("sendChatMessage", arguments);
    }
    client.checkVersion = function() {
        self.$emit("checkVersion", arguments);
    }
    client.removeChatMessage = function() {
        self.$emit("removeChatMessage", arguments);
    }
    client.clearChat = function() {
        self.$emit("clearChat", arguments);
    }
    client.loginSuccess = function() {
        self.$emit("loginSuccess", arguments);
    }
    client.userCreationSuccess = function() {
        self.$emit("userCreationSuccess", arguments);
    }
    client.userLoggedOut = function() {
        self.$emit("userLoggedOut", arguments);
    }
    client.updateUsername = function() {
        self.$emit("updateUsername", arguments);
    }
    client.appendUser = function() {
        self.$emit("appendUser", arguments);
    }
    client.removeUser = function() {
        self.$emit("removeUser", arguments);
    }
    client.alert = function() {
        self.$emit("alert", arguments);
    }
    client.setQuestionState = function() {
        self.$emit("setQuestionState", arguments);
    }
    client.sendUserId = function() {
        self.$emit("sendUserId", arguments);
    }
    client.updateOtherUsername = function() {
        self.$emit("updateOtherUsername", arguments);
    }
    client.setAdminState = function() {
        self.$emit("setAdminState", arguments);
    }
    client.clearChannels = function() {
        self.$emit("clearChannels", arguments);
    }
    client.sendReloginData = function() {
        self.$emit("sendReloginData", arguments);
    }
    client.tokenLoginFailed = function() {
        self.$emit("tokenLoginFailed", arguments);
    }
    client.passwordResetRequestResult = function() {
        self.$emit("passwordResetRequestResult", arguments);
    }
    client.passwordResetResult = function() {
        self.$emit("passwordResetResult", arguments);
    }
    client.passwordChanged = function() {
        self.$emit("passwordChanged", arguments);
    }
    client.allUsersLoggedOut = function() {
        self.$emit("allUsersLoggedOut", arguments);
    }
    client.setChannel = function() {
        self.$emit("setChannel", arguments);
    }
    client.cleanupTime = function() {
        self.$emit("cleanupTime", arguments);
    }

    $.connection.hub.start().done(function() {
        self.$emit("connectionStarted");
    });

    // Bind the server to the signalr service
    self.server = helpHub.server;

    return self;
}

angular.module("zlSignalR", [])
    .factory("SignalR", ["$rootScope", SignalRFactory]);