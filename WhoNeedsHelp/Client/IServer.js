//interface SignalR {
//    centralHub: ICentralHubProxy;
//}
//interface ICentralHubProxy {
//    client: ICentralClient;
//    server: ICentralServer;
//}
//interface ICentralClient {
//    appendChannel: (channelname: string, channelid: string) => void;
//    addQuestions: (usernames: string[], questions: string[], questionsIds: string[], admin: boolean) => void;
//    addQuestion: (username: string, question: string, questionId: string, admin: boolean) => void;
//    userAreQuesting: () => void;
//    removeQuestion: (questionId: string) => void;
//    errorChannelAlreadyMade: () => void;
//    log: (text: string) => void;
//    exitChannel: (channelId: string) => void;
//    //channelsFound: (channelIds: string[], channelNames: string[]) => void;
//    setChannel: (channel: string, areUserQuestioning: boolean) => void;
//    updateChannelCount: (activeUsers: number, connectedUsers: number, channelId: string) => void;
//    sendQuestion: (question: string) => void;
//    updateQuestion: (question: string, questionId: string) => void;
//    reloadPage: () => void;
//    setLayout: (layout: number) => void;
//    sendChatMessage: (text: string, author: string, messageId: string, sender: boolean, appendToLast: boolean, canEdit: boolean) => void;
//    sendChatMessages: (text: string[], author: string[], messageId: string[], sender: boolean[], appendToLast: boolean[], canEdit: boolean[]) => void;
//    checkVersion: (version: number) => void;
//    removeChatMessage: (messageId: string) => void;
//    ipDiscover: (channelIds: string[], channelNames: string[]) => void;
//    clearChat: () => void;
//    loginSuccess: () => void;
//    loginFailed: () => void;
//    showChannels: (channelIds: string[], channelNames: string[]) => void;
//    userCreationFailed: (errorMessage: string) => void;
//    userCreationSuccess: () => void;
//    userLoggedOut: () => void;
//    appendChannel2: (channelname: any, channelid: any) => void;
//    updateUsername: (name: string) => void;
//    updateQuestionAuthorName: (name: string, id: string) => void;
//    updateChatMessageAuthorName: (name: string, ids: string[]) => void;
//    errorChat: (errorMessage: string) => void;
//    appendUsers(usernames: string[], userids: number[], admin: boolean);
//    appendUser(username: string, id: number, admin: boolean);
//    removeUser(id: number);
//    alert: (message: string, title: string, t: string) => void;
//}
//interface ICentralServer {
//    send(action: string, parameters: string): JQueryPromise<void>;
//    getData(action: number): JQueryPromise<void>;
//    setUsername(name: string): JQueryPromise<void>;
//    createNewChannel(channelName: string): JQueryPromise<void>;
//    loadNearbyChannels(): JQueryPromise<void>;
//    changeToChannel(channelId: string): JQueryPromise<void>;
//    exitChannel(channelId: string): JQueryPromise<void>;
//    joinChannel(channelId: string): JQueryPromise<void>;
//    removeQuestion(channelId: string): JQueryPromise<void>;
//    removeChatMessage(messageId: string): JQueryPromise<void>;
//    requestHelp(channelId: string): JQueryPromise<void>;
//    searchForChannel(channelId: string): JQueryPromise<void>;
//    changeQuestion(question: string): JQueryPromise<void>;
//    chat(message: string): JQueryPromise<void>;
//    clearChat(): JQueryPromise<void>;
//    createNewUser(username: string, email: string, password: string): JQueryPromise<void>;
//    requestActiveChannel(): JQueryPromise<void>;
//    loginUser(mail: string, pass: string): JQueryPromise<void>;
//    logoutUser(): JQueryPromise<void>;
//    removeUserFromChannel(tmpid: string): JQueryPromise<void>;
//} 
//# sourceMappingURL=IServer.js.map