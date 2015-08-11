/// <reference path="Scripts/typings/jquery/jquery.d.ts" />
/// <reference path="Scripts/typings/signalr/signalr.d.ts" />
/// <reference path="Scripts/typings/bootstrap/bootstrap.d.ts"/>
/// <reference path="Scripts/typings/jqueryui/jqueryui.d.ts"/>
/// <reference path="Scripts/typings/jquery.pnotify/jquery.pnotify.d.ts"/>
/// <reference path="scripts/typings/angularjs/angular.d.ts" />
/// <reference path="scripts/typings/angularjs/angular-animate.d.ts" />
/// <reference path="scripts/typings/angular-ui-bootstrap/angular-ui-bootstrap.d.ts" />
/// <reference path="scripts/typings/angularjs/angular-cookies.d.ts" />

interface SignalR {
    centralHub: ICentralHubProxy;
}

interface ICentralHubProxy {
    client: ICentralClient;
    server: ICentralServer;
}

interface ICentralClient {
    appendChannel: (channel: Help.Channel) => void;
    addQuestions: (questions: Help.Question[]) => void;
    addQuestion: (question: Help.Question, channelid: number) => void;
    userAreQuesting: () => void;
    removeQuestion: (questionId: number) => void;
    errorChannelAlreadyMade: () => void;
    log: (text: string) => void;
    exitChannel: (channelId: number) => void;
    setChannel: (channel: number, areUserQuestioning: boolean) => void;
    sendQuestion: (question: string) => void;
    updateQuestion: (question: string, questionId: number, channelId: number) => void;
    reloadPage: () => void;
    setQuestionState: (hasQuestion: boolean, channelid: number) => void;
    sendChatMessage: (message: Help.ChatMessage, channelId: number) => void;
    sendChatMessages: (text: string[], author: string[], messageId: number[], sender: boolean[], appendToLast: boolean[], canEdit: boolean[]) => void;
    checkVersion: (version: number) => void;
    removeChatMessage: (messageId: number) => void;
    ipDiscover: (channelIds: number[], channelNames: string[]) => void;
    clearChat: (channelId: number) => void;
    loginSuccess: () => void;
    loginFailed: () => void;
    showChannels: (channelIds: number[], channelNames: string[]) => void;
    userCreationFailed: (errorMessage: string) => void;
    userCreationSuccess: () => void;
    userLoggedOut: () => void;
    appendChannel2: (channelname: string, channelid: number) => void;
    updateUsername: (name: string) => void;
    updateQuestionAuthorName: (name: string, id: number) => void;
    updateChatMessageAuthorName: (name: string, ids: number[]) => void;
    errorChat: (errorMessage: string) => void;
    appendUsers(usernames: string[], userids: number[], isAdmin: boolean[]);
    appendUser(user: Help.User, channelid: number);
    removeUser(id: number, channelId: number);
    alert: (message: string, title: string, t: string) => void;
    sendUserId: (id: number) => void;
    updateOtherUsername: (name: string, userid: number, channelid: number) => void;
    setAdminState: (channelId: number, isAdmin: boolean) => void;
    clearChannels: () => void;
    sendReloginData: (key: string, id: number) => void;
    tokenLoginFailed: () => void;
}

