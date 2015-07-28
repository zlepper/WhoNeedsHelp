/// <reference path="Scripts/typings/jquery/jquery.d.ts" />
/// <reference path="Scripts/typings/signalr/signalr.d.ts" />
/// <reference path="Scripts/typings/bootstrap/bootstrap.d.ts"/>
/// <reference path="Scripts/typings/jqueryui/jqueryui.d.ts"/>
/// <reference path="Scripts/typings/jquery.pnotify/jquery.pnotify.d.ts"/>
/// <reference path="scripts/typings/angularjs/angular.d.ts" />
/// <reference path="scripts/typings/angularjs/angular-animate.d.ts" />
/// <reference path="scripts/typings/angular-ui-bootstrap/angular-ui-bootstrap.d.ts" />
/// <reference path="scripts/typings/angularjs/angular-cookies.d.ts" />
var __extends = (this && this.__extends) || function (d, b) {
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
var confirmNotice = null;
var Help;
(function (Help) {
    var app = angular.module("Help", ["ui.bootstrap", "ngAnimate", "ngCookies"]);
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
        ServerActions.prototype.alert = function (typ, text, title) {
            // ReSharper disable once UnusedLocals
            var notify = new PNotify({
                title: title,
                text: text,
                type: typ,
                animation: "show",
                styling: "fontawesome",
                mouse_reset: false,
                desktop: {
                    desktop: document.hidden
                },
                click: function (n) {
                    n.remove();
                }
            });
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
    var HelpCtrl = (function (_super) {
        __extends(HelpCtrl, _super);
        function HelpCtrl($scope, $Modal, $timeout, $cookieStore) {
            var _this = this;
            _super.call(this);
            this.$scope = $scope;
            this.$Modal = $Modal;
            this.$timeout = $timeout;
            this.$cookieStore = $cookieStore;
            $scope.Loading = true;
            $scope.StartingModal = new LoginOptions();
            $scope.Me = new Me();
            $scope.Channels = {};
            $scope.ActiveChannel = 0;
            $scope.editQuestionText = { text: "" };
            $scope.lastActiveChannel = 0;
            this.helper = $.connection.centralHub;
            var that = this;
            $scope.$watch("ActiveChannel", function (newValue, oldValue) {
                $scope.lastActiveChannel = oldValue;
            });
            $scope.changeUsernamePopover = {
                templateUrl: "/templates/changeUsernamePopover.html",
                title: "Skift brugernavn"
            };
            $scope.createUserPopover = {
                templateUrl: "/templates/createUserPopover.html",
                title: "Opret bruger"
            };
            $scope.loginUserPopover = {
                templateUrl: "/templates/loginPopover.html",
                title: "Login"
            };
            $scope.LoginModalOptions = {
                templateUrl: "/templates/startModal.html",
                scope: $scope,
                keyboard: false,
                backdrop: "static",
                animation: false
            };
            $scope.changeQuestionModalOptions = {
                templateUrl: "/templates/editQuestionModal.html",
                scope: $scope,
                keyboard: false,
                backdrop: "static",
                animation: false
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
                    if ($scope.LoginModal) {
                        $scope.Ready = true;
                        $scope.LoginModal.close();
                        $scope.LoginModal = null;
                    }
                    else {
                        $timeout(function () {
                            jQuery("#editUsername").click();
                        });
                    }
                }
            };
            $.connection.hub.start().done(function () {
                $timeout(function () {
                    var token = $cookieStore.get("token");
                    if (!token) {
                        $scope.LoginModal = $Modal.open($scope.LoginModalOptions);
                    }
                    else {
                        _this.loginWithToken(token.id, token.key);
                    }
                    $scope.Loading = false;
                });
            });
            this.helper.client.tokenLoginFailed = function () {
                $timeout(function () {
                    $scope.LoginModal = $Modal.open($scope.LoginModalOptions);
                });
            };
            $scope.exitChannel = function (channelid) {
                _this.confirm("Er du sikker på at du vil lukke kanalen?", "Bekræftelse nødvendig", function () {
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
                $timeout(function () {
                    if ($scope.Channels[channelid] != null)
                        $scope.Channels[channelid].HaveQuestion = hasQuestion;
                });
            };
            this.helper.client.updateUsername = function (name) {
                $timeout(function () {
                    $scope.Me.Name = name;
                }, 0);
            };
            this.helper.client.sendUserId = function (id) {
                $timeout(function () {
                    $scope.Me.Id = id;
                }, 0);
            };
            this.helper.client.appendChannel = function (channel) {
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
                $timeout(function () {
                    $scope.ActiveChannel = channel.Id;
                    $scope.Channels[channel.Id] = channel;
                });
                MathJax.Hub.Queue(["Typeset", MathJax.Hub]);
            };
            this.helper.client.exitChannel = function (channelId) {
                $timeout(function () {
                    delete $scope.Channels[channelId];
                    if ($scope.ActiveChannel === channelId) {
                        if ($scope.Channels[$scope.lastActiveChannel] != null) {
                            $scope.ActiveChannel = $scope.lastActiveChannel;
                        }
                        else {
                            if (Object.keys($scope.Channels).length > 0) {
                                $scope.ActiveChannel = Number(Object.keys($scope.Channels)[0]);
                            }
                            else {
                                $scope.ActiveChannel = 0;
                                $scope.lastActiveChannel = 0;
                            }
                        }
                    }
                });
            };
            this.helper.client.addQuestion = function (question, channelid) {
                question.User = $scope.Channels[channelid].Users[question.User.Id];
                $timeout(function () {
                    $scope.Channels[channelid].Questions[question.Id] = question;
                });
                MathJax.Hub.Queue(["Typeset", MathJax.Hub]);
                if ($scope.Channels[channelid].IsAdmin) {
                    if (document.hidden) {
                        _this.alert("info", question.User.Name + " har brug for hjælp." + (question.Text ? "\nTil sp\u00F8rgsm\u00E5let er teksten: \"" + question.Text + "\"" : ""), "Nyt spørgsmål");
                    }
                }
            };
            this.helper.client.removeQuestion = function (questionid) {
                $timeout(function () {
                    for (var channelid in $scope.Channels) {
                        if ($scope.Channels.hasOwnProperty(channelid)) {
                            if ($scope.Channels[channelid].Questions[questionid] != null) {
                                delete $scope.Channels[channelid].Questions[questionid];
                            }
                        }
                    }
                });
            };
            this.helper.client.sendQuestion = function (questionText) {
                $timeout(function () {
                    $scope.editQuestionText.text = questionText;
                    $scope.changeQuestionModal = $Modal.open($scope.changeQuestionModalOptions);
                });
            };
            $scope.UpdateQuestion = function () {
                _this.changeQuestion($scope.editQuestionText.text, $scope.ActiveChannel);
                $scope.changeQuestionModal.close();
            };
            this.helper.client.updateQuestion = function (questionText, questionid, channelid) {
                $timeout(function () {
                    if ($scope.Channels[channelid] != null) {
                        if ($scope.Channels[channelid].Questions[questionid] != null) {
                            $scope.Channels[channelid].Questions[questionid].Text = questionText;
                        }
                    }
                });
                MathJax.Hub.Queue(["Typeset", MathJax.Hub]);
            };
            $scope.CloseEditModal = function () {
                $scope.changeQuestionModal.close();
            };
            this.helper.client.appendUser = function (user, channelid) {
                if ($scope.Channels[channelid] != null) {
                    $timeout(function () {
                        $scope.Channels[channelid].Users[user.Id] = user;
                    });
                }
            };
            this.helper.client.removeUser = function (userid, channelid) {
                if ($scope.Channels[channelid] != null) {
                    if ($scope.Channels[channelid].Users[userid] != null) {
                        $timeout(function () {
                            delete $scope.Channels[channelid].Users[userid];
                        });
                    }
                }
            };
            $scope.RemoveUser = function (userid) {
                _this.removeUserFromChannel(userid, $scope.ActiveChannel);
            };
            $scope.RemoveChatMessage = function (messageId) {
                _this.removeChatMessage(messageId);
            };
            this.helper.client.removeChatMessage = function (messageId) {
                $timeout(function () {
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
                });
            };
            $scope.Chat = function () {
                var mes = $scope.Channels[$scope.ActiveChannel].MessageText;
                if (mes) {
                    _this.chat(mes, $scope.ActiveChannel);
                }
                $scope.Channels[$scope.ActiveChannel].MessageText = "";
            };
            this.helper.client.sendChatMessage = function (message, channelId) {
                setTimeout(function () {
                    if (message.Text.toLowerCase().indexOf($scope.Me.Name.toLowerCase()) !== -1) {
                        if (message.User.Id !== $scope.Me.Id) {
                            if (channelId !== $scope.ActiveChannel || !document.hidden) {
                                _this.alert("info", message.Text, $scope.Channels[channelId].ChannelName);
                            }
                        }
                    }
                }, 0);
                $timeout(function () {
                    message.User = $scope.Channels[channelId].Users[message.User.Id];
                    $scope.Channels[channelId].ChatMessages[message.Id] = message;
                });
            };
            this.helper.client.alert = function (message, heading, oftype) {
                _this.alert(oftype, message, heading);
            };
            $scope.createUser = function () {
                if ($scope.StartingModal.Password !== $scope.StartingModal.Passwordcopy) {
                    _this.alert("error", "Kodeord stemmer ikke overens", "Problem med kodeord");
                    return;
                }
                if (!$scope.StartingModal.Name || !$scope.StartingModal.Email) {
                    _this.alert("error", "Du har felter der endnu ikke er udfyldte", "Mangelende information");
                    return;
                }
                var email = $scope.StartingModal.Email;
                var pass = $scope.StartingModal.Password;
                var name = $scope.StartingModal.Name;
                // Simple checks to see if this is an email
                if (email.indexOf("@") === 0 || email.indexOf("@") === email.length - 1 || email.indexOf(".") === 0 || email.indexOf(".") === email.length - 1) {
                    return;
                }
                _this.createNewUser(name, email, pass, $scope.StartingModal.StayLoggedIn);
            };
            this.helper.client.userCreationSuccess = function () {
                $("#createUserBtn").click();
                setTimeout(function () {
                    $scope.Me.LoggedIn = true;
                    $scope.StartingModal.Passwordcopy = "";
                    $scope.StartingModal.Password = "";
                    $scope.$apply();
                }, 1000);
                _this.alert("success", "Din bruger er nu oprettet", "Oprettelse lykkedes");
            };
            $scope.logout = function () {
                var token = $cookieStore.get("token");
                if (!token)
                    _this.logoutUser(null);
                else
                    _this.logoutUser(token.key);
            };
            this.helper.client.userLoggedOut = function () {
                $timeout(function () {
                    $scope.Me.LoggedIn = false;
                    for (var ch in $scope.Channels) {
                        if ($scope.Channels.hasOwnProperty(ch)) {
                            delete $scope.Channels[ch];
                        }
                    }
                    $scope.setActiveChannel(0);
                    $cookieStore.remove("token");
                });
            };
            $scope.login = function () {
                if (!$scope.StartingModal.Email || !$scope.StartingModal.Password) {
                    _this.alert("error", "Manglende info", "Manglende info");
                    return;
                }
                _this.loginUser($scope.StartingModal.Email, $scope.StartingModal.Password, $scope.StartingModal.StayLoggedIn);
            };
            function loginClear() {
                $scope.Me.LoggedIn = true;
                $scope.StartingModal.Passwordcopy = "";
                $scope.StartingModal.Password = "";
                $scope.$apply();
            }
            this.helper.client.loginSuccess = function () {
                if ($scope.LoginModal) {
                    $scope.LoginModal.close();
                    $scope.LoginModal = null;
                    loginClear();
                }
                else {
                    $("#loginBtn").click();
                    setTimeout(loginClear(), 1000);
                }
                _this.alert("success", "Du er nu logget ind.", "Login successfuld");
            };
            this.helper.client.updateOtherUsername = function (name, userid, channelid) {
                $timeout(function () {
                    $scope.Channels[channelid].Users[userid].Name = name;
                });
            };
            this.helper.client.setAdminState = function (channelId, isAdmin) {
                $timeout(function () {
                    $scope.Channels[channelId].IsAdmin = isAdmin;
                });
            };
            $scope.ClearChat = function () {
                _this.confirm("Er du sikker på at du vil ryde chatten?", "Bekræftelse nødvendigt", function () {
                    _this.clearChat($scope.ActiveChannel);
                });
            };
            this.helper.client.clearChat = function (channelId) {
                $timeout(function () {
                    var chatMessages = $scope.Channels[channelId].ChatMessages;
                    for (var chatMessageId in chatMessages) {
                        if (chatMessages.hasOwnProperty(chatMessageId)) {
                            delete chatMessages[chatMessageId];
                        }
                    }
                });
            };
            this.helper.client.clearChannels = function () {
                $timeout(function () {
                    for (var channelId in $scope.Channels) {
                        if ($scope.Channels.hasOwnProperty(channelId)) {
                            delete $scope.Channels[channelId];
                        }
                    }
                });
            };
            this.helper.client.sendReloginData = function (key, id) {
                $timeout(function () {
                    var token = new LoginToken(id, key);
                    $cookieStore.put("token", token, { expires: new Date(Date.now() + 1000 * 60 * 60 * 24 * 30) });
                    $scope.Me.LoggedIn = true;
                });
            };
        }
        HelpCtrl.$inject = ["$scope", "$modal", "$timeout", "$cookieStore"];
        return HelpCtrl;
    })(ServerActions);
    Help.HelpCtrl = HelpCtrl;
    app.controller("HelpCtrl", HelpCtrl);
    app.filter("keylength", function () { return function (input) {
        if (!angular.isObject(input)) {
            throw Error("Usage of non-objects with keylength filter!!");
        }
        return Object.keys(input).length;
    }; }).filter("toArray", function () { return function (obj) {
        if (!(obj instanceof Object)) {
            return obj;
        }
        return Object.keys(obj).map(function (key) { return Object.defineProperty(obj[key], "$key", { __proto__: null, value: key }); });
    }; });
    ;
    app.directive("wrapper", [
        function () {
            return {
                restrict: "C",
                link: function (scope, element) {
                    var innerElement = element.find("inner");
                    scope.$watch(function () {
                        return innerElement[0].offsetHeight;
                    }, function (value) {
                        element.css("height", value + "px");
                    }, true);
                }
            };
        }
    ]);
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
    var LoginToken = (function () {
        function LoginToken(i, k) {
            this.id = i;
            this.key = k;
        }
        return LoginToken;
    })();
    Help.LoginToken = LoginToken;
})(Help || (Help = {}));
//# sourceMappingURL=helper.js.map