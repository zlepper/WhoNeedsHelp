/*$(function () {
    // Declare a proxy to reference the hub.
    var chat = $.connection.chatHub;

    // Create a function that the hub can call to broadcast messages.
    chat.client.broadcastMessage = function (name, message) {
        // Html encode display name and message.
        var encodedName = $('<div />').text(name).html();
        var encodedMsg = $('<div />').text(message).html();
        // Add the message to the page.
        $('#discussion').append('<li><strong>' + encodedName
            + '</strong>:&nbsp;&nbsp;' + encodedMsg + '</li>');
    };

    // Get the user name and store it to prepend to messages.
    $('#displayname').val(prompt('Enter your name:', ''));

    // Set initial focus to message input box.
    $('#message').focus();

    chat.client.sendOldData = function (message) {
        console.log("something");
        console.log(message);
        $('#discussion').append(message);
    }

    // Start the connection.
    $.connection.hub.start().done(function () {
        chat.server.getData();

        $('#sendmessage').click(function () {
            // Call the Send method on the hub.
            chat.server.send($('#displayname').val(), $('#message').val());
            // Clear text box and reset focus for next comment.
            $('#message').val('').focus();
        });
    });
});*/


"use strict";
// The pattern for the username
var patt = /[\w][\w ]+[\w]/;

// Make a connection to the correct hub
// In this case the CentralHub which handles this application.
var chat = $.connection.centralHub;
var fetchTables;

    function setUserName2() {
        var input = $("#usernameModalInput");
        var name = input.val();
        name = name.replace(/[\s]+/g, " ");
        var n = name.match(patt);
        console.log(n[0]);
        chat.server.send(1, null, n[0]);
        $("#usernameModal").modal("hide");
        fetchTables();
    }

    function setUserName() {
        setUserName2();
        return false;
    }

    var validate = function(e) {
        if (event.keyCode === 13) {
            setUserName2();
        } else {
            var t = $("#usernameModalInput").val();
            if (patt.test(t)) {
                $("#usernameGroup").addClass("has-success").removeClass("has-error");
            } else {
                $("#usernameGroup").addClass("has-error").removeClass("has-success");
            }
        }
    }

    chat.client.broadcastMessage = function (table) {
        $("#HelpList").html(table);
    }
$.connection.hub.start().done(function () {
    fetchTables = function() {
        chat.server.getData(1);
    }

    

    $(document).ready(function () {
        // Show the get username modal
        $("#usernameModal").modal("show");
        //$("#modalForm").submit(function () {
        
        $("#requestHelpForm").submit(function () {
            return false;
        });

    });
});