interface ICentralServer {
    send(action: string, parameters: string): JQueryPromise<void>;
    getData(action: number): JQueryPromise<void>;
    setUsername(name: string): JQueryPromise<void>;
    createNewChannel(channelName: string): JQueryPromise<void>;
    loadNearbyChannels(): JQueryPromise<void>;
    exitChannel(channelId: number): JQueryPromise<void>;
    joinChannel(channelId: number): JQueryPromise<void>;
    removeQuestion(channelId: number): JQueryPromise<void>;
    removeChatMessage(messageId: number): JQueryPromise<void>;
    requestHelp(question: string, channelid: number): JQueryPromise<void>;
    changeQuestion(question: string, channelid: number): JQueryPromise<void>;
    chat(message: string, channelid: number): JQueryPromise<void>;
    clearChat(channelId: number): JQueryPromise<void>;
    createNewUser(username: string, email: string, password: string, stay: boolean): JQueryPromise<void>;
    requestActiveChannel(): JQueryPromise<void>;
    loginUser(mail: string, pass: string, stay: boolean): JQueryPromise<void>;
    logoutUser(key: string): JQueryPromise<void>;
    removeUserFromChannel(userid: number, channelid: number): JQueryPromise<void>;
    removeOwnQuestion(channelid: number): JQueryPromise<void>;
    editOwnQuestion(channelId: number): JQueryPromise<void>;
    loginWithToken(id: number, key: string): JQueryPromise<void>;
    sendCountdownTime(time: number, channelid: number): JQueryPromise<void>;
}

function isNullOrWhitespace(input: any) {
    if (typeof input === "undefined" || input == null) return true;
    return input.replace(/\s/g, "").length < 1;
}
var patt = /[\w][\wæøåöäÆØÅÖÄ ]+[\w]/;
function removeFromArray(arr: any, index: any) {
    return arr.slice(0, index).concat(arr.slice(index + 1));
}

var confirmNotice: any = null;

module Help {
    import ModalServiceInstance = angular.ui.bootstrap.IModalServiceInstance;
    import ModalService = angular.ui.bootstrap.IModalService;
    import ModalSettings = angular.ui.bootstrap.IModalSettings;
    var app = angular.module("Help", ["ui.bootstrap", "ngAnimate", "ngCookies"]);

    export interface IHelpScope extends ng.IScope {
        /**
         * The local user.
         */
        Me: Me;
        /**
         * The channels the local user are in.
         */
        Channels: { [id: number]: Channel };
        /**
         * Indicates if the signalr connection is ready.
         */
        Loading: boolean;
        /**
         * Indicates if the user has choosen a username yet.
         */
        Ready: boolean;

        /**
         * The info from the first modal
         */
        StartingModal: LoginOptions;
        /**
         * Call to have the user login from the modal
         * @returns {} 
         */
        LoginFromModal: () => void;
        /**
         * The instance of the first modal
         */
        LoginModal: ModalServiceInstance;
        /**
         * The configuration options for the first modal
         */
        LoginModalOptions: ModalSettings;
        /**
         * Starts the application with the selected username
         * @returns {} 
         */
        Start: () => void;

        /**
         * The currently active channel id
         */
        ActiveChannel: number;

        /**
         * The result of IP-discover
         */
        DiscoveredIps: Channel[];
        /**
         * Finds a user with a specific ID
         * @param id The id of the user to find
         * @returns {} 
         */
        GetUser: (id: number) => User;
        CreateNewChannel: (channelName: string) => void;
        setActiveChannel: (channelid: number) => void;
        exitChannel: (channelid: number) => void;
        RequestHelp: () => void;
        RemoveQuestion: (questionid: number) => void;
        RemoveOwnQuestion: () => void;
        EditOwnQuestion: () => void;
        changeQuestionModalOptions: ModalSettings;
        changeQuestionModal: ModalServiceInstance;
        UpdateQuestion: () => void;
        CloseEditModal: () => void;
        editQuestionText: { text: string };
        RemoveUser: (userid: number) => void;
        newChannelName: string;
        RemoveChatMessage: (messageId: number) => void;
        Chat: () => void;
        createUserPopover: PopoverOptions;
        createUser: () => void;
        logout: () => void;
        loginUserPopover: PopoverOptions;
        login: () => void;
        lastActiveChannel: number;
        changeUsernamePopover: PopoverOptions;
        ClearChat: () => void;
        StartTimer: (channel: Channel) => void;
        countDown: (channel: Channel) => void;
        StopTimer: (channel: Channel) => void;
        alarm: HTMLAudioElement;
        HaltTimer: (channel: Channel) => void;
        EditTimer: () => void;
        startTime: number;
    }

