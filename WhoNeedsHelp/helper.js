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

chat.client.broadcastTable = function(table) {
    $("#HelpList").html(table);
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

chat.client.setChannel = function (channel) {
    $("#CurrentChannelId").html(channel);
    $("div#ChannelList > a.active").removeClass("active", 400);
    setTimeout(function() {
        $("#" + channel).addClass("active", 400);
    }, 100);
}

chat.client.updateChannelCount = function(activeUsers, connectedUsers, channelId) {
    var badge = activeUsers + "/" + connectedUsers;
    $("a#" + channelId + " .badge").html(badge);
}

chat.client.errorChannelAlreadyMade = function() {
    alert("This channel already exists");
}

chat.client.exitChannel = function (e) {
    var tmpid = $("#" + e);
    tmpid.hide("blind", function () {
        tmpid.remove();
    });
}

chat.client.channelsFound = function(ids, names) {
    var resultList = $("#SearchChannelResults");

    for (var i = 0; i < ids.length; i++) {
        var html = "<a href='#' style='display: none;' id='" + ids[i] + "' class='list-group-item'>" + names[i] + "</a>";
        resultList.append(html);
        $("#" + ids[i]).show("clip");
    }
}

chat.client.addQuestions = function (usernames, questions, questionIds) {
    var helpList = $("#HelpList");
    helpList.empty();
    for (var i = 0; i < questionIds.length; i++) {
        var html = "<div style=\"display: none;\" class=\"panel panel-primary\" id=\"" + questionIds[i] + "\"><div class=\"panel-heading\"> <h3 class=\"panel-title\">" + usernames[i] + "</h3></div><div class=\"panel-body\">" + questions[i] + "</div></div>";
        helpList.append(html);
        setTimeout(showId(questionIds[i]), 200*i);
    }
}

chat.client.addQuestion = function(username, question, questionId) {
    var helpList = $("#HelpList");
    var html = "<div style=\"display: none;\" class=\"panel panel-primary\" id=\"" + questionId + "\"><div class=\"panel-heading\"> <h3 class=\"panel-title\">" + username + "</h3></div><div class=\"panel-body\">" + question + "</div></div>";
    helpList.append(html);
    $("#" + questionId).show("drop");
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

var showId = function(id) {
    $("#" + id).show("drop");
}
    
$.connection.hub.start().done(function () {
    console.log("connected");
    fetchTables = function() {
        chat.server.getData(1);
    }

    $(document).on("click", "span.channel-remove", function () {
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
});


$(document).ready(function () {
    // Show the get username modal
    $("#usernameModal").modal("show");
    //$("#modalForm").submit(function () {

    $("#usernameModalForm").submit(function () {
        setUserName2();
    });

    $("#requestHelpForm").submit(function () {
        var question = $("#question").val();
        if (question !== "") {
            
        }
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
});