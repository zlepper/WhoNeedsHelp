/// <reference path="Scripts/typings/jquery/jquery.d.ts" />
/// <reference path="Scripts/typings/signalr/signalr.d.ts" />
/// <reference path="Scripts/typings/bootstrap/bootstrap.d.ts"/>
/// <reference path="Scripts/typings/jqueryui/jqueryui.d.ts"/>
/// <reference path="Scripts/typings/jquery.pnotify/jquery.pnotify.d.ts"/>
/// <reference path="scripts/typings/angularjs/angular.d.ts" />
/// <reference path="scripts/typings/angularjs/angular-animate.d.ts" />
/// <reference path="scripts/typings/angular-ui-bootstrap/angular-ui-bootstrap.d.ts" />

interface SignalR {
    centralHub: ICentralHubProxy;
}

interface ICentralHubProxy {
    client: ICentralClient;
    server: ICentralServer;
}

interface ICentralClient {
    appendChannel: (channelname: string, channelid: number) => void;
    addQuestions: (usernames: string[], questions: string[], questionsIds: number[]) => void;
    addQuestion: (username: string, question: string, questionId: string) => void;
    userAreQuesting: () => void;
    removeQuestion: (questionId: number) => void;
    errorChannelAlreadyMade: () => void;
    log: (text: string) => void;
    exitChannel: (channelId: number) => void;
    setChannel: (channel: number, areUserQuestioning: boolean) => void;
    sendQuestion: (question: string) => void;
    updateQuestion: (question: string, questionId: number, channelId: number) => void;
    reloadPage: () => void;
    setLayout: (layout: number) => void;
    sendChatMessage: (text: string, author: string, messageId: number, sender: boolean, appendToLast: boolean, canEdit: boolean) => void;
    sendChatMessages: (text: string[], author: string[], messageId: number[], sender: boolean[], appendToLast: boolean[], canEdit: boolean[]) => void;
    checkVersion: (version: number) => void;
    removeChatMessage: (messageId: number) => void;
    ipDiscover: (channelIds: number[], channelNames: string[]) => void;
    clearChat: () => void;
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
    appendUser(username: string, id: number, isAdmin: boolean);
    removeUser(id: number, channelId: number);
    alert: (message: string, title: string, t: string) => void;
}

interface ICentralServer {
    send(action: string, parameters: string): JQueryPromise<void>;
    getData(action: number): JQueryPromise<void>;
    setUsername(name: string): JQueryPromise<void>;
    createNewChannel(channelName: string): JQueryPromise<void>;
    loadNearbyChannels(): JQueryPromise<void>;
    changeToChannel(channelId: string): JQueryPromise<void>;
    exitChannel(channelId: string): JQueryPromise<void>;
    joinChannel(channelId: string): JQueryPromise<void>;
    removeQuestion(channelId: string): JQueryPromise<void>;
    removeChatMessage(messageId: string): JQueryPromise<void>;
    requestHelp(channelId: string): JQueryPromise<void>;
    searchForChannel(channelId: string): JQueryPromise<void>;
    changeQuestion(question: string): JQueryPromise<void>;
    chat(message: string): JQueryPromise<void>;
    clearChat(): JQueryPromise<void>;
    createNewUser(username: string, email: string, password: string): JQueryPromise<void>;
    requestActiveChannel(): JQueryPromise<void>;
    loginUser(mail: string, pass: string): JQueryPromise<void>;
    logoutUser(): JQueryPromise<void>;
    removeUserFromChannel(tmpid: string): JQueryPromise<void>;
}

function isNullOrWhitespace(input: any) {
    if (typeof input === "undefined" || input == null) return true;
    return input.replace(/\s/g, "").length < 1;
}
var patt = /[\w][\wæøåöäÆØÅÖÄ ]+[\w]/;
function removeFromArray(arr: any, index: any) {
    return arr.slice(0, index).concat(arr.slice(index + 1));
}
module Help {
    import ModalServiceInstance = angular.ui.bootstrap.IModalServiceInstance;
    import ModalService = angular.ui.bootstrap.IModalService;
    import ModalSettings = angular.ui.bootstrap.IModalSettings;
    var app = angular.module("Help", ["ui.bootstrap"]);

