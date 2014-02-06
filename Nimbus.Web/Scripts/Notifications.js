function makeElementFromHtml(htmlString) {
    var auxDiv = document.createElement("div");
    auxDiv.innerHTML = htmlString;
    return auxDiv.firstElementChild;
}

function AcceptOrNotBeModerator(id, accepted, notifGuid) {
    var ajaxMessage = {};
    
    $.ajax({
        url: "/api/channel/AcceptOrNotBeModerator/" + id + "?accepted=" + accepted + "&guid=" + notifGuid,
        data: JSON.stringify(ajaxMessage),
        type: "POST",
        contentType: "application/json;charset=utf-8",
        statusCode: {
            200: function (newData) {
                try {
                    var notif = document.getElementById("notifModerator_" + notifGuid)
                    var notifBtn = notif.getElementsByClassName("attChoice")[0];
                    if (newData == true) {
                        notifBtn.innerHTML = "<div class='btnAccept'>Aceito</div>";
                    }
                    else if (newData == false) {
                        notifBtn.innerHTML = "<div class='btnRefuse'>Recusado</div>";
                    }
                } catch (e) { }
            },

            500: function () {
                //erro
                window.alert("Não foi possível aceitar ou recusar o pedido de moderação. Tente novamente mais tarde.");
            }
        }
    });
}

function newModeratorNotification(msg) {
    try {
        var divNotif = document.getElementById("divNotificationWrapper");
        if (divNotif.children.length == 0) {
            divNotif.innerHTML = msg.Html;
        } else {
            divNotif.insertBefore(makeElementFromHtml(msg.Html), divNotif.children[0]);
        }

    } catch (e) { }
}

function newMessageNotification(msg) {
    try {
        var divNotif = document.getElementById("divNotificationWrapper");
        if (divNotif.children.length == 0) {
            divNotif.innerHTML = msg.Html;
        } else {
            divNotif.insertBefore(makeElementFromHtml(msg.Html), divNotif.children[0]);
        }
        
    } catch (e) { }
    
    try {
        var divMsg = document.getElementById("divSeeMessages");
        if (divMsg !== null) {
            $.ajax({
                url: "/api/message/messagehtml/" + msg.MessageId,
                type: "GET",
                contentType: "application/json;charset=utf-8",
                statusCode: {
                    200: function (msgObj) {
                        var divMsg = document.getElementById("divSeeMessages");
                        divMsg.insertBefore(makeElementFromHtml(msgObj.Html), divMsg.children[0]);
                    }
                }
            });
        }
    } catch (e) { }

}

function getChannelNotifications(id, after) {

    if (typeof (after) === "undefined" || after == "") {
        after = "";
    } else {
        after = "?after=" + after;
    }

    document.getElementById("divNotificationLoadButton").style.display = "none";
    document.getElementById("imgNotificationLoad").style.display = "block";

    $.ajax({
        url: "/api/notification/channel/" + id + after,
        type: "GET",
        contentType: "application/json;charset=utf-8",
        statusCode: {
            200: function (notifObj) {
                var divNotif = document.getElementById("divNotificationWrapper");
                //se nao houver notificacoes na div e o count == 0, nao existem notifs.
                if (notifObj.Count == 0) {
                    if (divNotif.children.length == 0) {
                        divNotif.innerHTML = "Não há novas atividades neste canal."
                    }
                    document.getElementById("divNotificationLoadButton").style.display = "none";
                } else {
                    try {
                        if (divNotif.children.length == 0) {
                            divNotif.innerHTML = notifObj.Html;
                        } else {
                            divNotif.innerHTML += notifObj.Html;
                        }
                    } catch (e) { }
                    if (notifObj.Count >= 6) {
                        var loadButton = document.getElementById("divNotificationLoadButton");
                        loadButton.style.display = "block";
                        loadButton.onclick = function () { getChannelNotifications(id, notifObj.LastNotificationGuid); }
                    }
                }

                document.getElementById("imgNotificationLoad").style.display = "none";

            }
        }
    });

}

function getNotifications(after) {
    if (typeof (after) === "undefined") {
        after = "";
    } else {
        after = "?after=" + after;
    }

    document.getElementById("divNotificationLoadButton").style.display = "none";
    document.getElementById("imgNotificationLoad").style.display = "block";

    $.ajax({
        url: "/api/notification/" + after,
        type: "GET",
        contentType: "application/json;charset=utf-8",
        statusCode: {
            200: function (notifObj) {
                var divNotif = document.getElementById("divNotificationWrapper");
                //se nao houver notificacoes na div e o count == 0, nao existem notifs.
                if (notifObj.Count == 0 && divNotif.children.length == 0) {
                    divNotif.innerHTML = "Você não possui novas notificações. Participe no Nimbus, aumente seus pontos e fique atento a novidades!"
                    document.getElementById("divNotificationLoadButton").style.display = "none";
                } else {
                    try {
                        if (divNotif.children.length == 0) {
                            divNotif.innerHTML = notifObj.Html;
                        } else {
                            divNotif.innerHTML += notifObj.Html;
                        }
                    } catch (e) { }
                    if (notifObj.Count >= 6) {
                        var loadButton = document.getElementById("divNotificationLoadButton");
                        loadButton.style.display = "block";
                        loadButton.onclick = function () { getNotifications(notifObj.LastNotificationGuid); }
                    }
                }

                document.getElementById("imgNotificationLoad").style.display = "none";

            }
        }
    });

}

function newTopicCommentNotification(notif) {
    
    //var parentDiv = document.getElementById("divContentComment_" + notif.parentId);
    var parentDiv = document.getElementById("divAllCommentChild_" + notif.parentId);
    if (parentDiv !== null) {
        //existe comentario pai, entao renderiza um comentario filho
        $.ajax({
            url: "/api/comment/commentshtml/" + notif.commentId + "?type=onechild",
            type: "GET",
            contentType: "text/html;charset=utf-8",
            statusCode: {
                200: function (cw) {
                    parentDiv.innerHTML += cw.Html;
                    //var recvElement = makeElementFromHtml(cw.Html);
                    //parentDiv.insertBefore(recvElement, document.getElementById("btn_skipCommentsChild_" + notif.parentId));
                }
            }
        });
    } else if (document.getElementById("divNoComments") !== null) {
        //foi enviado um comentario filho, mas o pai nao existe. condição de erro, recarregar tudo
        //renderiza todos os comentarios
        $.ajax({
            url: "/api/comment/commentshtml/" + notif.topicId + "?type=notificationtopic",
            type: "GET",
            contentType: "text/html;charset=utf-8",
            statusCode: {
                200: function (cw) {
                    document.getElementById("divAllComments").innerHTML = cw.Html;
                }
            }
        });
    } else {
        //foi enviado um comentario pai
        //insere comentario pai
        $.ajax({
            url: "/api/comment/commentshtml/" + notif.commentId + "?type=oneparent",
            type: "GET",
            contentType: "text/html;charset=utf-8",
            statusCode: {
                200: function (cw) {
                    var recvElement = makeElementFromHtml(cw.Html);
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
    nimbusHub.client.newModeratorNotification = newModeratorNotification;
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