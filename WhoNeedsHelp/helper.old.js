﻿"use strict";
// The pattern for the username
var patt = /[\w][\w ]+[\w]/;

// Make a connection to the correct hub
// In this case the CentralHub which handles this application.
var chat = $.connection.centralHub;
var fetchTables;
//var setUserName;
//var setUserName2;
//var validate;

var setUserName2 = function () {
    var input = $("#usernameModalInput");
    var name = input.val();
    name = name.replace(/[\s]+/g, " ");
    var n = name.match(patt);
    console.log(n[0]);
    chat.server.send("1", n[0]);
    $("#CurrentUserName").html(n[0]);
    $("#usernameModal").modal("hide");
    $("#SearchChannelName").focus();
    return false;
}

var validate = function() {
    var t = $("#usernameModalInput").val();
    if (patt.test(t)) {
        $("#usernameGroup").addClass("has-success").removeClass("has-error");
    } else {
        $("#usernameGroup").addClass("has-error").removeClass("has-success");
    }
}

chat.client.log = function (text)
{
    console.log(text);
}

chat.client.appendChannel = function (channelname, channelid) {
    var html = "<a href='#' style='display: none;' id='" + channelid + "' class='list-group-item'><span class='glyphicon glyphicon-remove text-danger channel-remove'></span><span class='badge'>0/0</span> " + channelname + "</a>";
    $("#ChannelList").append(html);
    $("#" + channelid).show("blind");
    chat.server.send("7", channelid);
}

