/// <reference path="Scripts/typings/jquery/jquery.d.ts" />
/// <reference path="Scripts/typings/signalr/signalr.d.ts" />
/// <reference path="Client/IServer.ts"/>
/// <reference path="Scripts/typings/bootstrap/bootstrap.d.ts"/>
/// <reference path="Scripts/typings/jqueryui/jqueryui.d.ts"/>

"use strict";
// The pattern for the username
var patt = /[\w][\wæøåöäÆØÅÖÄ ]+[\w]/;

// Make a connection to the correct hub
// In this case the CentralHub which handles this application.
var chat = $.connection.centralHub;
var fetchTables;
//var setUserName;
//var setUserName2;
//var validate;

var setUserName2 = () => {
    var input = $("#usernameModalInput");
    var name = input.val();
    name = name.replace(/[\s]+/g, " ");
    var n = name.match(patt);
    console.log(n[0]);
    //chat.server.send("1", n[0]);
    chat.server.setUsername(n[0]);
    $("#CurrentUserName").html(n[0]);
    $("#usernameModal").modal("hide");
    $("#SearchChannelName").focus();
    return false;
}

var validate = () => {
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
    // TODO Prevent scripting injection attacks in the channel name
    var html = "<a href='#' style='display: none;' id='" + channelid + "' class='list-group-item'><span class='glyphicon glyphicon-remove close channel-remove'></span><span class='badge'>0/0</span> " + channelname + "</a>";
    $("#ChannelList").append(html);
    $("#" + channelid).show("slide", 400);
    chat.server.send("7", channelid);
}

chat.client.setChannel = (channel, areUserQuestioning) => {
    $("#CurrentChannelId").html(channel);
    $("div#ChannelList > a").stop().removeClass("active").attr("style", "");
    setTimeout(() => {
        $("#" + channel).addClass("active", 400);
    }, 100);
    if (areUserQuestioning) {
        console.log("User are questing");
        setQuestionLayout(3);
    } else {
        console.log("User are not questing");
        setQuestionLayout(1);
    }
}

chat.client.updateChannelCount = (activeUsers, connectedUsers, channelId) => {
    var badge = activeUsers + "/" + connectedUsers;
    $("a#" + channelId + " .badge").html(badge);
}

chat.client.updateQuestion = (question, questionId) => {
    // TODO Prevent scripting injection attacks
    console.log("updating question");
    var panel = $("#" + questionId + " .panel-body");
    if (question === "") {
        panel.hide("blind", function () {
            $(this).remove();
        });
    } else {
        if (panel.length > 0) {
            panel.html(question);
        } else {
            var html = "<div style=\"display: none;\" class=\"panel-body\">" + question + "</div>";
            $("#" + questionId).append(html);
            $("#" + questionId + " .panel-body").show("blind");
        }
    }
}

chat.client.errorChannelAlreadyMade = () => {
    alert("This channel already exists");
}

chat.client.sendQuestion = question => {
    console.log(question);
    $("#newQuestionText").val(question);
}

chat.client.exitChannel = e => {
    var tmpid = $("#" + e);
    tmpid.hide("slide", {}, 400, () => {
        tmpid.remove();
        var id = $("#ChannelList a:first-child").attr("id");
        if (id == undefined) {
            setQuestionLayout(2);
            $("#CurrentChannelId").html("Ikke forbundet til nogen kanal");
            $("#HelpList > div").each(function (index) {
                $(this).delay(index * 300).hide("blind", {}, 400, function () {
                    $(this).remove();
                });
            });
        } else {
            chat.server.send("7", id);
        }
    });
}

