function newMessageNotification(msg) {
    alert(msg);
}


$(function () {
    //habilita log
    $.connection.hub.logging = true;
    var nimbusHub = $.connection.nimbusHub;
      
    nimbusHub.client.newMessageNotification = newMessageNotification;

    // Start the connection
    $.connection.hub.start();
});