    export class ServerActions {
        helper: ICentralHubProxy;
        confirmNotice: PNotify;

        send(action: string, parameters: string): JQueryPromise<void> {
            return this.helper.server.send(action, parameters);
        }

        getData(action: number): JQueryPromise<void> {
            return this.helper.server.getData(action);
        }

        setUsername(name: string): JQueryPromise<void> {
            return this.helper.server.setUsername(name);
        }

        createNewChannel(channelName: string): JQueryPromise<void> {
            return this.helper.server.createNewChannel(channelName);
        }

        loadHearbyChannels(): JQueryPromise<void> {
            return this.helper.server.loadNearbyChannels();
        }

        exitChannel(channelId): JQueryPromise<void> {
            return this.helper.server.exitChannel(channelId);
        }

        joinChannel(channelId: number): JQueryPromise<void> {
            return this.helper.server.joinChannel(channelId);
        }

        removeQuestion(channelId: number): JQueryPromise<void> {
            return this.helper.server.removeQuestion(channelId);
        }

        removeChatMessage(messageId: number): JQueryPromise<void> {
            return this.helper.server.removeChatMessage(messageId);
        }

        chat(message: string, channelid: number): JQueryPromise<void> {
            return this.helper.server.chat(message, channelid);
        }

        clearChat(channelId: number): JQueryPromise<void> {
            return this.helper.server.clearChat(channelId);
        }

        createNewUser(username: string, email: string, password: string, stay: boolean): JQueryPromise<void> {
            return this.helper.server.createNewUser(username, email, password, stay);
        }

        requestActiveChannel(): JQueryPromise<void> {
            return this.helper.server.requestActiveChannel();
        }

        requestHelp(question: string, channelid: number): JQueryPromise<void> {
            return this.helper.server.requestHelp(question, channelid);
        }

        loginUser(mail: string, pass: string, stay: boolean): JQueryPromise<void> {
            return this.helper.server.loginUser(mail, pass, stay);
        }

        logoutUser(key: string): JQueryPromise<void> {
            return this.helper.server.logoutUser(key);
        }

        removeUserFromChannel(id: number, channelid: number): JQueryPromise<void> {
            return this.helper.server.removeUserFromChannel(id, channelid);
        }

        removeOwnQuestion(channelid: number): JQueryPromise<void> {
            return this.helper.server.removeOwnQuestion(channelid);
        }

        editOwnQuestion(channelId: number): JQueryPromise<void> {
            return this.helper.server.editOwnQuestion(channelId);
        }

