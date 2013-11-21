/*Este arquivo deve conter apenas métodos utilizados nas views de tópicos*/

function clickEditTopic(id, topicType)
{
    if (topicType == "video")
    {
        document.getElementById('divCurrent_'+topicType).style.display = 'none';
    }
    else
        if (topicType == "text" || topicType == "discussion")
    { 
            document.getElementById('divCurrent_'+topicType).style.display = 'none';
    }
    else
    if (topicType == "exam") {
        document.getElementById('divCurrent_'+topicType).style.display = 'none';
    }
    document.getElementById('divRenderEdit').style.display = 'block';
}

function ajaxEditTopic(id, topicType, divOld) {
    var video;
    var text; var exam;
    var ajaxData = {}  

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

    title = document.getElementById('iptNewTitle').value;

    if (title != "" && id > 0 && (text != "" || video != "" || exam != "")) {

        ajaxData["Title"] = title
        ajaxData["Id"] = id;
        ajaxData["ChannelId"] = channelID;
        ajaxData["ImgUrl"] = document.getElementById('imgNewPrevia').src;
        ajaxData["Description"] = document.getElementById('iptNewDescription').value;
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
                    /* 0 = text, 1= video, 2=discussion, 3=exam*/
                    if (newData != null)
                    {                      
                        if (newData.TopicType == 1)
                        {
                            document.getElementById('iFrameVideo').src = newData.UrlVideo;
                        }
                        else if (newData.topicType == 0 || newData.topicType == 2)
                        {
                            document.getElementById('pTopicText').innerHTML = newData.Text;
                        }
                        else if (newData.topicType == 3) {
                            //TODO
                        }

                        document.getElementById('iptNewTitle').value = newData.Title;
                        document.getElementById('imgNewPrevia').src = newData.ImgUrl;
                        document.getElementById('iptNewDescription').value = newData.Description;

                        document.getElementById('divRenderEdit').style.display = 'none';
                        document.getElementById(divOld).style.display = 'block';
                    }
                },

                400: function () {
                    //erro
                    window.alert("Não foi possível enviar seu comentário. Tente novamente mais tarde.");
                }
            }
        });

    }

}