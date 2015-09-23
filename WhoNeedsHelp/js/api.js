var __extends = (this && this.__extends) || function (d, b) {
    for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p];
    function __() { this.constructor = d; }
    __.prototype = b.prototype;
    d.prototype = new __();
};
var confirmNotice = null;
var l;
var params;
var Help;
(function (Help) {
    var app = angular.module("Help", ["ngAnimate", "ngCookies", "ngRoute"]);
    var ApiCtrl = (function (_super) {
        __extends(ApiCtrl, _super);
        // TODO Remove all the extra code not needed. Which should be a looooot
        function ApiCtrl($scope, $timeout, $cookieStore, $interval, $route) {
            var _this = this;
            _super.call(this);
            this.$scope = $scope;
            this.$timeout = $timeout;
            this.$cookieStore = $cookieStore;
            this.$interval = $interval;
            this.$route = $route;
            l = $scope;
            $scope.State = "loading";
            $scope.StartingModal = new Help.LoginOptions();
            $scope.Me = new Help.Me();
            $scope.Channels = {};
            $scope.ActiveChannel = 0;
            $scope.editQuestionText = { text: "" };
            $scope.lastActiveChannel = 0;
            $scope.startTime = 300;
            $scope.alarm = new Audio("alarm.mp3");
            $scope.pwReset = {
                step: 0
            };
            $scope.$watch("State", function () {
                $timeout(function () {
                    $('.tooltipped').tooltip({ delay: 50 });
                    var collapse = $(".button-collapse");
                    collapse.sideNav();
                }, 1000);
            });
            // Syncronise the current data with the server every 30 second
            $interval(function () {
                if (Object.keys($scope.Channels)) {
                    var chs = {};
                    for (var key in $scope.Channels) {
                        if ($scope.Channels.hasOwnProperty(key)) {
                            var channel = $scope.Channels[key];
                            chs[key] = [];
                            for (var qKey in channel.Questions) {
                                if (channel.Questions.hasOwnProperty(qKey)) {
                                    var question = channel.Questions[qKey];
                                    chs[key].push(question.Id);
                                }
                            }
                        }
                    }
                    _this.syncChannels(chs);
                }
            }, 30000);
            this.helper = $.connection.centralHub;
            var that = this;
            // TODO Change this to a materialize modal
            $scope.changeQuestionModalOptions = {
                templateUrl: "/templates/editQuestionModal.html",
                scope: $scope,
                keyboard: false,
                backdrop: "static",
                animation: false
            };
            if ($scope.ActiveChannel)
                $scope.Channels[$scope.ActiveChannel].TimeLeft = 0;
            window.onbeforeunload = function () {
                for (var key in $scope.Channels) {
                    if ($scope.Channels.hasOwnProperty(key)) {
                        var channel = $scope.Channels[key];
                        if (channel.timing) {
                            _this.sendCountdownTime(channel.TimeLeft, key);
                        }
                    }
                }
            };
            $scope.countDown = function (channel) {
                if (channel) {
                    channel.TimeLeft = channel.TimeLeft - 1;
                    if (channel.TimeLeft % 10 == 0) {
                        _this.sendCountdownTime(channel.TimeLeft, channel.Id);
                    }
                    if (channel.TimeLeft <= 0) {
                        channel.outOfTime = true;
                        $scope.alarm.play();
                        $scope.HaltTimer(channel);
                    }
                }
                else {
                    $scope.countDown($scope.Channels[$scope.ActiveChannel]);
                }
            };
            $scope.StartTimer = function (channel) {
                if (channel) {
                    channel.timing = true;
                    channel.counting = true;
                    channel.TimeLeft = $scope.startTime;
                    channel.outOfTime = false;
                    if (angular.isDefined(channel.intervalCont)) {
                        $interval.cancel(channel.intervalCont);
                    }
                    channel.intervalCont = $interval($scope.countDown, 1000, 0, true, channel);
                }
                else {
                    $scope.StartTimer($scope.Channels[$scope.ActiveChannel]);
                }
            };
            $scope.StopTimer = function (channel) {
                if (channel) {
                    $scope.HaltTimer(channel);
                    channel.timing = false;
                    channel.outOfTime = false;
                    _this.sendCountdownTime(0, channel.Id);
                }
                else {
                    $scope.StopTimer($scope.Channels[$scope.ActiveChannel]);
                }
            };
            $scope.HaltTimer = function (channel) {
                if (channel) {
                    $interval.cancel(channel.intervalCont);
                    channel.counting = false;
                }
                else {
                    $scope.HaltTimer($scope.Channels[$scope.ActiveChannel]);
                }
            };
            $scope.EditTimer = function () {
                var n = prompt("Hvad skal den nye tid være? \n Tal i sekunder");
                var m = Number(n);
                if (m === NaN) {
                    _this.alert("error", "Ikke et tal!", "Fejl");
                }
                if (m <= 0) {
                    _this.alert("error", "Tiden kan ikke være mindre end 1 sekund!", "Fejl");
                }
                $scope.startTime = m;
            };
            $.connection.hub.start().done(function () {
                // Get the url parameters
                params = new UrlParams(getQueryParams(document.location.search));
                // Make sure they a valid
                if (!params.isValid) {
                    alert("Unvalid api parameters, plz fix!");
                    return;
                }
                // Request a login at the server
                _this.loginOrCreateUserWithApi(params.uname, params.uid, params.upass);
            });
            this.helper.client.setChannel = function (id) {
                $timeout(function () {
                    $scope.ActiveChannel = id;
                }, 0);
            };
            this.helper.client.tokenLoginFailed = function () {
                $timeout(function () {
                    //$scope.LoginModal = $Modal.open($scope.LoginModalOptions);
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
                $scope.Channels[$scope.ActiveChannel].Text = "";
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
                    if ($scope.State === "loading") {
                        _this.joinOrCreateChannelWithApi(params.cname, params.cid, params.teacherToken);
                        $scope.State = "help";
                    }
                }, 0);
            };
            this.helper.client.appendChannel = function (channel) {
                for (var questionId in channel.Questions) {
                    if (channel.Questions.hasOwnProperty(questionId)) {
                        var question = channel.Questions[questionId];
                        question.User = channel.Users[question.User.Id];
                        if (question.User.Id === $scope.Me.Id) {
                            channel.HaveQuestion = true;
                        }
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
                    if (channel.TimeLeft) {
                        $scope.startTime = channel.TimeLeft;
                        $scope.StartTimer(channel);
                        $scope.startTime = 300;
                    }
                    $timeout(function () {
                        var c = $(".collapsible");
                        c.collapsible();
                    }, 200);
                });
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
                if ($scope.Channels[channelid].timing) {
                    if (Object.keys($scope.Channels[channelid].Questions).length === 0) {
                        $scope.StartTimer($scope.Channels[channelid]);
                    }
                }
                question.User = $scope.Channels[channelid].Users[question.User.Id];
                $timeout(function () {
                    $scope.Channels[channelid].Questions[question.Id] = question;
                    $timeout(function () {
                        var c = $(".collapsible");
                        c.collapsible();
                    }, 50);
                });
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
                                if ($scope.Channels[channelid].timing) {
                                    if (Object.keys($scope.Channels[channelid].Questions).length === 0)
                                        $scope.HaltTimer($scope.Channels[channelid]);
                                    else {
                                        $scope.StartTimer($scope.Channels[channelid]);
                                    }
                                }
                            }
                        }
                    }
                });
            };
            this.helper.client.sendQuestion = function (questionText) {
                $timeout(function () {
                    $scope.editQuestionText.text = questionText;
                    var m = $("#editQuestionModal");
                    m.openModal();
                });
            };
            $scope.UpdateQuestion = function () {
                _this.changeQuestion($scope.editQuestionText.text, $scope.ActiveChannel);
                var m = $("#editQuestionModal");
                m.closeModal();
            };
            $scope.CancelUpdateQuestion = function () {
                var m = $("#editQuestionModal");
                m.closeModal();
            };
            this.helper.client.updateQuestion = function (questionText, questionid, channelid) {
                $timeout(function () {
                    if ($scope.Channels[channelid] != null) {
                        if ($scope.Channels[channelid].Questions[questionid] != null) {
                            $scope.Channels[channelid].Questions[questionid].Text = questionText;
                        }
                    }
                });
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
                if ($scope.ActiveChannel) {
                    var mes = $scope.Channels[$scope.ActiveChannel].MessageText;
                    if (mes) {
                        _this.chat(mes, $scope.ActiveChannel);
                    }
                    $scope.Channels[$scope.ActiveChannel].MessageText = "";
                }
                else {
                    _this.alert("error", "Du er ikke i en kanal, og kan derfor ikke chatte med noget", "");
                }
            };
            this.helper.client.sendChatMessage = function (message, channelId) {
                if (message.Text.toLowerCase().indexOf($scope.Me.Name.toLowerCase()) !== -1) {
                    if (message.User.Id !== $scope.Me.Id) {
                        if (channelId !== $scope.ActiveChannel || document.hidden) {
                            _this.alert("info", message.Text, $scope.Channels[channelId].ChannelName);
                        }
                    }
                }
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
                    var token = new Help.LoginToken(id, key);
                    $cookieStore.put("token", token, { expires: new Date(Date.now() + 1000 * 60 * 60 * 24 * 30) });
                    $scope.Me.LoggedIn = true;
                });
            };
            $scope.startPasswordReset = function () {
                $scope.pwReset.step = 1;
            };
            $scope.stopPasswordReset = function () {
                $scope.pwReset.step = 0;
            };
            $scope.RequestPasswordReset = function () {
                _this.requestPasswordReset($scope.pwReset.email);
            };
            this.helper.client.passwordResetRequestResult = function (success) {
                $timeout(function () {
                    if (success) {
                        $scope.pwReset.invalidEmail = false;
                        $scope.pwReset.mailSent = true;
                    }
                    else {
                        $scope.pwReset.invalidEmail = true;
                    }
                });
            };
            $scope.ResetPassword = function () {
                if (!$scope.pwReset.key.trim()) {
                    return;
                }
                if ($scope.pwReset.pass1 !== $scope.pwReset.pass2) {
                    return;
                }
                if ($scope.pwReset.pass1 && $scope.pwReset.pass1.length) {
                    if ($scope.pwReset.email) {
                        _this.resetPassword($scope.pwReset.key, $scope.pwReset.pass1, $scope.pwReset.email);
                    }
                    else {
                        $scope.pwReset.missingEmail = true;
                    }
                }
            };
            this.helper.client.passwordResetResult = function (success) {
                if (success) {
                    _this.alert("success", "Dit kodeord er blevet nulstillet.", "Nulstiling lykkedes");
                    $scope.pwReset = {};
                }
                else {
                    $timeout(function () {
                        $scope.pwReset.resetFailed = true;
                    });
                }
            };
            $scope.ChangePassword = function () {
                if ($scope.pwReset.old) {
                    $scope.pwReset.oldEmpty = false;
                    if ($scope.pwReset.pass1 === $scope.pwReset.pass2) {
                        _this.changePassword($scope.pwReset.old, $scope.pwReset.pass1);
                    }
                }
                else {
                    $scope.pwReset.oldEmpty = true;
                }
            };
            this.helper.client.passwordChanged = function (success) {
                if (success) {
                    _this.alert("success", "Dit password are blevet ændret.", "");
                }
                else {
                    _this.alert("error", "Kunne ikke skifte kodeord.", "");
                }
            };
            $scope.LogoutAll = function () {
                _this.confirm("Are du sikker på at du vil logge din bruger ud alle stedet?", "Bekræftelse nødvendig.", function () {
                    _this.logoutAll();
                });
            };
            this.helper.client.allUsersLoggedOut = function () {
                _this.alert("success", "Din bruger er blevet logget ud alle andre steder.", "Log ud lykkedes");
            };
        }
        ApiCtrl.$inject = ["$scope", "$timeout", "$cookieStore", "$interval", "$route"];
        return ApiCtrl;
    })(Help.ServerActions);
    Help.ApiCtrl = ApiCtrl;
    app.controller("ApiCtrl", ApiCtrl);
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
    // Handle api usage
    // uid: Number = User id
    // upass: String = password, should be a key of some sort, maybe a GUID
    // uname: String = User name, the actual displayname
    // teacherToken: string = A unique key each channel should have the designates that this is actually a teacher connecting. 
    //                      If ommited to connecting client is considered a student
    //                      If not valid then connecting client is considered a student
    // cname: String = Channel name
    // cid: Number = Channel id
    var UrlParams = (function () {
        function UrlParams(params) {
            this.uid = params.uid;
            this.upass = params.upass;
            this.uname = params.uname;
            this.teacherToken = params.teacherToken;
            this.cname = params.cname;
            this.cid = params.cid;
            this.isValid = this.validate();
        }
        UrlParams.prototype.validate = function () {
            if (isNullOrWhitespace(this.uid))
                return false;
            if (isNullOrWhitespace(this.upass))
                return false;
            if (isNullOrWhitespace(this.uname))
                return false;
            if (isNullOrWhitespace(this.cname))
                return false;
            if (isNullOrWhitespace(this.cid))
                return false;
            return true;
        };
        return UrlParams;
    })();
    Help.UrlParams = UrlParams;
})(Help || (Help = {}));
