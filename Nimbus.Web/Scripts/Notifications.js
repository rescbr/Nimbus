function newMessageNotification(msg) {
    alert(msg);
}


$(function () {
    //habilita log
    $.connection.hub.logging = true;
    var nimbusHub = $.connection.nimbusHub;

    // Declare a function on the chat hub so the server can invoke it          
    nimbusHub.client.newMessageNotification = newMessageNotification;

    // Start the connection
    $.connection.hub.start();
});