//deve conter nesse scprits, os scripts utilizados  APENAS nas views de CHANNEL

function SaveNewTopic(channelID, isEdit)
{
    if (isEdit == false)
    {
        ajaxSaveNewTopic(channelID);
    }
    else
    {
        ajaxEditTopic(channelID);
    }
}

function ajaxSaveNewTopic(channelID)
{
    var video; 
    var text; var exam;
    var enumTopicType;
    var ajaxData = {}
    var title = document.getElementById("txtNameTopic").value;
    var ImgUrl = document.getElementById("url").value;
    var shortDescription = document.getElementById("txtaDescription").value;

    if (divTipoTopic == "divVideo")
    {
        video = document.getElementById('iframeVideo').src;
        enumTopicType = 1;

    }
    if (divTipoTopic == "divText")
    {
        text = CKEDITOR.instances.txtaArticle.getData();
        enumTopicType = 0;
    }
    if (divTipoTopic == "divDiscussion")
    {
        text = CKEDITOR.instances.txtaTextMsg.getData();
        enumTopicType = 2;
    }
    if (divTipoTopic == "divExam")
    {
        enumTopicType = 3;

        //TODO
    }
    if (title != "" && shortDescription != "" && channelID > 0 && (text != "" || video != "" || exam != ""))
    {
        ajaxData["Title"] = title;
        ajaxData["ImgUrl"] = ImgUrl;
        ajaxData["Description"] = shortDescription;
        ajaxData["ChannelId"] = channelID;
        ajaxData["TopicType"] = enumTopicType;
        ajaxData["Text"] = text;
        ajaxData["UrlVideo"] = video;
        ajaxData["Question"] = exam;

        $.ajax({
            url: "/api/topic/NewTopic",
            data: JSON.stringify(ajaxData),
            type: "POST",
            contentType: "application/json;charset=utf-8",
            statusCode: {
                200: function (newData) {
                    ajaxTopicCallback(newData);
                    //liumpar campos
                },

                400: function () {
                    //erro
                    ajaxTopicCallback(newData);
                }
            }
        });
        
    }
    
}


function ajaxEditTopic()
{
    var video;
    var text; var exam;
    var ajaxData = {}
    var title = document.getElementById("").value;
    var ImgUrl = document.getElementById("").value;
    var shortDescription = document.getElementById("").value;
    var topicId = document.getElementById("").value;
    var enumTopicType = document.getElementById(""), value;
    var price = document.getElementById().value;

    if (enumTopicType == 1) {
        video = document.getElementById('iframeVideo').src;
    }
    if (enumTopicType == 0) {
        text = CKEDITOR.instances.txtaArticle.getData();
    }
    if (enumTopicType == 2) {
        text = CKEDITOR.instances.txtaTextMsg.getData();
    }
    if (enumTopicType == 3) {
        //TODO
    }
        
    if (title != "" && shortDescription != "" && topicId > 0 && (text != "" || video != "" || exam != "")) {
        ajaxData["Title"] = title;
        ajaxData["ImgUrl"] = ImgUrl;
        ajaxData["Description"] = shortDescription;
        ajaxData["ChannelId"] = channelID;
        ajaxData["TopicType"] = enumTopicType;
        ajaxData["Text"] = text;
        ajaxData["UrlVideo"] = video;
        ajaxData["Question"] = exam;
        ajaxData["Price"] = price;
        ajaxData["Id"] = topicId;

        $.ajax({
            url: "/api/topic/EditTopic",
            data: JSON.stringify(ajaxData),
            type: "POST",
            contentType: "application/json;charset=utf-8",
            statusCode: {
                200: function (newData) {
                    ajaxTopicCallback(newData);
                    //liumpar campos
                },

                400: function () {
                    //erro
                    ajaxTopicCallback(newData);
                }
            }
        });

    }

}

function ajaxTopicCallback(response) {
    if (response.message) {
        alert(response.message);        
    } else {
        window.location.href = "/topic/index/" + response.Id; //
    }
}

function ajaxAnswerComment(commentId, channelId, topicId) {
    
    var ajaxData = {}
    var text = document.getElementById("txtaAnswer").value;

  
    if (text != "") {
        ajaxData["ParentId"] = commentId;
        ajaxData["Text"] = text;
        ajaxData["TopicId"] = topicId;
        ajaxData["ChannelId"] = channelId;

        $.ajax({
            url: "/api/comment/AnswerComment",
            data: JSON.stringify(ajaxData),
            type: "POST",
            contentType: "application/json;charset=utf-8",
            statusCode: {
                200: function (newData) {
                    document.getElementById("divAnswer_" + commentId).style.display = 'none';
                    document.getElementById("divComment").style.display = 'none';
                    //liumpar campos
                },

                400: function () {
                    //erro
                    window.alert("Não foi possível enviar seu comentário. Tente novamente mais tarde.");
                }
            }
        });

    }

}