        changeQuestion(questionText: string, channelId: number): JQueryPromise<void> {
            return this.helper.server.changeQuestion(questionText, channelId);
        }
        loginWithToken(id: number, key: string) {
            return this.helper.server.loginWithToken(id, key);
        }
        sendCountdownTime(time: number, channelid: number) {
            return this.helper.server.sendCountdownTime(time, channelid);
        }
        alert(typ: string, text: string, title: string) {
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
                }
            });
            notify.elem.click(() => {
                notify.remove();
            });
        }

        confirm(text: string, title: string, callback: Function) {
            if (confirmNotice == null)
                confirmNotice = new PNotify(<any>{
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
                                click(n) {
                                    n.remove();
                                    callback();
                                    confirmNotice = null;
                                }
                            },
                            {
                                text: "Annuller",
                                click(n) {
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
        }
    }

    export class HelpCtrl extends ServerActions {

        static $inject = ["$scope", "$modal", "$timeout", "$cookieStore", "$interval"];

        constructor(public $scope: IHelpScope, public $Modal: ModalService, public $timeout: any, public $cookieStore: any, public $interval: any) {
            super();
            $scope.Loading = true;
            $scope.StartingModal = new LoginOptions();
            $scope.Me = new Me();
            $scope.Channels = {};
            $scope.ActiveChannel = 0;
            $scope.editQuestionText = { text: "" };
            $scope.lastActiveChannel = 0;
            $scope.startTime = 300;
            $scope.alarm = new Audio("alarm.mp3");

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
                    var channel = $scope.Channels[key];
                    if (channel.timing) {
                        this.sendCountdownTime(channel.TimeLeft, key);
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
            };
            $scope.Start = () => {
                var name = $scope.StartingModal.Name;
                name = name.replace(/[\s]+/g, " ");
                var n = name.match(patt);
                if (n.length > 0) {
                    this.setUsername(n[0]);
                    if ($scope.LoginModal) {
                        $scope.Ready = true;
                        $scope.LoginModal.close();
                        $scope.LoginModal = null;
                    } else {
                        $timeout(() => {
                            jQuery("#editUsername").click();
                        });
                    }

                }
            };
            $.connection.hub.start().done(() => {
                $timeout(() => {
                    var token: LoginToken = $cookieStore.get("token");
                    if (!token) {
                        $scope.LoginModal = $Modal.open($scope.LoginModalOptions);
                    } else {
                        this.loginWithToken(token.id, token.key);
                    }

                    $scope.Loading = false;
                });
            });
            this.helper.client.tokenLoginFailed = () => {
                $timeout(() => {
                    $scope.LoginModal = $Modal.open($scope.LoginModalOptions);
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
            };
            $scope.RequestHelp = () => {
                var qt: string = $scope.Channels[$scope.ActiveChannel].Text;
                this.requestHelp(qt, $scope.ActiveChannel);
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
                MathJax.Hub.Queue(["Typeset", MathJax.Hub]);
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
                });
                MathJax.Hub.Queue(["Typeset", MathJax.Hub]);
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
                    $scope.changeQuestionModal = $Modal.open($scope.changeQuestionModalOptions);
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
                MathJax.Hub.Queue(["Typeset", MathJax.Hub]);
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
                var mes = $scope.Channels[$scope.ActiveChannel].MessageText;
                if (mes) {
                    this.chat(mes, $scope.ActiveChannel);
                }
                $scope.Channels[$scope.ActiveChannel].MessageText = "";
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

        return Object.keys(obj).map(key => Object.defineProperty(obj[key], "$key", { __proto__: null, value: key }));
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

    export enum QuestionState {
        HaveQuestion,
        NoQuestion
    }

    export class Question {
        Id: number;
        User: User;
        Text: string;
        Class: string;

        constructor(id: number, user: User, questionText: string) {
            this.Id = id;
            this.User = user;
            this.Text = questionText;
        }
    }

    export class User {
        Name: string;
        Id: number;

        constructor(id: number, name: string) {
            this.Id = id;
            this.Name = name;
        }
    }

    export class Me {
        Name: string;
        LoggedIn: boolean;
        Id: number;

        constructor() {
            this.Name = null;
            this.LoggedIn = false;
        }
    }

    export class Channel {
        Id: number;
        ChatMessages: { [id: number]: ChatMessage };
        Questions: { [id: number]: Question };
        ChannelName: string;
        Users: { [id: number]: User };
        QuestionState: QuestionState;
        HaveQuestion: boolean;
        IsAdmin: boolean;
        Text: string;
        MessageText: string;
        counting = false;
        outOfTime = false;
        timing = false;
        TimeLeft = 300;
        intervalCont: any;

        constructor(id: number, channelName: string) {
            this.Id = id;
            this.ChannelName = channelName;
        }
    }

    export class ChatMessage {
        Id: number;
        Text: string;
        User: User;
    }

    export class LoginOptions {
        Name: string;
        Email: string;
        Password: string;
        Passwordcopy: string;
        StayLoggedIn: boolean;

        constructor() {
            this.Name = "";
            this.Email = "";
            this.Password = "";
            this.Passwordcopy = "";
        }
    }

    export class LoginToken {
        id: number;
        key: string;

        constructor(i, k) {
            this.id = i;
            this.key = k;
        }
    }
}