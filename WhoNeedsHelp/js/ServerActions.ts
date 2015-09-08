/// <reference path="../Scripts/typings/jquery/jquery.d.ts" />
/// <reference path="../Scripts/typings/signalr/signalr.d.ts" />
/// <reference path="../Scripts/typings/jqueryui/jqueryui.d.ts"/>
/// <reference path="../Scripts/typings/jquery.pnotify/jquery.pnotify.d.ts"/>
/// <reference path="../scripts/typings/angularjs/angular.d.ts" />
/// <reference path="../scripts/typings/angularjs/angular-animate.d.ts" />
/// <reference path="../scripts/typings/angular-ui-bootstrap/angular-ui-bootstrap.d.ts" />
/// <reference path="../scripts/typings/angularjs/angular-cookies.d.ts" />

module Help {
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
        requestPasswordReset(email: string) {
            return this.helper.server.requestPasswordReset(email);
        }
        resetPassword(key: string, pass: string, email: string) {
            return this.helper.server.resetPassword(key, pass, email);
        }
        changePassword(oldpass: string, newpass: string) {
            return this.helper.server.changePassword(oldpass, newpass);
        }
        logoutAll() {
            return this.helper.server.logoutAll();
        }
        syncChannels(chs: Object) {
            return this.helper.server.syncChannels(chs);
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
}