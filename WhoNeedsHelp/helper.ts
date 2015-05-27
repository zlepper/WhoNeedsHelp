﻿/// <reference path="Scripts/typings/jquery/jquery.d.ts" />
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
    appendChannel: (channel: Help.Channel) => void;
    addQuestions: (questions: Help.Question[]) => void;
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
    exitChannel(channelId: number): JQueryPromise<void>;
    joinChannel(channelId: number): JQueryPromise<void>;
    removeQuestion(channelId: number): JQueryPromise<void>;
    removeChatMessage(messageId: number): JQueryPromise<void>;
    requestHelp(question: string, channelid: number): JQueryPromise<void>;
    changeQuestion(question: string, channelid: number): JQueryPromise<void>;
    chat(message: string, channelid: number): JQueryPromise<void>;
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
        Me: Me;
        /**
         * The channels the local user are in.
         */
        Channels: {[id: number]: Channel};
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
        setActiveChannel: (channelid: any) => void;
        exitChannel: (channelid: any) => void;
    }

    export class ServerActions {
        helper: ICentralHubProxy;

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

        joinChannel(channelId): JQueryPromise<void> {
            return this.helper.server.joinChannel(channelId);
        }

        removeQuestion(channelId): JQueryPromise<void> {
            return this.helper.server.removeQuestion(channelId);
        }

        removeChatMessage(messageId: number): JQueryPromise<void> {
            return this.helper.server.removeChatMessage(messageId);
        }

        chat(message: string, channelid: number): JQueryPromise<void> {
            return this.helper.server.chat(message, channelid);
        }

        clearChat(): JQueryPromise<void> {
            return this.helper.server.clearChat();
        }

        createNewUser(username: string, email: string, password: string): JQueryPromise<void> {
            return this.helper.server.createNewUser(username, email, password);
        }

        requestActiveChannel(): JQueryPromise<void> {
            return this.helper.server.requestActiveChannel();
        }

        loginUser(mail: string, pass: string): JQueryPromise<void> {
            return this.helper.server.loginUser(mail, pass);
        }

        logoutUser(): JQueryPromise<void> {
            return this.helper.server.logoutUser();
        }

        removeUserFromChannel(id: string): JQueryPromise<void> {
            return this.helper.server.removeUserFromChannel(id);
        }
    }

    export class HelpCtrl extends ServerActions {

        static $inject = ["$scope", "$modal"];

        constructor(public $scope: IHelpScope, public $Modal: ModalService) {
            super();
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
            }

            $scope.setActiveChannel = (channelid) => {
                $scope.ActiveChannel = channelid;
            }

            $scope.Start = () => {
                var name = $scope.StartingModal.Name;
                name = name.replace(/[\s]+/g, " ");
                var n = name.match(patt);
                if (n.length > 0) {
                    this.setUsername(n[0]);
                    console.log(n[0]);
                }
                $scope.Ready = true;
                $scope.LoginModal.close();
            }

            $.connection.hub.start().done(() => {
                $scope.Loading = false;
                console.log($scope.LoginModalOptions);
                $scope.LoginModal = $Modal.open($scope.LoginModalOptions);
            });

            $scope.exitChannel = (channelid) => {
                console.log(typeof (channelid));
                this.exitChannel(channelid);
            }

            $scope.CreateNewChannel = (channelName) => {
                if (isNaN(Number(channelName))) {
                    this.createNewChannel(channelName);
                } else {
                    this.joinChannel(Number(channelName));
                }
            }

            this.helper.client.updateUsername = (name) => {
                $scope.Me.Name = name;
                $scope.$apply();
            }

            this.helper.client.appendChannel = (channel) => {
                $scope.ActiveChannel = channel.Id;
                $scope.Channels[channel.Id] = channel;
                //$scope.Channels.push(channel);
                $scope.$apply();
                console.log(channel);
            }

        }


    }

    app.controller("HelpCtrl", HelpCtrl);
    app.filter("keylength", () => input => {
        if (!angular.isObject(input)) {
            throw Error("Usage of non-objects with keylength filter!!");
        }
        return Object.keys(input).length;
    });

    export enum QuestionState {
        HaveQuestion,
        NoQuestion
    }

    export class Question {
        Id: number;
        User: User;
        QuestionText: string;

        constructor(id: number, user: User, questionText: string) {
            this.Id = id;
            this.User = user;
            this.QuestionText = questionText;
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

        constructor() {
            this.Name = null;
            this.LoggedIn = false;
        }
    }

    export class Channel {
        Id: number;
        ChatMessages: {[id: number]: ChatMessage};
        Questions: { [id: number]: Question };
        ChannelName: string;
        Users: { [id: number]: User };
        QuestionState: QuestionState;
        HaveQuestion: boolean;
        IsAdmin: boolean;

        constructor(id: number, channelName: string) {
            this.Id = id;
            this.ChannelName = channelName;
        }
    }

    export class ChatMessage {
        Text: string;
        Author: User;
    }

    export class LoginOptions {
        Name: string;
        Email: string;
        Password: string;

        constructor() {
            this.Name = "";
            this.Email = "";
            this.Password = "";
        }
    }
}