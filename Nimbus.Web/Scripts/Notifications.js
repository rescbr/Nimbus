function makeElementFromHtml(htmlString) {
    var auxDiv = document.createElement("div");
    auxDiv.innerHTML = htmlString;
    return auxDiv.firstElementChild;
}

function newMessageNotification(msg) {
    try {
        var wrapperUser = document.getElementById("wrapperUser");
        var divMyChannels = document.getElementById("divMyChannels");
        var msgElement = makeElementFromHtml(msg);

        wrapperUser.insertBefore(msgElement, divMyChannels);
        
    } catch (e) { }
    
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
                    var recvElement = makeElementFromHtml(htmlData);
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
                    var recvElement = makeElementFromHtml(htmlData);
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