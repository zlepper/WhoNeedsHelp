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
        ServerActions.prototype.requestHelp = function (question, channelid) {
            return this.helper.server.requestHelp(question, channelid);
        };
        ServerActions.prototype.loginUser = function (mail, pass) {
            return this.helper.server.loginUser(mail, pass);
        };
        ServerActions.prototype.logoutUser = function () {
            return this.helper.server.logoutUser();
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
        ServerActions.prototype.showNotification = function (typ, text, title) {
            var notice = new PNotify({
                title: title,
                text: text,
                type: typ,
                animation: "show",
                styling: "fontawesome",
                mouse_reset: false,
                desktop: {
                    desktop: document.hidden
                }
            });
            notice.elem.click(function () {
                notice.remove();
            });
        };
        ServerActions.prototype.confirm = function (text, title, callback) {
            var notice = new PNotify({
                title: title,
                text: text,
                icon: "glyphicon glyphicon-question-sign",
                mouse_reset: false,
                hide: false,
                confirm: {
                    confirm: true
                },
                buttons: {
                    closer: false,
                    sticker: false
                },
                history: {
                    history: false
                }
            });
            notice.elem.on("pnotify.confirm", function () {
                callback();
            }).on("pnotify.cancel", function () { return false; });
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
            $scope.editQuestionText = { text: "" };
            $scope.createUserOptions = new LoginOptions();
            this.helper = $.connection.centralHub;
            var that = this;
            $scope.createUserPopover = {
                templateUrl: "/templates/createUserPopover.html",
                title: "Opret bruger"
            };
            $scope.LoginModalOptions = {
                templateUrl: "/templates/startModal.html",
                scope: $scope,
                keyboard: false,
                backdrop: "static"
            };
            $scope.changeQuestionModalOptions = {
                templateUrl: "/templates/editQuestionModal.html",
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
                }
                $scope.Ready = true;
                $scope.LoginModal.close();
            };
            $.connection.hub.start().done(function () {
                $scope.Loading = false;
                console.log($scope.LoginModalOptions);
                $scope.LoginModal = $Modal.open($scope.LoginModalOptions);
                $scope.$apply();
            });
            $scope.exitChannel = function (channelid) {
                _this.confirm("Are du sikker på at du vil lukke kanalen?", "Bekræftelse nødvendig", function () {
                    that.exitChannel(channelid);
                });
            };
            $scope.CreateNewChannel = function (channelName) {
                if (isNaN(Number(channelName))) {
                    _this.createNewChannel(channelName);
                }
                else {
                    _this.joinChannel(Number(channelName));
                }
                $scope.newChannelName = "";
            };
            $scope.RequestHelp = function () {
                var qt = $scope.Channels[$scope.ActiveChannel].Text;
                console.log($scope.Channels[$scope.ActiveChannel].Text);
                _this.requestHelp(qt, $scope.ActiveChannel);
            };
            $scope.RemoveQuestion = function (questionid) {
                _this.removeQuestion(questionid);
            };
            $scope.RemoveOwnQuestion = function () {
                _this.removeOwnQuestion($scope.ActiveChannel);
            };
            $scope.EditOwnQuestion = function () {
                _this.editOwnQuestion($scope.ActiveChannel);
            };
            this.helper.client.setQuestionState = function (hasQuestion, channelid) {
                if ($scope.Channels[channelid] != null)
                    $scope.Channels[channelid].HaveQuestion = hasQuestion;
                $scope.$apply();
            };
            this.helper.client.updateUsername = function (name) {
                $scope.Me.Name = name;
                $scope.$apply();
            };
            this.helper.client.sendUserId = function (id) {
                $scope.Me.Id = id;
                $scope.$apply();
            };
            this.helper.client.appendChannel = function (channel) {
                $scope.ActiveChannel = channel.Id;
                $scope.Channels[channel.Id] = channel;
                $scope.$apply();
                MathJax.Hub.Queue(["Typeset", MathJax.Hub]);
            };
            this.helper.client.exitChannel = function (channelId) {
                delete $scope.Channels[channelId];
                $scope.ActiveChannel = Number(Object.keys($scope.Channels)[0]);
                $scope.$apply();
            };
            this.helper.client.addQuestion = function (question, channelid) {
                $scope.Channels[channelid].Questions[question.Id] = question;
                $scope.$apply();
                MathJax.Hub.Queue(["Typeset", MathJax.Hub]);
            };
            this.helper.client.removeQuestion = function (questionid) {
                for (var channelid in $scope.Channels) {
                    if ($scope.Channels.hasOwnProperty(channelid)) {
                        if ($scope.Channels[channelid].Questions[questionid] != null) {
                            delete $scope.Channels[channelid].Questions[questionid];
                            console.log($scope.Channels[channelid]);
                        }
                    }
                }
                $scope.$apply();
            };
            this.helper.client.sendQuestion = function (questionText) {
                $scope.editQuestionText.text = questionText;
                $scope.changeQuestionModal = $Modal.open($scope.changeQuestionModalOptions);
            };
            $scope.UpdateQuestion = function () {
                console.log($scope.editQuestionText);
                _this.changeQuestion($scope.editQuestionText.text, $scope.ActiveChannel);
                $scope.changeQuestionModal.close();
            };
            this.helper.client.updateQuestion = function (questionText, questionid, channelid) {
                if ($scope.Channels[channelid] != null) {
                    if ($scope.Channels[channelid].Questions[questionid] != null) {
                        $scope.Channels[channelid].Questions[questionid].Text = questionText;
                    }
                }
                $scope.$apply();
                MathJax.Hub.Queue(["Typeset", MathJax.Hub]);
            };
            $scope.CloseEditModal = function () {
                $scope.changeQuestionModal.close();
            };
            this.helper.client.appendUser = function (user, channelid) {
                if ($scope.Channels[channelid] != null) {
                    $scope.Channels[channelid].Users[user.Id] = user;
                    $scope.$apply();
                }
            };
            this.helper.client.removeUser = function (userid, channelid) {
                if ($scope.Channels[channelid] != null) {
                    if ($scope.Channels[channelid].Users[userid] != null) {
                        delete $scope.Channels[channelid].Users[userid];
                        $scope.$apply();
                    }
                }
            };
            $scope.RemoveUser = function (userid) {
                _this.removeUserFromChannel(userid, $scope.ActiveChannel);
            };
            $scope.RemoveChatMessage = function (messageId) {
                console.log(messageId);
                _this.removeChatMessage(messageId);
            };
            this.helper.client.removeChatMessage = function (messageId) {
                for (var channel in $scope.Channels) {
                    if ($scope.Channels.hasOwnProperty(channel)) {
                        var ch = $scope.Channels[channel];
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
                $scope.$apply();
            };
            $scope.Chat = function () {
                var mes = $scope.Channels[$scope.ActiveChannel].MessageText;
                if (mes) {
                    _this.chat(mes, $scope.ActiveChannel);
                }
                $scope.Channels[$scope.ActiveChannel].MessageText = "";
            };
            this.helper.client.sendChatMessage = function (message, channelId) {
                $scope.Channels[channelId].ChatMessages[message.Id] = message;
                $scope.$apply();
            };
            this.helper.client.alert = function (message, heading, oftype) {
                console.log("ALERT!");
                _this.showNotification(oftype, message, heading);
            };
            $scope.createUser = function () {
                if ($scope.createUserOptions.Password !== $scope.createUserOptions.Passwordcopy || !$scope.createUserOptions.Name || !$scope.createUserOptions.Email) {
                    return;
                }
                var email = $scope.createUserOptions.Email;
                var pass = $scope.createUserOptions.Password;
                var name = $scope.createUserOptions.Name;
                // Simple checks to see if this is an email
                if (email.indexOf("@") === 0 || email.indexOf("@") === email.length - 1 || email.indexOf(".") === 0 || email.indexOf(".") === email.length - 1) {
                    return;
                }
                _this.createNewUser(name, email, pass);
            };
            this.helper.client.userCreationSuccess = function () {
                $scope.Me.LoggedIn = true;
                $scope.createUserOptions = new LoginOptions();
                $scope.$apply();
            };
            $scope.logout = function () {
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
            this.Text = questionText;
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
            this.Passwordcopy = "";
        }
        return LoginOptions;
    })();
    Help.LoginOptions = LoginOptions;
})(Help || (Help = {}));
//# sourceMappingURL=helper.js.map