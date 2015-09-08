/// <reference path="../Scripts/typings/jquery/jquery.d.ts" />
/// <reference path="../Scripts/typings/signalr/signalr.d.ts" />
/// <reference path="../Scripts/typings/jqueryui/jqueryui.d.ts"/>
/// <reference path="../Scripts/typings/jquery.pnotify/jquery.pnotify.d.ts"/>
/// <reference path="../scripts/typings/angularjs/angular.d.ts" />
/// <reference path="../scripts/typings/angularjs/angular-animate.d.ts" />
/// <reference path="../scripts/typings/angular-ui-bootstrap/angular-ui-bootstrap.d.ts" />
/// <reference path="../scripts/typings/angularjs/angular-cookies.d.ts" />

module Help {
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
        LoginModal: angular.ui.bootstrap.IModalServiceInstance;
        /**
         * The configuration options for the first modal
         */
        LoginModalOptions: angular.ui.bootstrap.IModalSettings;
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
        changeQuestionModalOptions: angular.ui.bootstrap.IModalSettings;
        changeQuestionModal: angular.ui.bootstrap.IModalServiceInstance;
        UpdateQuestion: () => void;
        CloseEditModal: () => void;
        editQuestionText: { text: string };
        RemoveUser: (userid: number) => void;
        newChannelName: string;
        RemoveChatMessage: (messageId: number) => void;
        Chat: () => void;
        createUserPopover: any;
        createUser: () => void;
        logout: () => void;
        loginUserPopover: any;
        login: () => void;
        lastActiveChannel: number;
        changeUsernamePopover: any;
        ClearChat: () => void;
        StartTimer: (channel: Channel) => void;
        countDown: (channel: Channel) => void;
        StopTimer: (channel: Channel) => void;
        alarm: HTMLAudioElement;
        HaltTimer: (channel: Channel) => void;
        EditTimer: () => void;
        startTime: number;
        pwReset: any;
        startPasswordReset: () => void;
        stopPasswordReset: () => void;
        RequestPasswordReset: () => void;
        ResetPassword: () => void;
        changePasswordPopover: any;
        ChangePassword: () => void;
        LogoutAll: () => void;
        State: string;
    }
}