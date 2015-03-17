﻿/// <reference path="Scripts/typings/jquery/jquery.d.ts" />
/// <reference path="Scripts/typings/signalr/signalr.d.ts" />
/// <reference path="Client/IServer.ts"/>
/// <reference path="Scripts/typings/bootstrap/bootstrap.d.ts"/>
/// <reference path="Scripts/typings/jqueryui/jqueryui.d.ts"/>
/// <reference path="Scripts/typings/jquery.pnotify/jquery.pnotify.d.ts"/>

"use strict";
// The pattern for the username
var patt = /[\w][\wæøåöäÆØÅÖÄ ]+[\w]/;

// Make a connection to the correct hub
// In this case the CentralHub which handles this application.
var chat = $.connection.centralHub;
var fetchTables;
var createUserPopover: JQuery;
var loginUserPopover: JQuery;
var changeUsernamePopover: JQuery;
var firstName: Boolean;
firstName = false;
PNotify.prototype.options.styling = "bootstrap3";
//var validate;

    function setUserName() {
        var input = $("#usernameModalInput");
        var name = input.val();
        name = name.replace(/[\s]+/g, " ");
        var n = name.match(patt);
        chat.server.setUsername(n[0]);
        $("#CurrentUserName").html(n[0]);
        $("#usernameModal").modal("hide");
        var id = getUrlParameter("id");
        if (!isNullOrWhitespace(id)) {
            chat.server.joinChannel(id);
        }
        return false;
    }

    function getUrlParameter(sParam) {
        var sPageUrl = window.location.search.substring(1);
        var sUrlVariables = sPageUrl.split("&");
        for (var i = 0; i < sUrlVariables.length; i++) {
            var sParameterName = sUrlVariables[i].split("=");
            if (sParameterName[0] === sParam) {
                return sParameterName[1];
            }
        }
        return "";
    }

    function validate() {
        var t = $("#usernameModalInput").val();
        if (patt.test(t)) {
            $("#usernameGroup").addClass("has-success").removeClass("has-error");
        } else {
            $("#usernameGroup").addClass("has-error").removeClass("has-success");
        }
    }

    chat.client.log = text => {
        console.log(text);
    }

    chat.client.appendChannel = (channelname, channelid) => {
        var span1 = $("<span />").addClass("glyphicon glyphicon-remove close channel-remove");
        var span2 = $("<span />").addClass("badge").text("0/0");
        var html = $("<a />").attr("href", "#").attr("style", "display: none;").attr("id", channelid).addClass("list-group-item").text(channelname).prepend(span2).prepend(span1);
        $("#ChannelList").append(html);
        $("#ChannelList #" + channelid).show("slide", 400);
        chat.server.changeToChannel(channelid);
    }

    chat.client.appendChannel2 = (channelname, channelid) => {
        var span1 = $("<span />").addClass("glyphicon glyphicon-remove close channel-remove");
        var span2 = $("<span />").addClass("badge").text("0/0");
        var html = $("<a />").attr("href", "#").attr("style", "display: none;").attr("id", channelid).addClass("list-group-item").text(channelname).prepend(span2).prepend(span1);
        $("#ChannelList").append(html);
        $("#ChannelList #" + channelid).show("slide", 400);
    }

    chat.client.setChannel = (channel, areUserQuestioning) => {
        window.history.pushState({}, "Hvem behøver hjælp", "?id=" + channel);
        $("#CurrentChannelId").html(channel);
        $("div#ChannelList > a").stop().removeClass("active").attr("style", "");
        setTimeout(() => {
            $("#" + channel).addClass("active", 400);
            var t = $("#ChannelList #" + channel).text();
        }, 100);
        if (areUserQuestioning) {
            setQuestionLayout(3);
        } else {
            setQuestionLayout(1);
        }
    }

    chat.client.updateChannelCount = (activeUsers, connectedUsers, channelId) => {
        var badge = activeUsers + "/" + connectedUsers;
        $("a#" + channelId + " .badge").html(badge);
    }

    chat.client.updateQuestion = (question, questionId) => {
        var panel = $("#" + questionId + " .panel-body");
        if (question === "") {
            panel.hide("blind", function() {
                $(this).remove();
            });
        } else {
            if (panel.length > 0) {
                panel.html(question);
            } else {
                var html = $("<div />").attr("style", "display: none;").addClass("panel-body").text(question);
                $("#HelpList #" + questionId).append(html);
                MathJax.Hub.Queue(["Typeset", MathJax.Hub]);
                $("#HelpList #" + questionId + " .panel-body").show("blind");
            }
        }
    }

    chat.client.showChannels = (channelIds, channelNames) => {
        $("#ChannelList").empty();
        for (var i = 0; i < channelIds.length; i++) {
            var span1 = $("<span />").addClass("glyphicon glyphicon-remove close channel-remove");
            var span2 = $("<span />").addClass("badge").text("0/0");
            var html = $("<a />").attr("href", "#").attr("style", "display: none;").attr("id", channelIds[i]).addClass("list-group-item").text(channelNames[i]).prepend(span2).prepend(span1);
            $("#ChannelList").append(html);
            $("#ChannelList #" + channelIds[i]).show("slide", 400);
        }
        chat.server.requestActiveChannel();
    }

    chat.client.errorChannelAlreadyMade = () => {
        alert("This channel already exists");
    }

    chat.client.sendQuestion = question => {
        $("#newQuestionText").val(question);
    }

    chat.client.exitChannel = e => {
        var tmpid = $("#ChannelList #" + e);
        window.history.pushState({}, "Hvem behøver hjælp", "?");
        tmpid.hide("slide", {}, 400, () => {
            tmpid.remove();
            var id = $("#ChannelList a:first-child").attr("id");
            if (id == undefined) {
                setQuestionLayout(2);
                $(".chat").empty();
                $("#CurrentChannelId").html("Ikke forbundet til nogen kanal");
                $("#HelpList > div").each(function(index) {
                    $(this).delay(index * 300).hide("blind", {}, 400, function() {
                        $(this).remove();
                    });
                });
            } else {
                chat.server.changeToChannel(id);
            }
        });
    }

    chat.client.channelsFound = (ids, names) => {
        var resultList = $("#SearchChannelResults");

        for (var i = 0; i < ids.length; i++) {
            var html = $("<a />");
            html = html.attr("id", ids[i]).attr("class", "list-group-item").attr("href", "#").attr("data-toggle", "tooltip").attr("data-placement","left").attr("title", "Kanel ID: " + ids[i]).text(names[i]);
            resultList.append(html);
            $("[data-toggle=\"tooltip\"]").tooltip();
        }
    }

    chat.client.ipDiscover = (ids, names) => {
        var discoverElement = $("#ip-discovery");
        discoverElement.empty();

        for (var i = 0; i < ids.length; i++) {
            var html = $("<a />");
            html = html.attr("href", "#").attr("id", ids[i]).attr("class", "list-group-item").text(names[i]);
            discoverElement.append(html);
        }
    }

    chat.client.addQuestions = (usernames, questions, questionIds, admin) => {
        var helpList = $("#HelpList");
        helpList.empty();
        for (var i = 0; i < questionIds.length; i++) {
            var span: JQuery, button: JQuery = $();
            if (admin) {
                span = $("<span />").attr("aria-hidden", "true").html("&times;");
                button = $("<button />").attr("type", "button").addClass("close").attr("id", "closeBox").attr("aria-label", "luk").append(span);
            }
            var h3 = $("<h3 />").addClass("panel-title").text(usernames[i]).prop("outerHTML");
            var heading = $("<div />").addClass("panel-heading").html(h3).prepend(button).prop("outerHTML");
            var body: JQuery = $();
            if (!isNullOrWhitespace(questions[i])) {
                body = $("<div />").addClass("panel-body").text(questions[i]).prop("outerHTML");
            }
            var html = $("<div />").attr("style", "display: none;").addClass("panel panel-primary").attr("id", questionIds[i]).html(heading).append(body);
            helpList.append(html);
            var timeout = 200 * i;
            MathJax.Hub.Queue(["Typeset", MathJax.Hub]);
            setTimeout(showId(questionIds[i], timeout));
        }
    }

    chat.client.addQuestion = (username, question, questionId, admin) => {
        var helpList = $("#HelpList");
        var span, button = $();
        if (admin) {
            span = $("<span />").attr("aria-hidden", "true").html("&times;");
            button = $("<button />").attr("type", "button").addClass("close").attr("id", "closeBox").attr("aria-label", "luk").html(span);
        }
        var h3 = $("<h3 />").addClass("panel-title").text(username).prop("outerHTML");
        var heading = $("<div />").addClass("panel-heading").html(h3).prepend(button).prop("outerHTML");
        var body = $();
        if (!isNullOrWhitespace(question)) {
            body = $("<div />").addClass("panel-body").text(question).prop("outerHTML");
        }
        var html = $("<div />").attr("style", "display: none;").addClass("panel panel-primary").attr("id", questionId).html(heading).append(body);
        helpList.append(html);
        MathJax.Hub.Queue(["Typeset", MathJax.Hub]);
        $("#HelpList #" + questionId).show("blind");
    }

    chat.client.userAreQuesting = () => {
        setQuestionLayout(3);
    }

    chat.client.removeQuestion = questionId => {
        var element = $("#HelpList #" + questionId);
        element.hide("blind", () => {
            element.remove();
        });
    }

    chat.client.reloadPage = () => {
        location.reload(true);
    }

    chat.client.setLayout = layout => {
        setQuestionLayout(layout);
    }

    chat.client.sendChatMessage = (text, author, messageId, sender, appendToLast, canEdit) => {
        var span, button = $();
        if (canEdit) {
            span = $("<span />").attr("aria-hidden", "true").html("&times;");
            button = $("<button />").attr("type", "button").addClass("close").attr("id", "closeChatMessage").attr("aria-label", "luk").html(span);
        }
        var intter = $("<p />").text(text).prop("outerHTML");
        var p: JQuery = $("<p />").html(intter).attr("id", messageId).prepend(button).addClass("clearfix");
        if (appendToLast) {
            var location: JQuery = $(".chat li:last-child > div");
            location.append(p);
        } else {
            var strong: JQuery = $("<strong />").addClass("primary-font").text(author);
            var header: JQuery = $("<div />").addClass("header").append(strong);
            var chatBody = $("<div />").addClass("chat-body").addClass("clearfix").append(header).append(p);
            var li: JQuery = $("<li />").addClass("clearfix").append(chatBody);
            if (sender) {
                li = li.addClass("left");
            } else {
                li = li.addClass("right");
            }
            $(".chat").append(li);
        }
        MathJax.Hub.Queue(["Typeset", MathJax.Hub]);
    }

    chat.client.removeChatMessage = messageId => {
        var message = $("#" + messageId);
        var parent = message.parent();
        message.remove();
        if (parent.children().length <= 1) {
            parent.parent().remove();
        }
    }

    chat.client.sendChatMessages = (text, author, messageId, sender, appendToLast, canEdit) => {
        $(".chat").empty();
        for (var i = 0; i < text.length; i++) {
            chat.client.sendChatMessage(text[i], author[i], messageId[i], sender[i], appendToLast[i], canEdit[i]);
        }
    }

    chat.client.checkVersion = version => {
        if (version !== 3) {
            location.reload(true);
        }
    }

    chat.client.clearChat = () => {
        $(".chat").empty();
    }

    chat.client.userCreationFailed = (errorMessage) => {
        var input = createUserPopover.find("#userCreationError");
        input.text(errorMessage);
        input.show("blind");
    }

    chat.client.userCreationSuccess = () => {
        if (createUserPopover !== undefined)
            createUserPopover.popover("hide");
        showNotification("success", "Din bruger er nu oprettet", "Tillykke!");
        setLoginState(1);
    }

    chat.client.loginFailed = () => {
        if (loginUserPopover === undefined) return;
        loginUserPopover.find("#invalidLoginMessage").show("blind");
    }

    chat.client.loginSuccess = () => {
        if (loginUserPopover === undefined) return;
        loginUserPopover.popover("hide");
        showNotification("success", "Du er nu logget ind", "Login succesfuld!");
        setLoginState(1);

    }

    chat.client.userLoggedOut = () => {
        setLoginState(2);
        $("#ChannelList").empty();
        $("#HelpList").empty();
        setQuestionLayout(2);
        $(".chat").empty();
        showNotification("info", "Du er nu logget ud", "Logud succesfuld");
    }

    chat.client.updateUsername = (name) => {
        $("#CurrentUserName").text(name);
        if (firstName) {
            showNotification("info", "Dit brugernavn er blevet ændret til: \"" + name + "\"", "Brugernavn ændret");
        } else {
            firstName = !firstName;
        }
    }

    chat.client.updateQuestionAuthorName = (name, id) => {
        var question = $("#HelpList #" + id);
        question.find("h3.panel-title").text(name);
    }

    chat.client.updateChatMessageAuthorName = (name, ids) => {
        var chatten = $(".chat");
        for (var i = 0; i < ids.length; i++) {
            var element = chatten.find("#" + ids[i]);
            var parent = element.parent();
            var strong = parent.find("strong");
            strong.text(name);
        }
    }

    function showNotification(typ: string, text: string, title: string) {
        // ReSharper disable once WrongExpressionStatement
        new PNotify({
            title: title,
            text: text,
            type: typ,
            animation: "show",
            nonblock: {
                nonblock: true,
                nonblock_opacity: .2
            },
            styling: "bootstrap3"
        });
    }
    function setQuestionLayout(layout: any) {
        switch (layout) {
            // Standard Layout
        case 1:
            $("#requestingHelp").hide();
            $("#noChannelsSelected").hide();
            $("#requestHelpForm").show();
            break;
        // No channel selected
        case 2:
            $("#requestingHelp").hide();
            $("#noChannelsSelected").show();
            $("#requestHelpForm").hide();
            break;
        // Have question in current channel
        case 3:
            $("#requestingHelp").show();
            $("#noChannelsSelected").hide();
            $("#requestHelpForm").hide();
            break;
        default:
        }
    }

    function setLoginState(layout: Number) {
        switch (layout) {
            // Logged in
        case 1:
            $(".not-logged-in").hide();
            $(".logged-in").show();
            break;
        case 2:
                // Not logged in
            $(".not-logged-in").show();
            $(".logged-in").hide();
            break;
        default:
        }
    }

    function enableBetaFunctions(pass: string) {
        if (pass === "12345") {
            $(".beta").show();
            return "Success";
        } else {
            return "Failed";
        }
    }

    function showId(id, timeout) {
        jQuery("#" + id).delay(timeout).show("drop", { "direction": "up" });
    }

    function isNullOrWhitespace(input) {
        if (typeof input === "undefined" || input == null) return true;
        return input.replace(/\s/g, "").length < 1;
    }

    $.connection.hub.start().done(() => {
        chat.server.getData(2);
        setInterval(() => {
            chat.server.getData(2);
        }, 1000 * 60 * 10);
        // Show the get username modal
        $("#loadingAnimation").hide("blind");
        $("#interface").show("blind");
        $("#usernameModal").modal("show");
        $("#usernameModalInput").focus();

        $(document).on("click", "span.channel-remove", function (e) {
            e.preventDefault();
            var tmpid = $(this).parent().attr("id");
            chat.server.exitChannel(tmpid);
        });

        $(document).on("click", "div.joinChannel > a", function (e) {
            e.preventDefault();
            var tmpid = $(this).attr("id");
            chat.server.joinChannel(tmpid);
            if ($(this).parent().attr("id") === "SearchChannelResults") {
                $(this).hide("slide", {}, 400, function () {
                    $(this).remove();
                });
            }
        });

        $(document).on("click", "div#ChannelList > a", function (e) {
            e.preventDefault();
            var tmpid = $(this).attr("id");
            chat.server.changeToChannel(tmpid);
        });

        $(document).on("click", "#closeBox", function (e) {
            e.preventDefault();
            var tmpid = $(this).parent().parent().attr("id");
            chat.server.removeQuestion(tmpid);
        });

        $(document).on("click", "#closeChatMessage", function (e) {
            e.preventDefault();
            var tmpid = $(this).parent().attr("id");
            chat.server.removeChatMessage(tmpid);
        });
    });

    $(document).ready(() => {

        $("#usernameModalForm").submit(() => {
            setUserName();
        });

        $("#requestHelpForm").submit(() => {
            var question: string = $("#question").val();
            chat.server.requestHelp(question);
            $("#question").val("");
            setQuestionLayout(3);
        });

        $("#CreateChannelForm").submit(() => {
            var channelName: string = $("#newChannelName").val();
            chat.server.createNewChannel(channelName);
            $("#newChannelName").val("");
        });

        $("#SearchChannelName").keyup(function () {
            $("#SearchChannelResults").empty();
            var value = $(this).val();
            if (value.length > 0) {
                chat.server.searchForChannel(value);
            }
        });

        $("#removeQuestion").click((e) => {
            e.preventDefault();
            chat.server.removeQuestion(null);
            setQuestionLayout(1);
        });

        $("#editQuestion").click((e) => {
            e.preventDefault();
            $("#changeQuestionModal").modal("show");
            $("#newQuestionText").focus();
            chat.server.getData(1);
        });

        $("#newQuestionSubmit").click((e) => {
            e.preventDefault();
            var question = $("#newQuestionText").val();
            chat.server.changeQuestion(question);
            $("#changeQuestionModal").modal("hide");
        });

        $("#chatForm").submit(() => {
            var message: string = $("#chatMessageInput").val();
            if (!isNullOrWhitespace(message)) {
                chat.server.chat(message);
                $("#chatMessageInput").val("");
            }
        });

        $("#editUsername").click((e) => {
            e.preventDefault();
            if (loginUserPopover !== undefined) loginUserPopover.popover("hide");
            if (createUserPopover !== undefined) createUserPopover.popover("hide");
            setTimeout(() => {
                changeUsernamePopover = $("#" + $("#editUsername").attr("aria-describedby"));
                var name = $("#CurrentUserName").text();
                var input = changeUsernamePopover.find("#changeusernameInput");
                input.val(name);
            });
        }).popover({
            html: true,
            content: () => $("#changeusernameContent").html(),
            title: () => $("#changeUsernameTitle").html(),
            placement: "bottom",
            container: "body"
        });

        $("#reloadNearbyChannels").click((e) => {
            e.preventDefault();
            chat.server.loadNearbyChannels();
        });

        $("#ClearChatButton").click((e) => {
            e.preventDefault();
            chat.server.clearChat();
        });

        $("#CreateUserButton").popover({
            html: true,
            content: () => $("#createUserContent").html(),
            title: () => $("#createUserTitle").html(),
            placement: "bottom",
            container: "body"
        }).click((e) => {
            e.preventDefault();
            if (loginUserPopover !== undefined) loginUserPopover.popover("hide");
            if (changeUsernamePopover !== undefined) changeUsernamePopover.popover("hide");
            setTimeout(() => {
                createUserPopover = $("#" + $("#CreateUserButton").attr("aria-describedby"));
                var name = $("#CurrentUserName").text();
                var input = createUserPopover.find("#CreateUserName");
                input.val(name);
            }, 500);
            
        });

        $(document).on("submit", "#createUserForm", e => {
            e.preventDefault();
            var name = createUserPopover.find("#CreateUserName").val();
            var mail = createUserPopover.find("#CreateUserEmail").val();
            var pass = createUserPopover.find("#CreateUserPw").val();
            var pass2 = createUserPopover.find("#CreateUserPwConfirm").val();
            if (isNullOrWhitespace(name) || isNullOrWhitespace(mail) || isNullOrWhitespace(pass) || isNullOrWhitespace(pass2)) return;
            if (pass === pass2) {
                chat.server.createNewUser(name, mail, pass);
            } else {
                chat.client.userCreationFailed("Kodeorderne er ikke ens");
            }
        }).on("submit", "#loginUserForm", e => {
            e.preventDefault();
            var mail = loginUserPopover.find("#LoginUserEmail").val();
            var pass = loginUserPopover.find("#LoginUserPassword").val();
            if (isNullOrWhitespace(mail) || isNullOrWhitespace(pass)) return;
            chat.server.loginUser(mail, pass);
        }).on("submit", "#changeusernameForm", e => {
            e.preventDefault();
            console.log("submitted");
            var input = changeUsernamePopover.find("#changeusernameInput");
            var name = input.val();
            name = name.replace(/[\s]+/g, " ");
            var n = name.match(patt);
            chat.server.setUsername(n[0]);
            changeUsernamePopover.popover("hide");
        });

        $("#LoginButton").popover({
            html: true,
            content: () => $("#loginUserContent").html(),
            title: () => $("#loginUserTitle").html(),
            placement: "bottom",
            container: "body"
        }).click((e) => {
            e.preventDefault();
            if (createUserPopover !== undefined) createUserPopover.popover("hide");
            if (changeUsernamePopover !== undefined) changeUsernamePopover.popover("hide");
            setTimeout(() => {
                loginUserPopover = $("#" + $("#LoginButton").attr("aria-describedby"));
            }, 500);
            });

        $("#logoutButton").click((e) => {
            e.preventDefault();
            chat.server.logoutUser();
        });
    });

function getPopoverId() {
    return $(this).attr("aria-describedby");
}
