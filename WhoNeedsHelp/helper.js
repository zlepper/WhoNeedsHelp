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
    $("div#ChannelList > a.active").removeClass("active", 1000);
    $("#" + channel).addClass("active", 1000);
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
    
$.connection.hub.start().done(function () {
    console.log("connected");
    fetchTables = function() {
        chat.server.getData(1);
    }

    $(document).on("click", "a.channel-remove", function () {
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
        return false;
    });

    $("#CreateChannelForm").submit(function () {
        var channelName = $("#newChannelName").val();
        console.log(channelName);
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