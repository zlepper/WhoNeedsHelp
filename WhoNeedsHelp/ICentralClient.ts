/// <reference path="Scripts/typings/jquery/jquery.d.ts" />
/// <reference path="Scripts/typings/signalr/signalr.d.ts" />
/// <reference path="Scripts/typings/bootstrap/bootstrap.d.ts"/>
/// <reference path="Scripts/typings/jqueryui/jqueryui.d.ts"/>
/// <reference path="Scripts/typings/jquery.pnotify/jquery.pnotify.d.ts"/>
/// <reference path="scripts/typings/angularjs/angular.d.ts" />
/// <reference path="scripts/typings/angularjs/angular-animate.d.ts" />
/// <reference path="scripts/typings/angular-ui-bootstrap/angular-ui-bootstrap.d.ts" />
/// <reference path="scripts/typings/angularjs/angular-cookies.d.ts" />

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
    passwordResetRequestResult: (success: boolean) => void;
    passwordResetResult: (success: boolean) => void;
    passwordChanged: (success: boolean) => void;
    allUsersLoggedOut: () => void;
}