function newMessageNotification(msg) {
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
                    var recvElement = wrapperDiv.firstChild;
                    parentDiv.insertBefore(recvElement, document.getElementById("divAnswerComment_" + notif.parentId));
                }
            }
        });
    } else  {//if (document.getElementById("divNoComments") !== null) {
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
    }
}

function registerTopicNotifications(topicId) {
    var nimbusHub = $.connection.nimbusHub;
    nimbusHub.server.registerTopicCommentNotifications(topicId);
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
        if (nimbusRegisterTopic !== null) {
            registerTopicNotifications(nimbusRegisterTopic);
        }
    });
});