chat.client.setChannel = function (channel, areUserQuestioning) {
    $("#CurrentChannelId").html(channel);
    $("div#ChannelList > a").stop().removeClass("active").attr("style", "");
    setTimeout(function() {
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

chat.client.updateChannelCount = function(activeUsers, connectedUsers, channelId) {
    var badge = activeUsers + "/" + connectedUsers;
    $("a#" + channelId + " .badge").html(badge);
}

chat.client.updateQuestion = function (question, questionId) {
    console.log("updating question");
    var panel = $("#" + questionId + " .panel-body");
    if (question === "") {
        panel.hide("blind", function() {
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

chat.client.errorChannelAlreadyMade = function() {
    alert("This channel already exists");
}

chat.client.sendQuestion = function (question) {
    console.log(question);
    $("#newQuestionText").val(question);
}

chat.client.exitChannel = function (e) {
    var tmpid = $("#" + e);
    tmpid.hide("blind", function () {
        tmpid.remove();
        tmpid = $("#ChannelList a:first-child").attr("id");
        console.log(tmpid);
        if (tmpid == undefined) {
            setQuestionLayout(2);
            $("#CurrentChannelId").html("Ikke forbundet til nogen kanal");
            $("#HelpList").children().each(function(index) {
                $(this).delay(index * 300).hide("blind", function() {
                    $(this).remove();
                });
            });
        } else {
            chat.server.send("7", tmpid);
        }
    });
}

chat.client.listChannels = function(channelsNames, channelIds, activeChannelId) {
    var channelList = $("#ChannelList");
    channelList.empty();

    for (var i = 0; i < channelsNames.length; i++) {
        var html = "<a href='#' style='display: none;' id='" + channelIds[i] + "' class='list-group-item'><span class='glyphicon glyphicon-remove text-danger channel-remove'></span><span class='badge'>0/0</span> " + channelsNames[i] + "</a>";
        $("#ChannelList").append(html);
        $("#" + channelIds[i]).delay(i * 200).show("blind");
    }

    console.log(activeChannelId);
    if (activeChannelId != null) {
        chat.server.send("7", activeChannelId);
    }
}

chat.client.getUsername = function() {
    chat.server.send("1", $("#CurrentUserName").html());
}

chat.client.channelsFound = function(ids, names) {
    var resultList = $("#SearchChannelResults");

    for (var i = 0; i < ids.length; i++) {
        var html = "<a href='#' style='display: none;' id='" + ids[i] + "' class='list-group-item'>" + names[i] + "</a>";
        resultList.append(html);
        $("#" + ids[i]).show("clip");
    }
}

chat.client.addQuestions = function (usernames, questions, questionIds, admin) {
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
        var span = "", button = "";
        if (admin) {
            span = $("<span />").attr("aria-hidden", "true").html("&times;");
            button = $("<button />").attr("type", "button").addClass("close").attr("id", "closeBox").attr("aria-label", "luk").html(span);
        }
        var h3 = $("<h3 />").addClass("panel-title").text(usernames[i]);
        var heading = $("<div />").addClass("panel-heading").html(h3).prepend(button);
        var body = "";
        if (!isNullOrWhitespace(questions[i])) {
            body = $("<div />").addClass("panel-body").text(questions[i]);
        }
        var html = $("<div />").attr("style", "display: none;").addClass("panel panel-primary").attr("id", questionIds[i]).html(heading).append(body);
        helpList.append(html);
        var timeout = 200 * i;
        setTimeout(showId(questionIds[i], timeout));
    }
}

chat.client.addQuestion = function(username, question, questionId, admin) {
    var helpList = $("#HelpList");
    var span = "", button = "";
    if (admin) {
        span = $("<span />").attr("aria-hidden", "true").html("&times;");
        button = $("<button />").attr("type", "button").addClass("close").attr("id", "closeBox").attr("aria-label", "luk").html(span);
    }
    var h3 = $("<h3 />").addClass("panel-title").text(username);
    var heading = $("<div />").addClass("panel-heading").html(h3).prepend(button);
    var body = "";
    if (!isNullOrWhitespace(question)) {
        body = $("<div />").addClass("panel-body").text(question);
    }
    var html = $("<div />").attr("style", "display: none;").addClass("panel panel-primary").attr("id", questionId).html(heading).append(body);
    helpList.append(html);
    $("#" + questionId).show("blind");
}

chat.client.userAreQuesting = function() {
    setQuestionLayout(3);
}

chat.client.removeQuestion = function(questionId) {
    var element = $("#" + questionId);
    console.log(questionId);
    element.hide("blind", function() {
        element.remove();
    });
}

chat.client.reloadPage = function() {
    location.reload(true);
}

chat.client.setLayout = function(layout) {
    setQuestionLayout(layout);
}

chat.client.sendChatMessage = function(text, time, author, sender) {
    
}

var setQuestionLayout = function(layout) {
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

var showId = function(id, timeout) {
    $("#" + id).delay(timeout).show("drop", {"direction": "up"});
}

var isNullOrWhitespace = function(input) {
    if (typeof input === "undefined" || input == null) return true;
    return input.replace(/\s/g, "").length < 1;
}

$.connection.hub.start().done(function() {
    console.log("connected");
    fetchTables = function() {
        chat.server.getData(1);
    }

    $(document).on("click", "span.channel-remove", function() {
        var tmpid = $(this).parent().attr("id");
        chat.server.send("4", tmpid);
    });

    $(document).on("click", "div#SearchChannelResults > a", function() {
        var tmpid = $(this).attr("id");
        chat.server.send("6", tmpid);
        $(this).hide("blind", function() {
            $(this).remove();
        });
    });

    $(document).on("click", "div#ChannelList > a", function() {
        var tmpid = $(this).attr("id");
        console.log(tmpid);
        chat.server.send("7", tmpid);
    });

    $(document).on("click", "#closeBox", function(){
        var tmpid = $(this).parent().parent().attr("id");
        console.log(tmpid);
        chat.server.send("8", tmpid);
    });
});


$(document).ready(function () {
    // Show the get username modal
    $("#usernameModal").modal("show");
    $("#usernameModalInput").focus();
    //$("#modalForm").submit(function () {

    $("#usernameModalForm").submit(function () {
        setUserName2();
    });

    $("#requestHelpForm").submit(function () {
        var question = $("#question").val();
        chat.server.send("2", question);
        setQuestionLayout(3);
    });

    $("#CreateChannelForm").submit(function () {
        var channelName = $("#newChannelName").val();
        chat.server.send("3", channelName);
    });

    $("#SearchChannelName").keyup(function () {
        $("#SearchChannelResults").empty();
        var value = $(this).val();
        if (value.length > 4) {
            chat.server.send("5", value);
        }
    });

    $("#removeQuestion").click(function () {
        chat.server.send("8", null);
        setQuestionLayout(1);
    });

    $("#editQuestion").click(function() {
        console.log("edit");
        $("#changeQuestionModal").modal("show");
        $("#newQuestionText").focus();
        chat.server.getData(1);
    });

    $("#newQuestionSubmit").click(function () {
        console.log("submit");
        var question = $("#newQuestionText").val();
        console.log(question);
        chat.server.send("9", question);
        $("#changeQuestionModal").modal("hide");
    });

    $("#chatForm").submit(function() {
        var message = $("#chatMessageInput").val();
        if (!isNullOrWhitespace(message)) {
            chat.serverS.send("10");
        }
    });
});