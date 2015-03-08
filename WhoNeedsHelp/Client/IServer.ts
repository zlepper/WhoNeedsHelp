﻿interface SignalR {
    centralHub: ICentralHubProxy;
}

interface ICentralHubProxy {
    client: ICentralClient;
    server: ICentralServer;
}

interface ICentralClient {
    appendChannel: (channelname: string, channelid: string) => void;
    addQuestions: (usernames: string[], questions: string[], questionsIds: string[], admin: boolean) => void;
    addQuestion: (username: string, question: string, questionId: string, admin: boolean) => void;
    userAreQuesting: () => void;
    removeQuestion: (questionId: string) => void;
    errorChannelAlreadyMade: () => void;
    log: (text: string) => void;
    exitChannel: (channelId: string) => void;
    channelsFound: (channelIds: string[], channelNames: string[]) => void;
    setChannel: (channel: string, areUserQuestioning: boolean) => void;
    updateChannelCount: (activeUsers: number, connectedUsers: number, channelId: string) => void;
    sendQuestion: (question: string) => void;
    updateQuestion: (question: string, questionId: string) => void;
    reloadPage: () => void;
    setLayout: (layout: number) => void;
    sendChatMessage: (text: string, author: string, messageId: string, sender: boolean, appendToLast: boolean, canEdit: boolean) => void;
    sendChatMessages: (text: string[], author: string[], messageId: string[], sender: boolean[], appendToLast: boolean[], canEdit: boolean[]) => void;
    checkVersion: (version: number) => void;
    removeChatMessage: (messageId: string) => void;
    ipDiscover: (channelIds: string[], channelNames: string[]) => void;
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
}