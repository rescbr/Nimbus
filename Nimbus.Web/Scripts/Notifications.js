﻿function newMessageNotification(msg) {
    alert(msg);
}

function newTopicCommentNotification(notif) {
    
    var parentDiv = document.getElementById("divContentComment_" + notif.parentId);
    if (parentDiv !== null) {
        //existe comentario pai
        $.ajax({
            url: "/topic/comment/" + notif.commentId,
            type: "GET",
            contentType: "text/html;charset=utf-8",
            statusCode: {
                200: function (htmlData) {
                    var wrapperDiv = document.createElement("div");
                    wrapperDiv.innerHTML = htmlData;
                    var recvElement = wrapperDiv.firstElementChild  ;
                    parentDiv.insertBefore(recvElement, document.getElementById("divAnswerTopic_" + notif.parentId));
                }
            }
        });
    } else if (document.getElementById("divNoComments") !== null) {
        //renderiza todos os comentarios
        $.ajax({
            url: "/topic/comments/" + notif.topicId,
            type: "GET",
            contentType: "text/html;charset=utf-8",
            statusCode: {
                200: function (htmlData) {
                    document.getElementById("divAllComments").innerHTML = htmlData;
                }
            }
        });
    } else {
        $.ajax({
            url: "/topic/parentcomment/" + notif.commentId,
            type: "GET",
            contentType: "text/html;charset=utf-8",
            statusCode: {
                200: function (htmlData) {
                    var wrapperDiv = document.createElement("div");
                    wrapperDiv.innerHTML = htmlData;
                    var recvElement = wrapperDiv.firstElementChild;
                    document.getElementById("divAllComments").insertBefore(recvElement,
                        document.getElementById("placeholderAfterLastComment"));
                }
            }
        });

    }
}

$(function () {
    //habilita log
    $.connection.hub.logging = true;
    var nimbusHub = $.connection.nimbusHub;
      
    nimbusHub.client.newMessageNotification = newMessageNotification;
    nimbusHub.client.newTopicCommentNotification = newTopicCommentNotification;
    // Start the connection
    $.connection.hub.start().done(function () {;
        //requisicoes de registro
        try {
            if (nimbusRegisterTopic !== null) {
                nimbusHub.server.registerTopicCommentNotifications(nimbusRegisterTopic);
            }
        } catch (e) { }

        try {
            if (nimbusRegisterMessageNotifications === true) {
                nimbusHub.server.registerMessageNotifications();
            }
        } catch (e) { }
    });
});