chat.client.channelsFound = (ids, names) => {
    var resultList = $("#SearchChannelResults");

    for (var i = 0; i < ids.length; i++) {
        //var html = "<a href='#' style='display: none;' id='" + ids[i] + "' class='list-group-item'>" + names[i] + "</a>";
        var html = $("<a />");
        html = html.attr("style", "display: none;").attr("id", ids[i]).attr("class", "list-group-item").attr("href", "#").text(names[i]);
        resultList.append(html);
        $("#" + ids[i]).show("clip");
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
        /*var html = "";
        if (admin) {
            console.log("Admin");
            html = "<div style=\"display: none;\" class=\"panel panel-primary\" id=\"" + questionIds[i] + "\"><div class=\"panel-heading\"><button type=\"button\" class=\"close\" id=\"closeBox\"  aria-label=\"luk\"><span aria-hidden=\"true\">&times;</span></button> <h3 class=\"panel-title\">" + usernames[i] + "</h3></div>{0}</div>";
        } else {
            console.log("not admin");
            html = "<div style=\"display: none;\" class=\"panel panel-primary\" id=\"" + questionIds[i] + "\"><div class=\"panel-heading\"> <h3 class=\"panel-title\">" + usernames[i] + "</h3></div>{0}</div>";
        }
        if (questions[i] === "") {
            html = html.replace("{0}", "");
        } else {
            html = html.replace("{0}", "<div class=\"panel-body\">" + questions[i] + "</div>");
        }*/
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
    $("#" + questionId).show("blind");
}

chat.client.userAreQuesting = () => {
    setQuestionLayout(3);
}

chat.client.removeQuestion = questionId => {
    var element = $("#" + questionId);
    console.log(questionId);
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
        console.log(location);
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
}

chat.client.removeChatMessage = messageId => {
    var message = $("#" + messageId);
    var parent = message.parent();
    message.remove();
    console.log(parent.children().length);
    if (parent.children().length <= 1) {
        parent.parent().remove();
    }
}

chat.client.sendChatMessages = (text, author, messageId, sender, appendToLast, canEdit) => {
    console.log("sendChatMessages");
    $(".chat").empty();
    for (var i = 0; i < text.length; i++) {
        chat.client.sendChatMessage(text[i], author[i], messageId[i], sender[i], appendToLast[i], canEdit[i]);
    }
}

chat.client.checkVersion = version => {
    if (version !== 1) {
        location.reload(true);
    }
}

var setQuestionLayout = layout => {
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

var showId = (id, timeout) => {
    jQuery("#" + id).delay(timeout).show("drop", { "direction": "up" });
}

var isNullOrWhitespace = input => {
    if (typeof input === "undefined" || input == null) return true;
    return input.replace(/\s/g, "").length < 1;
}

$.connection.hub.start().done(() => {
    console.log("connected");
    chat.server.getData(2);

    $(document).on("click", "span.channel-remove", function () {
        var tmpid = $(this).parent().attr("id");
        chat.server.send("4", tmpid);
    });

    $(document).on("click", "div.joinChannel > a", function () {
        var tmpid = $(this).attr("id");
        chat.server.send("6", tmpid);
        if ($(this).parent().attr("id") === "SearchChannelResults") {
            $(this).hide("slide", {}, 400, function() {
                $(this).remove();
            });
        }
    });

    $(document).on("click", "div#ChannelList > a", function () {
        var tmpid = $(this).attr("id");
        console.log(tmpid);
        chat.server.send("7", tmpid);
    });

    $(document).on("click", "#closeBox", function () {
        var tmpid = $(this).parent().parent().attr("id");
        console.log(tmpid);
        chat.server.send("8", tmpid);
    });

    $(document).on("click", "#closeChatMessage", function() {
        var tmpid = $(this).parent().attr("id");
        console.log(tmpid);
        chat.server.send("11", tmpid);
    });
});

$(document).ready(() => {
    setInterval(() => {
        chat.server.getData(2);
    }, 1000 * 60 * 10);
    // Show the get username modal
    $("#usernameModal").modal("show");
    $("#usernameModalInput").focus();
    //$("#modalForm").submit(function () {

    $("#usernameModalForm").submit(() => {
        setUserName2();
    });

    $("#requestHelpForm").submit(() => {
        var question = $("#question").val();
        chat.server.send("2", question);
        $("#question").val("");
        setQuestionLayout(3);
    });

    $("#CreateChannelForm").submit(() => {
        var channelName = $("#newChannelName").val();
        //chat.server.send("3", channelName);
        chat.server.createNewChannel(channelName);
        $("#newChannelName").val("");
    });

    $("#SearchChannelName").keyup(function () {
        $("#SearchChannelResults").empty();
        var value = $(this).val();
        if (value.length > 4) {
            chat.server.send("5", value);
        }
    });

    $("#removeQuestion").click(() => {
        chat.server.send("8", null);
        setQuestionLayout(1);
    });

    $("#editQuestion").click(() => {
        console.log("edit");
        $("#changeQuestionModal").modal("show");
        $("#newQuestionText").focus();
        chat.server.getData(1);
    });

    $("#newQuestionSubmit").click(() => {
        console.log("submit");
        var question = $("#newQuestionText").val();
        console.log(question);
        chat.server.send("9", question);
        $("#changeQuestionModal").modal("hide");
    });

    $("#chatForm").submit(() => {
        var message = $("#chatMessageInput").val();
        console.log(message);
        if (!isNullOrWhitespace(message)) {
            chat.server.send("10", message);
            $("#chatMessageInput").val("");
        }
    });

    $("#editUsername").click(() => {
        $("#usernameModal").attr("ata-backdrop", "").attr("data-keyboard", "");
        $("#selectUsernameButton").text("Gem");
        $("#usernameModal").modal("show");
    });
}); 