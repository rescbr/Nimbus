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
    var text; var exam = null;
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
            200: function (newData)
            {
                ajaxTopicCallback(newData);
                //liumpar campos
            },

            400: function ()
            {
                //erro
                ajaxTopicCallback(newData);
            }
        }
    });
}


function ajaxEditTopic(channelID)
{ }

function ajaxTopicCallback(response) {
    if (response.message) {
        alert(response.message);        
    } else {
        window.location.href = "/topic/index/" + response.Id; //
    }
}