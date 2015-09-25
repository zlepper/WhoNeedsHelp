﻿var confirmNotice: any = null;

module Help {
    var app = angular.module("Help", ["ngAnimate", "ngCookies"]);

    export class HelpCtrl extends ServerActions {

        static $inject = ["$scope", "$timeout", "$cookieStore", "$interval"];

        constructor(public $scope: IHelpScope, public $timeout: any, public $cookieStore: any, public $interval: any) {
            super();
            $scope.State = "loading";
            
            $scope.StartingModal = new LoginOptions();
            $scope.Me = new Me();
            $scope.Channels = {};
            $scope.ActiveChannel = 0;
            $scope.editQuestionText = { text: "" };
            $scope.lastActiveChannel = 0;
            $scope.startTime = 300;
            $scope.alarm = new Audio("alarm.mp3");
            $scope.pwReset = {
                step: 0
            }

            $scope.$watch("State", () => {
                $timeout(() => {
                    $('.tooltipped').tooltip(<any>{ delay: 50 });
                    var collapse: any = $(".button-collapse");
                    collapse.sideNav();
                }, 1000);
            });

            // Syncronise the current data with the server every 30 second
            $interval(() => {
                if(Object.keys($scope.Channels)) {
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
                    this.syncChannels(chs);
                }
            }, 30000);

            this.helper = $.connection.centralHub;
            var that = this;

            $scope.$watch("ActiveChannel", (newValue: number, oldValue: number) => {
                $scope.lastActiveChannel = oldValue;
            });

            $scope.changeUsernamePopover = {
                templateUrl: "/templates/changeUsernamePopover.html",
                title: "Skift brugernavn"
            }
            $scope.createUserPopover = {
                templateUrl: "/templates/createUserPopover.html",
                title: "Opret bruger"
            };
            $scope.loginUserPopover = {
                templateUrl: "/templates/loginPopover.html",
                title: "Login"
            }
            $scope.changePasswordPopover = {
                templateUrl: "/templates/changePasswordPopover.html",
                title: "Skift kodeord"
            }
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

            if ($scope.ActiveChannel)
                $scope.Channels[$scope.ActiveChannel].TimeLeft = 0;

            window.onbeforeunload = () => {
                for (var key in $scope.Channels) {
                    if ($scope.Channels.hasOwnProperty(key)) {
                        var channel = $scope.Channels[key];
                        if (channel.timing) {
                            this.sendCountdownTime(channel.TimeLeft, key);
                        }
                    }
                }

            }

            $scope.countDown = (channel: Channel) => {
                if (channel) {
                    channel.TimeLeft = channel.TimeLeft - 1;
                    if (channel.TimeLeft % 10 == 0) {
                        this.sendCountdownTime(channel.TimeLeft, channel.Id);
                    }
                    if (channel.TimeLeft <= 0) {
                        channel.outOfTime = true;
                        $scope.alarm.play();
                        $scope.HaltTimer(channel);
                    }
                } else {
                    $scope.countDown($scope.Channels[$scope.ActiveChannel]);
                }
            }

            $scope.StartTimer = (channel: Channel) => {
                if (channel) {
                    channel.timing = true;
                    channel.counting = true;
                    channel.TimeLeft = $scope.startTime;
                    channel.outOfTime = false;
                    if (angular.isDefined(channel.intervalCont)) {
                        $interval.cancel(channel.intervalCont);
                    }
                    channel.intervalCont = $interval($scope.countDown, 1000, 0, true, channel);
                } else {
                    $scope.StartTimer($scope.Channels[$scope.ActiveChannel]);
                }
            }

            $scope.StopTimer = (channel: Channel) => {
                if (channel) {
                    $scope.HaltTimer(channel);
                    channel.timing = false;
                    channel.outOfTime = false;
                    this.sendCountdownTime(0, channel.Id);
                } else {
                    $scope.StopTimer($scope.Channels[$scope.ActiveChannel]);
                }
            }

            $scope.HaltTimer = (channel: Channel) => {
                if (channel) {
                    $interval.cancel(channel.intervalCont);
                    channel.counting = false;
                } else {
                    $scope.HaltTimer($scope.Channels[$scope.ActiveChannel]);
                }
            }
            $scope.EditTimer = () => {
                var n = prompt("Hvad skal den nye tid være? \n Tal i sekunder");
                var m = Number(n);
                if (m === NaN) {
                    return this.alert("error", "Ikke et tal!", "Fejl");
                }
                if (m <= 0) {
                    return this.alert("error", "Tiden kan ikke være mindre end 1 sekund!", "Fejl");
                }
                $scope.startTime = m;
            }

            $scope.setActiveChannel = (channelid) => {
                $scope.ActiveChannel = channelid;
                $timeout(() => {
                    var c: any = $(".collapsible");
                    c.collapsible();
                }, 100);
            };
            $scope.Start = () => {
                var name = $scope.StartingModal.Name;
                name = name.replace(/[\s]+/g, " ");
                var n = name.match(patt);
                if (n.length > 0) {
                    this.setUsername(n[0]);
                    $scope.State = "help";
                }
            };
            $.connection.hub.start().done(() => {
                $timeout(() => {
                    var token: LoginToken = $cookieStore.get("token");
                    if (!token) {
                        //$scope.LoginModal = $Modal.open($scope.LoginModalOptions);
                    } else {
                        this.loginWithToken(token.id, token.key);
                    }

                    $scope.State = "login";
                });
            });
            this.helper.client.tokenLoginFailed = () => {
                $timeout(() => {
                    //$scope.LoginModal = $Modal.open($scope.LoginModalOptions);
                });
            }
            $scope.exitChannel = (channelid) => {
                this.confirm("Er du sikker på at du vil lukke kanalen?", "Bekræftelse nødvendig", () => {
                    that.exitChannel(channelid);
                });
            };
            $scope.CreateNewChannel = (channelName) => {
                if (isNaN(Number(channelName))) {
                    this.createNewChannel(channelName);
                } else {
                    this.joinChannel(Number(channelName));
                }
                $scope.newChannelName = "";
                console.log($scope.newChannelName);
            };
            $scope.RequestHelp = () => {
                var qt: string = $scope.Channels[$scope.ActiveChannel].Text;
                this.requestHelp(qt, $scope.ActiveChannel);
                $scope.Channels[$scope.ActiveChannel].Text = "";
            };
            $scope.RemoveQuestion = (questionid) => {
                this.removeQuestion(questionid);
            };
            $scope.RemoveOwnQuestion = () => {
                this.removeOwnQuestion($scope.ActiveChannel);
            };
            $scope.EditOwnQuestion = () => {
                this.editOwnQuestion($scope.ActiveChannel);
            };
            this.helper.client.setQuestionState = (hasQuestion, channelid) => {
                $timeout(() => {
                    if ($scope.Channels[channelid] != null) $scope.Channels[channelid].HaveQuestion = hasQuestion;
                });
            };
            this.helper.client.updateUsername = (name) => {
                $timeout(() => {
                    $scope.Me.Name = name;
                }, 0);
            };
            this.helper.client.sendUserId = (id) => {
                $timeout(() => {
                    $scope.Me.Id = id;
                }, 0);
            }
            this.helper.client.appendChannel = (channel) => {
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
                $timeout(() => {
                    $scope.ActiveChannel = channel.Id;
                    $scope.Channels[channel.Id] = channel;
                    if (channel.TimeLeft) {
                        $scope.startTime = channel.TimeLeft;
                        $scope.StartTimer(channel);
                        $scope.startTime = 300;
                    }
                });
            };
            this.helper.client.exitChannel = (channelId) => {
                $timeout(() => {
                    delete $scope.Channels[channelId];
                    if ($scope.ActiveChannel === channelId) {
                        if ($scope.Channels[$scope.lastActiveChannel] != null) {
                            $scope.ActiveChannel = $scope.lastActiveChannel;
                        } else {
                            if (Object.keys($scope.Channels).length > 0) {
                                $scope.ActiveChannel = Number(Object.keys($scope.Channels)[0]);
                            } else {
                                $scope.ActiveChannel = 0;
                                $scope.lastActiveChannel = 0;
                            }
                        }
                    }
                });
            };
            this.helper.client.addQuestion = (question, channelid) => {
                if ($scope.Channels[channelid].timing) {
                    if (Object.keys($scope.Channels[channelid].Questions).length === 0) {
                        $scope.StartTimer($scope.Channels[channelid]);
                    }
                }
                question.User = $scope.Channels[channelid].Users[question.User.Id];
                $timeout(() => {
                    $scope.Channels[channelid].Questions[question.Id] = question;
                    $timeout(() => {
                        var c: any = $(".collapsible");
                        c.collapsible();
                    }, 50);
                });
                if ($scope.Channels[channelid].IsAdmin) {
                    if (document.hidden) {
                        this.alert("info", question.User.Name + " har brug for hjælp." + (question.Text ? `
Til spørgsmålet er teksten: "${question.Text}"` : ""), "Nyt spørgsmål");
                    }
                }

            };
            this.helper.client.removeQuestion = (questionid: number) => {
                $timeout(() => {
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
            this.helper.client.sendQuestion = (questionText) => {
                $timeout(() => {
                    $scope.editQuestionText.text = questionText;
                    //$scope.changeQuestionModal = $Modal.open($scope.changeQuestionModalOptions);
                });
            };
            $scope.UpdateQuestion = () => {
                this.changeQuestion($scope.editQuestionText.text, $scope.ActiveChannel);
                $scope.changeQuestionModal.close();
            };
            this.helper.client.updateQuestion = (questionText, questionid, channelid) => {
                $timeout(() => {
                    if ($scope.Channels[channelid] != null) {
                        if ($scope.Channels[channelid].Questions[questionid] != null) {
                            $scope.Channels[channelid].Questions[questionid].Text = questionText;
                        }
                    }
                });
            };
            $scope.CloseEditModal = () => {
                $scope.changeQuestionModal.close();
            };
            this.helper.client.appendUser = (user, channelid) => {
                if ($scope.Channels[channelid] != null) {
                    $timeout(() => {
                        $scope.Channels[channelid].Users[user.Id] = user;
                    });
                }
            }
            this.helper.client.removeUser = (userid, channelid) => {
                if ($scope.Channels[channelid] != null) {
                    if ($scope.Channels[channelid].Users[userid] != null) {
                        $timeout(() => {
                            delete $scope.Channels[channelid].Users[userid];
                        });
                    }
                }
            }
            $scope.RemoveUser = (userid) => {
                this.removeUserFromChannel(userid, $scope.ActiveChannel);
            }
            $scope.RemoveChatMessage = (messageId) => {
                this.removeChatMessage(messageId);
            }
            this.helper.client.removeChatMessage = (messageId: number) => {
                $timeout(() => {
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
            }
            $scope.Chat = () => {
                if ($scope.ActiveChannel) {
                    var mes = $scope.Channels[$scope.ActiveChannel].MessageText;
                    if (mes) {
                        this.chat(mes, $scope.ActiveChannel);
                    }
                    $scope.Channels[$scope.ActiveChannel].MessageText = "";
                } else {
                    this.alert("error", "Du er ikke i en kanal, og kan derfor ikke chatte med noget", "");
                }
            }
            this.helper.client.sendChatMessage = (message: ChatMessage, channelId) => {
                if (message.Text.toLowerCase().indexOf($scope.Me.Name.toLowerCase()) !== -1) {
                    if (message.User.Id !== $scope.Me.Id) {
                        if (channelId !== $scope.ActiveChannel || document.hidden) {
                            this.alert("info", message.Text, $scope.Channels[channelId].ChannelName);
                        }
                    }
                }
                $timeout(() => {
                    message.User = $scope.Channels[channelId].Users[message.User.Id];
                    $scope.Channels[channelId].ChatMessages[message.Id] = message;
                });
            }
            this.helper.client.alert = (message, heading, oftype) => {
                this.alert(oftype, message, heading);
            }
            $scope.createUser = () => {
                if ($scope.StartingModal.Password !== $scope.StartingModal.Passwordcopy) {
                    this.alert("error", "Kodeord stemmer ikke overens", "Problem med kodeord");
                    return;
                }
                if (!$scope.StartingModal.Name || !$scope.StartingModal.Email) {
                    this.alert("error", "Du har felter der endnu ikke er udfyldte", "Mangelende information");
                    return;
                }
                var email = $scope.StartingModal.Email;
                var pass = $scope.StartingModal.Password;
                var name = $scope.StartingModal.Name;
                // Simple checks to see if this is an email
                if (email.indexOf("@") === 0 || email.indexOf("@") === email.length - 1 || email.indexOf(".") === 0 || email.indexOf(".") === email.length - 1) {
                    return;
                }
                this.createNewUser(name, email, pass, $scope.StartingModal.StayLoggedIn);
            }
            this.helper.client.userCreationSuccess = () => {
                $("#createUserBtn").click();
                setTimeout(() => {
                    $scope.Me.LoggedIn = true;
                    $scope.StartingModal.Passwordcopy = "";
                    $scope.StartingModal.Password = "";
                    $scope.$apply();
                }, 1000);
                this.alert("success", "Din bruger er nu oprettet", "Oprettelse lykkedes");
            }
            $scope.logout = () => {
                var token: LoginToken = $cookieStore.get("token");
                if (!token)
                    this.logoutUser(null);
                else
                    this.logoutUser(token.key);
            }
            this.helper.client.userLoggedOut = () => {
                $timeout(() => {
                    $scope.Me.LoggedIn = false;
                    for (var ch in $scope.Channels) {
                        if ($scope.Channels.hasOwnProperty(ch)) {
                            delete $scope.Channels[ch];
                        }
                    }
                    $scope.setActiveChannel(0);
                    $cookieStore.remove("token");
                });
            }
            $scope.login = () => {
                if (!$scope.StartingModal.Email || !$scope.StartingModal.Password) {
                    this.alert("error", "Manglende info", "Manglende info");
                    return;
                }
                this.loginUser($scope.StartingModal.Email, $scope.StartingModal.Password, $scope.StartingModal.StayLoggedIn);
            }
            function loginClear() {
                $scope.Me.LoggedIn = true;
                $scope.StartingModal.Passwordcopy = "";
                $scope.StartingModal.Password = "";
                $scope.$apply();
            }
            this.helper.client.loginSuccess = () => {
                if ($scope.LoginModal) {
                    $scope.LoginModal.close();
                    $scope.LoginModal = null;
                    loginClear();
                } else {
                    $("#loginBtn").click();
                    setTimeout(loginClear(), 1000);
                }
                this.alert("success", "Du er nu logget ind.", "Login successfuld");
            }
            this.helper.client.updateOtherUsername = (name, userid, channelid) => {
                $timeout(() => {
                    $scope.Channels[channelid].Users[userid].Name = name;
                });
            }
            this.helper.client.setAdminState = (channelId, isAdmin) => {
                $timeout(() => {
                    $scope.Channels[channelId].IsAdmin = isAdmin;
                });
            }
            $scope.ClearChat = () => {
                this.confirm("Er du sikker på at du vil ryde chatten?", "Bekræftelse nødvendigt", () => {
                    this.clearChat($scope.ActiveChannel);
                });
            }
            this.helper.client.clearChat = (channelId: number) => {
                $timeout(() => {
                    var chatMessages = $scope.Channels[channelId].ChatMessages;
                    for (var chatMessageId in chatMessages) {
                        if (chatMessages.hasOwnProperty(chatMessageId)) {
                            delete chatMessages[chatMessageId];
                        }
                    }
                });
            }
            this.helper.client.clearChannels = () => {
                $timeout(() => {
                    for (var channelId in $scope.Channels) {
                        if ($scope.Channels.hasOwnProperty(channelId)) {
                            delete $scope.Channels[channelId];
                        }
                    }
                });
            }
            this.helper.client.sendReloginData = (key: string, id: number) => {
                $timeout(() => {
                    var token = new LoginToken(id, key);
                    $cookieStore.put("token", token, { expires: new Date(Date.now() + 1000 * 60 * 60 * 24 * 30) });
                    $scope.Me.LoggedIn = true;
                });
            }


            $scope.startPasswordReset = () => {
                $scope.pwReset.step = 1;
            }
            $scope.stopPasswordReset = () => {
                $scope.pwReset.step = 0;
            }

            $scope.RequestPasswordReset = () => {
                this.requestPasswordReset($scope.pwReset.email);
            }
            this.helper.client.passwordResetRequestResult = (success: boolean) => {
                $timeout(() => {
                    if (success) {
                        $scope.pwReset.invalidEmail = false;
                        $scope.pwReset.mailSent = true;
                    } else {
                        $scope.pwReset.invalidEmail = true;
                    }
                });
            }

            $scope.ResetPassword = () => {
                if (!$scope.pwReset.key.trim()) {
                    return;
                } 
                if ($scope.pwReset.pass1 !== $scope.pwReset.pass2) {
                    return;
                } 
                if ($scope.pwReset.pass1 && $scope.pwReset.pass1.length) {
                    if ($scope.pwReset.email) {
                        this.resetPassword($scope.pwReset.key, $scope.pwReset.pass1, $scope.pwReset.email);
                    } else {
                        $scope.pwReset.missingEmail = true;
                    }
                }
            }
            this.helper.client.passwordResetResult = (success: boolean) => {
                if (success) {
                    this.alert("success", "Dit kodeord er blevet nulstillet.", "Nulstiling lykkedes");
                    $scope.pwReset = {};
                } else {
                    $timeout(() => {
                        $scope.pwReset.resetFailed = true;
                    });
                }
            }

            $scope.ChangePassword = () => {
                if ($scope.pwReset.old) {
                    $scope.pwReset.oldEmpty = false;
                    if ($scope.pwReset.pass1 === $scope.pwReset.pass2) {
                        this.changePassword($scope.pwReset.old, $scope.pwReset.pass1);
                    }
                } else {
                    $scope.pwReset.oldEmpty = true;
                }
            }

            this.helper.client.passwordChanged = (success: boolean) => {
                if (success) {
                    this.alert("success", "Dit password are blevet ændret.", "");
                } else {
                    this.alert("error", "Kunne ikke skifte kodeord.", "");
                }
            }

            $scope.LogoutAll = () => {
                this.confirm("Are du sikker på at du vil logge din bruger ud alle stedet?", "Bekræftelse nødvendig.", () => {
                    this.logoutAll();
                });
            }

            this.helper.client.allUsersLoggedOut = () => {
                this.alert("success", "Din bruger er blevet logget ud alle andre steder.", "Log ud lykkedes");
            }

        }

    }

    app.controller("HelpCtrl", HelpCtrl);
    app.filter("keylength", () => input => {
        if (!angular.isObject(input)) {
            throw Error("Usage of non-objects with keylength filter!!");
        }
        return Object.keys(input).length;
    }).filter("toArray", () => obj => {
        if (!(obj instanceof Object)) {
            return obj;
        }

        return Object.keys(obj).map(key => Object.defineProperty(obj[key], "$key", <any>{ __proto__: null, value: key }));
    });;

    app.directive("wrapper", [

        () => {
            return {
                restrict: "C",
                link(scope, element) {

                    var innerElement = element.find("inner");

                    scope.$watch(
                        () => {
                            return innerElement[0].offsetHeight;
                        },
                        (value) => {
                            element.css("height", value + "px");
                        }, true);
                }
            };
        }
    ]);
}