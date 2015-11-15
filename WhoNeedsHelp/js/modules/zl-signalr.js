"use strict";
function SignalRFactory($rootScope, $timeout) {
    var self = $rootScope.$new();

    var helpHub = $.connection.centralHub;

    // Bind all the client events to make sure they are emited
    var client = helpHub.client;


    var methods = ["appendChannel", "addQuestion", "removeQuestion", "exitChannel", "sendQuestion", "updateQuestion", "sendChatMessage", "checkVersion", "removeChatMessage", "clearChat", "loginSuccess", "userCreationSuccess", "userLoggedOut", "updateUsername", "appendUser", "removeUser", "alert", "setQuestionState", "sendUserId", "updateOtherUsername", "setAdminState", "clearChannels", "sendReloginData", "tokenLoginFailed", "passwordResetRequestResult", "passwordResetResult", "passwordChanged", "allUsersLoggedOut", "setChannel", "cleanupTime"];

    for (var i = 0; i < methods.length; i++) {
        var m = methods[i];
        (function (method) {
            client[method] = function () {
                var a = [method];
                for (var j = 0; j < arguments.length; j++) {
                    a.push(arguments[j]);
                }
                $timeout(function () {
                    self.$emit.apply(self, a);
                });
            }
        })(m);
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