    export interface IHelpScope extends ng.IScope {
        /**
         * The local user.
         */
        me: User;
        /**
         * The channels the local user are in.
         */
        channels: Channel[];
        /**
         * Indicates if the signalr connection is ready.
         */
        loading: boolean;
        /**
         * Indicates if the user has choosen a username yet.
         */
        ready: boolean;

        /**
         * The info from the first modal
         */
        startingModal: LoginOptions;
        /**
         * Call to have the user login from the modal
         * @returns {} 
         */
        loginFromModal: () => void;
        /**
         * The instance of the first modal
         */
        loginModal: ModalServiceInstance;
        /**
         * The configuration options for the first modal
         */
        loginModalOptions: ModalSettings;
        /**
         * Starts the application with the selected username
         * @returns {} 
         */
        start: () => void;

        /**
         * The currently active channel
         */
        activeChannel: Channel;

        /**
         * The result of IP-discover
         */
        discoveredIps: Channel[];
        /**
         * Finds a user with a specific ID
         * @param id The id of the user to find
         * @returns {} 
         */
        getUser: (id: number) => User;
    }

    export class HelpCtrl {
        private helper: ICentralHubProxy;

        static $inject = ["$scope", "$modal"];

        constructor(private $scope: IHelpScope, private $modal: ModalService) {
            $scope.loading = true;
            this.helper = $.connection.centralHub;
            var that = this;

            $.connection.hub.start().done(() => {
                $scope.loading = false;
                $scope.loginModal = $modal.open($scope.loginModalOptions);
            });

            $scope.loginFromModal = () => {
                if (isNullOrWhitespace($scope.startingModal.email) || isNullOrWhitespace($scope.startingModal.password))
                    return;
                that.loginUser($scope.startingModal.email, $scope.startingModal.password);
                $scope.startingModal.password = "";
            };

            $scope.start = () => {
                var name = $scope.startingModal.name;
                name = name.replace(/[\s]+/g, " ");
                var n = name.match(patt);
                if(n.length > 0)
                    this.setUsername(n[0]);
                $scope.ready = true;
            }

            $scope.loginModalOptions = {
                templateUrl: "startModal.html",
                controller: "HelpCtrl",
                keyboard: false,
                backdrop: false
            }

            this.helper.client.appendChannel = (channelname: string, channelId: number) => {
                var channel = new Channel(channelId, channelname);
                $scope.channels.push(channel);
                $scope.activeChannel = channel;
                $scope.$apply();
            }

            this.helper.client.appendChannel2 = (channelname: string, channelId: number) => {
                var channel = new Channel(channelId, channelname);
                $scope.channels.push(channel);
                $scope.$apply();
            }

            this.helper.client.setChannel = (channelId: number, areUserQuestioning) => {
                for (var i = 0; i < $scope.channels.length; i++) {
                    var channel = $scope.channels[0];
                    channel.currentlyActive = channel.id === channelId;
                    if (channel.currentlyActive) {
                        $scope.activeChannel = channel;
                    }
                }
                $scope.activeChannel.haveQuestion = areUserQuestioning;
            }

            this.helper.client.removeUser = (id: number, channelId: number) => {
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
            }

            this.helper.client.updateQuestion = (questionText: string, questionId: number, channelId: number) => {
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
            }

            this.helper.client.showChannels = (channelIds: number[], channelNames: string[]) => {
                $scope.channels.length = 0;
                for (var i = 0; i < channelIds.length; i++) {
                    var channel = new Channel(channelIds[i], channelNames[i]);
                    $scope.channels.push(channel);
                }
                this.requestActiveChannel();
            }

            this.helper.client.exitChannel = (channelId: number) => {
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
                } else {
                    $scope.activeChannel = null;
                }
            }

