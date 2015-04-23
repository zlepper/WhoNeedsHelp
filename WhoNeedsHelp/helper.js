/// <reference path="Scripts/typings/jquery/jquery.d.ts" />
/// <reference path="Scripts/typings/signalr/signalr.d.ts" />
/// <reference path="Scripts/typings/bootstrap/bootstrap.d.ts"/>
/// <reference path="Scripts/typings/jqueryui/jqueryui.d.ts"/>
/// <reference path="Scripts/typings/jquery.pnotify/jquery.pnotify.d.ts"/>
/// <reference path="scripts/typings/angularjs/angular.d.ts" />
/// <reference path="scripts/typings/angularjs/angular-animate.d.ts" />
var Hjalp;
(function (Hjalp) {
    var app = angular.module("hjalpCtrl", ["ui.bootstrap"]);
    var Question = (function () {
        function Question() {
        }
        return Question;
    })();
    Hjalp.Question = Question;
    var User = (function () {
        function User() {
        }
        return User;
    })();
    Hjalp.User = User;
    var Channel = (function () {
        function Channel() {
        }
        return Channel;
    })();
    Hjalp.Channel = Channel;
    var ChatMessage = (function () {
        function ChatMessage() {
        }
        return ChatMessage;
    })();
    Hjalp.ChatMessage = ChatMessage;
})(Hjalp || (Hjalp = {}));
//# sourceMappingURL=helper.js.map