/// <reference path="Scripts/typings/jquery/jquery.d.ts" />
/// <reference path="Scripts/typings/signalr/signalr.d.ts" />
/// <reference path="Scripts/typings/bootstrap/bootstrap.d.ts"/>
/// <reference path="Scripts/typings/jqueryui/jqueryui.d.ts"/>
/// <reference path="Scripts/typings/jquery.pnotify/jquery.pnotify.d.ts"/>
/// <reference path="scripts/typings/angularjs/angular.d.ts" />
/// <reference path="scripts/typings/angularjs/angular-animate.d.ts" />
/// <reference path="scripts/typings/angular-ui-bootstrap/angular-ui-bootstrap.d.ts" />
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
    var HelpCtrl = (function () {
        function HelpCtrl($scope, $modal) {
            var _this = this;
            this.$scope = $scope;
            this.$modal = $modal;
            $scope.loading = true;
            this.helper = $.connection.centralHub;
            var that = this;
            $.connection.hub.start().done(function () {
                $scope.loading = false;
                $scope.loginModal = $modal.open($scope.loginModalOptions);
            });
            $scope.loginFromModal = function () {
                if (isNullOrWhitespace($scope.startingModal.email) || isNullOrWhitespace($scope.startingModal.password))
                    return;
                that.loginUser($scope.startingModal.email, $scope.startingModal.password);
                $scope.startingModal.password = "";
            };
            $scope.start = function () {
                var name = $scope.startingModal.name;
                name = name.replace(/[\s]+/g, " ");
                var n = name.match(patt);
                if (n.length > 0)
                    _this.setUsername(n[0]);
                $scope.ready = true;
            };
            $scope.loginModalOptions = {
                templateUrl: "startModal.html",
                controller: "HelpCtrl",
                keyboard: false,
                backdrop: false
            };
            this.helper.client.appendChannel = function (channelname, channelId) {
                var channel = new Channel(channelId, channelname);
                $scope.channels.push(channel);
                $scope.activeChannel = channel;
                $scope.$apply();
            };
            this.helper.client.appendChannel2 = function (channelname, channelId) {
                var channel = new Channel(channelId, channelname);
                $scope.channels.push(channel);
                $scope.$apply();
            };
            this.helper.client.setChannel = function (channelId, areUserQuestioning) {
                for (var i = 0; i < $scope.channels.length; i++) {
                    var channel = $scope.channels[0];
                    channel.currentlyActive = channel.id === channelId;
                    if (channel.currentlyActive) {
                        $scope.activeChannel = channel;
                    }
                }
                $scope.activeChannel.haveQuestion = areUserQuestioning;
            };
            this.helper.client.removeUser = function (id, channelId) {
                for (var channel in $scope.channels) {
                    if ($scope.channels.hasOwnProperty(channel)) {
                        if (channel.id === channelId) {
                            for (var i = 0; i < channel.users.length; i++) {
                                if (channel.users[i].id === id) {
                                    channel.users = removeFromArray(channel.users, i);
                                    return;
                                }
                            }
                        }
                    }
                }
            };
            this.helper.client.updateQuestion = function (questionText, questionId, channelId) {
                for (var channel in $scope.channels) {
                    if ($scope.channels.hasOwnProperty(channel)) {
                        if (channel.id === channelId) {
                            for (var question in channel.questions) {
                                if (channel.questions.hasOwnProperty(question)) {
                                    if (question.id === questionId) {
                                        question.questionText = questionText;
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            };
            this.helper.client.showChannels = function (channelIds, channelNames) {
                $scope.channels.length = 0;
                for (var i = 0; i < channelIds.length; i++) {
                    var channel = new Channel(channelIds[i], channelNames[i]);
                    $scope.channels.push(channel);
                }
                _this.requestActiveChannel();
            };
            this.helper.client.exitChannel = function (channelId) {
                var index = 0;
                for (var i = 0; i < $scope.channels.length; i++) {
                    if ($scope.channels[i].id === channelId) {
                        index = i;
                        break;
                    }
                }
                $scope.channels = removeFromArray($scope.channels, index);
                if ($scope.channels.length !== 0) {
                    $scope.activeChannel = $scope.channels[0];
                }
                else {
                    $scope.activeChannel = null;
                }
            };
            this.helper.client.ipDiscover = function (ids, names) {
                $scope.discoveredIps.length = 0;
                for (var i = 0; i < ids.length; i++) {
                    var channel = new Channel(ids[i], names[i]);
                    $scope.discoveredIps.push(channel);
                }
            };
            this.helper.client.addQuestions = function (usernames, questions, questionIds) {
            };
        }
        HelpCtrl.prototype.send = function (action, parameters) {
            return this.helper.server.send(action, parameters);
        };
        HelpCtrl.prototype.getData = function (action) {
            return this.helper.server.getData(action);
        };
        HelpCtrl.prototype.setUsername = function (name) {
            return this.helper.server.setUsername(name);
        };
        HelpCtrl.prototype.createNewChannel = function (channelName) {
            return this.helper.server.createNewChannel(channelName);
        };
        HelpCtrl.prototype.loadHearbyChannels = function () {
            return this.helper.server.loadNearbyChannels();
        };
        HelpCtrl.prototype.changeToChannel = function (channelId) {
            return this.helper.server.changeToChannel(channelId);
        };
        HelpCtrl.prototype.exitChannel = function (channelId) {
            return this.helper.server.exitChannel(channelId);
        };
        HelpCtrl.prototype.joinChannel = function (channelId) {
            return this.helper.server.joinChannel(channelId);
        };
        HelpCtrl.prototype.removeQuestion = function (channelId) {
            return this.helper.server.removeQuestion(channelId);
        };
        HelpCtrl.prototype.removeChatMessage = function (messageId) {
            return this.helper.server.removeChatMessage(messageId);
        };
        HelpCtrl.prototype.searchForChannel = function (channelId) {
            return this.helper.server.searchForChannel(channelId);
        };
        HelpCtrl.prototype.chat = function (message) {
            return this.helper.server.chat(message);
        };
        HelpCtrl.prototype.clearChat = function () {
            return this.helper.server.clearChat();
        };
        HelpCtrl.prototype.createNewUser = function (username, email, password) {
            return this.helper.server.createNewUser(username, email, password);
        };
        HelpCtrl.prototype.requestActiveChannel = function () {
            return this.helper.server.requestActiveChannel();
        };
        HelpCtrl.prototype.loginUser = function (mail, pass) {
            return this.helper.server.loginUser(mail, pass);
        };
        HelpCtrl.prototype.logoutUser = function () {
            return this.helper.server.logoutUser();
        };
        HelpCtrl.prototype.removeUserFromChannel = function (id) {
            return this.helper.server.removeUserFromChannel(id);
        };
        HelpCtrl.$inject = ["$scope", "$modal"];
        return HelpCtrl;
    })();
    Help.HelpCtrl = HelpCtrl;
    app.controller("HelpCtrl", HelpCtrl);
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
        function User(id, name) {
            this.id = id;
            this.name = name;
        }
        return User;
    })();
    Help.User = User;
    var Channel = (function () {
        function Channel(id, channelName) {
            this.id = id;
            this.channelName = channelName;
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
        }
        return LoginOptions;
    })();
    Help.LoginOptions = LoginOptions;
})(Help || (Help = {}));
//# sourceMappingURL=helper.js.map