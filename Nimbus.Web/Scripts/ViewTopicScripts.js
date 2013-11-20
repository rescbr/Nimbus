/*Este arquivo deve conter apenas métodos utilizados nas views de tópicos*/

function clickEditTopic(id, topicType)
{
    var divNone; var divBlock;
    if (topicType == "video")
    {
        document.getElementById('divCurrentVideo').style.display = 'none';
        document.getElementById('divEditVideo').style.display = 'block';
    }
    else
        if (topicType == "text" || topicType == "discussion")
    { 
        document.getElementById('divCurrentText').style.display = 'none';
        document.getElementById('divNewText').style.display = 'block';
    }
    else
    if (topicType == "exam") {
        document.getElementById('divCurrentExam').style.display = 'none';
        document.getElementById('divNewExam').style.display = 'block';
    }   
}

function ajaxEditTopic(id, topicType, divOld, divNew) {
    var video;
    var text; var exam;
    var title = "teste";
    var ajaxData = {}

   // var title = document.getElementById("").value;
   // var ImgUrl = document.getElementById("").value;
   // var shortDescription = document.getElementById("").value;

    if (topicType == 'video') {
        video = document.getElementById('iframeNewVideo').src;
    }
    if (topicType == 'text') {
        text = CKEDITOR.instances.txtaArticle.getData();
    }
    if (topicType == 'discussion') {
        text = CKEDITOR.instances.txtaTextMsg.getData();
    }
    if (topicType == 'exam') {
        //TODO editar exam
    }

    if (/*title != "" &&*/ id > 0 && (text != "" || video != "" || exam != "")) {

        ajaxData["Title"] = title;
        ajaxData["Id"] = id;
        ajaxData["ChannelId"] = channelID;
        //ajaxData["ImgUrl"] = ImgUrl;
        ajaxData["Description"] = "TESTANDO AO SOM DE MANANANAMANAAAAAA";// shortDescription;
        ajaxData["TopicType"] = topicType;
        ajaxData["Text"] = text;
        ajaxData["UrlVideo"] = video;
        ajaxData["Question"] = exam;

        $.ajax({
            url: "/api/topic/EditTopic",
            data: JSON.stringify(ajaxData),
            type: "POST",
            contentType: "application/json;charset=utf-8",
            statusCode: {
                200: function (newData) {
                    if (newData != null)
                    {                      
                        if (newData.TopicType == 1)
                        {
                            document.getElementById('iFrameVideo').src = newData.UrlVideo;
                        }
                        document.getElementById(divNew).style.display = 'none';
                        document.getElementById(divOld).style.display = 'block';
                    }
                },

                400: function () {
                    //erro
                    ajaxTopicCallback(newData);
                }
            }
        });

    }

}