interface ICentralServer {
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
    requestPasswordReset(email: string): JQueryPromise<void>;
    resetPassword(key: string, pass: string, email: string): JQueryPromise<void>;
    changePassword(oldpass: string, newpass: string): JQueryPromise<void>;
    logoutAll(): JQueryPromise<void>;
    syncChannels(chs: Object): JQueryPromise<void>;
    
    loginOrCreateUserWithApi(username: string, userid: string, password: string): JQueryPromise<void>;
    joinOrCreateChannelWithApi(channelname: string, channelid: string, teacherKey: string): JQueryPromise<void>;

    cleaupTime(alarmId: number): JQueryPromise<void>;
}