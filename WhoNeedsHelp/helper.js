"use strict";
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

chat.client.errorChannelAlreadyMade = function() {
    alert("This channel already exists");
}

chat.client.sendQuestion = function(question) {
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
        var html = "";
        if (admin) {
            html = "<div style=\"display: none;\" class=\"panel panel-primary\" id=\"" + questionIds[i] + "\"><div class=\"panel-heading\"> <h3 class=\"panel-title\">" + usernames[i] + "</h3></div>{0}</div>";
        } else {
            html = "<div style=\"display: none;\" class=\"panel panel-primary\" id=\"" + questionIds[i] + "\"><div class=\"panel-heading\"> <h3 class=\"panel-title\">" + usernames[i] + "</h3></div>{0}</div>";
        }
        if (questions[i] === "") {
            html = html.replace("{0}", "");
        } else {
            html = html.replace("{0}", "<div class=\"panel-body\">" + questions[i] + "</div>");
        }
        helpList.append(html);
        var timeout = 200 * i;
        console.log(timeout);
        setTimeout(showId(questionIds[i], timeout));
    }
}

chat.client.addQuestion = function(username, question, questionId, admin) {
    var helpList = $("#HelpList");
    var html = "";
    if (admin) {
        html = "<div style=\"display: none;\" class=\"panel panel-primary\" id=\"" + questionId + "\"><div class=\"panel-heading\"> <h3 class=\"panel-title\">" + username + "</h3></div>{0}</div>";
    } else {
        html = "<div style=\"display: none;\" class=\"panel panel-primary\" id=\"" + questionId + "\"><div class=\"panel-heading\"> <h3 class=\"panel-title\">" + username + "</h3></div>{0}</div>";
    }
    if (question === "") {
        html = html.replace("{0}", "");
    } else {
        html = html.replace("{0}", "<div class=\"panel-body\">" + question + "</div>");
    }
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
});