/// <reference path="Scripts/typings/jquery/jquery.d.ts" />
/// <reference path="Scripts/typings/signalr/signalr.d.ts" />
/// <reference path="Scripts/typings/bootstrap/bootstrap.d.ts"/>
/// <reference path="Scripts/typings/jqueryui/jqueryui.d.ts"/>
/// <reference path="Scripts/typings/jquery.pnotify/jquery.pnotify.d.ts"/>
/// <reference path="scripts/typings/angularjs/angular.d.ts" />
/// <reference path="scripts/typings/angularjs/angular-animate.d.ts" />
var Help;
(function (Help) {
    var app = angular.module("HelpCtrl", ["ui.bootstrap"]);
    var HelpCtrl = (function () {
        function HelpCtrl($scope) {
            this.$scope = $scope;
            this.helper = $.connection.centralHub;
            var that = this;
        }
        HelpCtrl.$inject = ["$scope"];
        return HelpCtrl;
    })();
    Help.HelpCtrl = HelpCtrl;
    (function (QuestionState) {
        QuestionState[QuestionState["HaveQuestion"] = 0] = "HaveQuestion";
        QuestionState[QuestionState["NoQuestion"] = 1] = "NoQuestion";
    })(Help.QuestionState || (Help.QuestionState = {}));
    var QuestionState = Help.QuestionState;
    var Question = (function () {
        function Question() {
        }
        return Question;
    })();
    Help.Question = Question;
    var User = (function () {
        function User() {
        }
        return User;
    })();
    Help.User = User;
    var Channel = (function () {
        function Channel() {
        }
        return Channel;
    })();
    Help.Channel = Channel;
    var ChatMessage = (function () {
        function ChatMessage() {
        }
        return ChatMessage;
    })();
    Help.ChatMessage = ChatMessage;
})(Help || (Help = {}));
//# sourceMappingURL=helper.js.map