            this.helper.client.ipDiscover = (ids: number[], names: string[]) => {
                $scope.discoveredIps.length = 0;
                for (var i = 0; i < ids.length; i++) {
                    var channel = new Channel(ids[i], names[i]);
                    $scope.discoveredIps.push(channel);
                }
            }

            this.helper.client.addQuestions = (usernames: string[], questions: string[], questionIds: number[]) => {
                for (var i = 0; i < questions.length; i++) {
                    var question = new Question()
                }
            }


            $scope.getUser = (id) => User {
                
            }
        }

        private send(action: string, parameters: string): JQueryPromise<void> {
            return this.helper.server.send(action, parameters);
        }

        private getData(action: number): JQueryPromise<void> {
            return this.helper.server.getData(action);
        }

        private setUsername(name: string): JQueryPromise<void> {
            return this.helper.server.setUsername(name);
        }

        private createNewChannel(channelName: string): JQueryPromise<void> {
            return this.helper.server.createNewChannel(channelName);
        }

        private loadHearbyChannels(): JQueryPromise<void> {
            return this.helper.server.loadNearbyChannels();
        }

        private changeToChannel(channelId: string): JQueryPromise<void> {
            return this.helper.server.changeToChannel(channelId);
        }

        private exitChannel(channelId): JQueryPromise<void> {
            return this.helper.server.exitChannel(channelId);
        }

        private joinChannel(channelId): JQueryPromise<void> {
            return this.helper.server.joinChannel(channelId);
        }

        private removeQuestion(channelId): JQueryPromise<void> {
            return this.helper.server.removeQuestion(channelId);
        }

        private removeChatMessage(messageId: string): JQueryPromise<void> {
            return this.helper.server.removeChatMessage(messageId);
        }

        private searchForChannel(channelId: string): JQueryPromise<void> {
            return this.helper.server.searchForChannel(channelId);
        }

        private chat(message: string): JQueryPromise<void> {
            return this.helper.server.chat(message);
        }

        private clearChat(): JQueryPromise<void> {
            return this.helper.server.clearChat();
        }

        private createNewUser(username: string, email: string, password: string): JQueryPromise<void> {
            return this.helper.server.createNewUser(username, email, password);
        }

        private requestActiveChannel(): JQueryPromise<void> {
            return this.helper.server.requestActiveChannel();
        }

        private loginUser(mail: string, pass: string): JQueryPromise<void> {
            return this.helper.server.loginUser(mail, pass);
        }

        private logoutUser(): JQueryPromise<void> {
            return this.helper.server.logoutUser();
        }

        private removeUserFromChannel(id: string): JQueryPromise<void> {
            return this.helper.server.removeUserFromChannel(id);
        }
    }

    app.controller("HelpCtrl", HelpCtrl);

    export enum QuestionState {
        HaveQuestion,
        NoQuestion
    }

    export class Question {
        public id: number;
        public user: User;
        public questionText: string;

        constructor(id: number, user: User, questionText: string) {
            this.id = id;
            this.user = user;
            this.questionText = questionText;
        }
    }

    export class User {
        public name: string;
        public id: number;

        constructor(id: number, name: string) {
            this.id = id;
            this.name = name;
        }
    }

    export class Channel {
        public id: number;
        public chatMessages: ChatMessage[];
        public questions: Question[];
        public channelName: string;
        public users: User[];
        public questionState: QuestionState;
        public currentlyActive: boolean;
        public haveQuestion: boolean;
        public isAdmin: boolean;

        constructor(id: number, channelName: string) {
            this.id = id;
            this.channelName = channelName;
        }
    }

    export class ChatMessage {
        public text: string;
        public author: User;
    }

    export class LoginOptions {
        public name: string;
        public email: string;
        public password: string;
    }
}