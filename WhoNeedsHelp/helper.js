/// <reference path="Scripts/typings/jquery/jquery.d.ts" />
/// <reference path="Scripts/typings/signalr/signalr.d.ts" />
/// <reference path="Scripts/typings/bootstrap/bootstrap.d.ts"/>
/// <reference path="Scripts/typings/jqueryui/jqueryui.d.ts"/>
/// <reference path="Scripts/typings/jquery.pnotify/jquery.pnotify.d.ts"/>
/// <reference path="scripts/typings/angularjs/angular.d.ts" />
/// <reference path="scripts/typings/angularjs/angular-animate.d.ts" />
/// <reference path="scripts/typings/angular-ui-bootstrap/angular-ui-bootstrap.d.ts" />
var __extends = this.__extends || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    __.prototype = b.prototype;
    d.prototype = new __();
};
function isNullOrWhitespace(input) {
    if (typeof input === "undefined" || input == null)
        return true;
    return input.replace(/\s/g, "").length < 1;
}
var patt = /[\w][\wæøåöäÆØÅÖÄ ]+[\w]/;
function removeFromArray(arr, index) {
    return arr.slice(0, index).concat(arr.slice(index + 1));
}
var Help;
(function (Help) {
    var app = angular.module("Help", ["ui.bootstrap"]);
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
        ServerActions.prototype.clearChat = function () {
            return this.helper.server.clearChat();
        };
        ServerActions.prototype.createNewUser = function (username, email, password) {
            return this.helper.server.createNewUser(username, email, password);
        };
        ServerActions.prototype.requestActiveChannel = function () {
            return this.helper.server.requestActiveChannel();
        };
        ServerActions.prototype.loginUser = function (mail, pass) {
            return this.helper.server.loginUser(mail, pass);
        };
        ServerActions.prototype.logoutUser = function () {
            return this.helper.server.logoutUser();
        };
        ServerActions.prototype.removeUserFromChannel = function (id) {
            return this.helper.server.removeUserFromChannel(id);
        };
        return ServerActions;
    })();
    Help.ServerActions = ServerActions;
    var HelpCtrl = (function (_super) {
        __extends(HelpCtrl, _super);
        function HelpCtrl($scope, $Modal) {
            var _this = this;
            _super.call(this);
            this.$scope = $scope;
            this.$Modal = $Modal;
            $scope.Loading = true;
            $scope.StartingModal = new LoginOptions();
            $scope.Me = new Me();
            $scope.Channels = {};
            $scope.ActiveChannel = 0;
            this.helper = $.connection.centralHub;
            //var that = this;
            $scope.LoginModalOptions = {
                templateUrl: "/startModal.html",
                scope: $scope,
                keyboard: false,
                backdrop: "static"
            };
            $scope.setActiveChannel = function (channelid) {
                $scope.ActiveChannel = channelid;
            };
            $scope.Start = function () {
                var name = $scope.StartingModal.Name;
                name = name.replace(/[\s]+/g, " ");
                var n = name.match(patt);
                if (n.length > 0) {
                    _this.setUsername(n[0]);
                    console.log(n[0]);
                }
                $scope.Ready = true;
                $scope.LoginModal.close();
            };
            $.connection.hub.start().done(function () {
                $scope.Loading = false;
                console.log($scope.LoginModalOptions);
                $scope.LoginModal = $Modal.open($scope.LoginModalOptions);
            });
            $scope.exitChannel = function (channelid) {
                console.log(typeof (channelid));
                _this.exitChannel(channelid);
            };
            $scope.CreateNewChannel = function (channelName) {
                if (isNaN(Number(channelName))) {
                    _this.createNewChannel(channelName);
                }
                else {
                    _this.joinChannel(Number(channelName));
                }
            };
            this.helper.client.updateUsername = function (name) {
                $scope.Me.Name = name;
                $scope.$apply();
            };
            this.helper.client.appendChannel = function (channel) {
                $scope.ActiveChannel = channel.Id;
                $scope.Channels[channel.Id] = channel;
                //$scope.Channels.push(channel);
                $scope.$apply();
                console.log(channel);
            };
        }
        HelpCtrl.$inject = ["$scope", "$modal"];
        return HelpCtrl;
    })(ServerActions);
    Help.HelpCtrl = HelpCtrl;
    app.controller("HelpCtrl", HelpCtrl);
    app.filter("keylength", function () { return function (input) {
        if (!angular.isObject(input)) {
            throw Error("Usage of non-objects with keylength filter!!");
        }
        return Object.keys(input).length;
    }; });
    (function (QuestionState) {
        QuestionState[QuestionState["HaveQuestion"] = 0] = "HaveQuestion";
        QuestionState[QuestionState["NoQuestion"] = 1] = "NoQuestion";
    })(Help.QuestionState || (Help.QuestionState = {}));
    var QuestionState = Help.QuestionState;
    var Question = (function () {
        function Question(id, user, questionText) {
            this.Id = id;
            this.User = user;
            this.QuestionText = questionText;
        }
        return Question;
    })();
    Help.Question = Question;
    var User = (function () {
        function User(id, name) {
            this.Id = id;
            this.Name = name;
        }
        return User;
    })();
    Help.User = User;
    var Me = (function () {
        function Me() {
            this.Name = null;
            this.LoggedIn = false;
        }
        return Me;
    })();
    Help.Me = Me;
    var Channel = (function () {
        function Channel(id, channelName) {
            this.Id = id;
            this.ChannelName = channelName;
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
    var LoginOptions = (function () {
        function LoginOptions() {
            this.Name = "";
            this.Email = "";
            this.Password = "";
        }
        return LoginOptions;
    })();
    Help.LoginOptions = LoginOptions;
})(Help || (Help = {}));
//# sourceMappingURL=helper.js.map