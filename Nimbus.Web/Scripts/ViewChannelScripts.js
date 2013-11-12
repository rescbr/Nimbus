//deve conter nesse scprits, os scripts utilizados  APENAS nas views de CHANNEL

function CreatedDivQuestion()
{
    var nextPerg= parseInt(CurrentQuestion) + 1; //variavel global
    var html = 
    "<div id=\"divPergunta" + nextPerg + "\">" +
                   "<p>Enunciado da questão:"+
    "<input id=\"QuestionPerg" + nextPerg + "\" name=\"enunciado\" type=\"text\" maxlength=\"600\" />" +
    "</p>"+
    "<p>Respostas:</p>"+
     "<div>"+
         "<ul id=\"ulPerg" + nextPerg + "\">" +
           "<li id=\"liPerg" + nextPerg + "_opt1\">" + //ex: pergunta 1 _ opçao 1
                "<input type=\"radio\" name=\"radio\" id=\"rdbPerg" + nextPerg + "_opt1\" />" +
                "<input id=\"txtPerg" + nextPerg + "_opt1\" name=\"resposta\" type=\"text\" onfocus=\"javascript: this.value = ''\" value=\"Opção 1\" />" +
            "</li>"+
            "<li id=\"liPerg" + nextPerg + "_opt2\" onclick=\"DisableOption('2', 'divPergunta" + nextPerg + "');\">" +
                 "<input type=\"radio\" name=\"radio\" id=\"rdbPerg" + nextPerg + "_opt2\" disabled=\"disabled\" />" +
                 "<input id=\"txtPerg" + nextPerg + "_opt2\" name=\"resposta\" type=\"text\" disabled=\"disabled\" onfocus=\"javascript: this.value = ''\" value=\"Opção 2\" />" +
             "</li>"+
         "</ul>"+ 
    "</div>"+
"</div>";
    
    $("#divExam").append(html);
    CurrentQuestion = nextPerg;
}

function DisableOption(currentOpt, nameDiv)
{
    
    var indexPerg = parseInt(nameDiv.replace("divPergunta", ""));
    var indexActive
    if (indexPerg != CurrentQuestion)
        indexActive = indexPerg;
    else
        indexActive = CurrentQuestion

    var rdb = document.getElementById("rdbPerg" + indexActive + "_opt" + currentOpt);//ex: rdbPerg1_opt2
    rdb.removeAttribute('disabled');
    var txt = document.getElementById("txtPerg" + indexActive + "_opt" + currentOpt);
    txt.removeAttribute('disabled');
    document.getElementById("liPerg" + indexActive + "_opt" + currentOpt).removeAttribute("onClick");

    
    var name = $("ul#ulPerg" + indexActive + " li:last-child").attr("id");
    name = name.replace("liPerg" + indexActive + "_opt", ""); //retorna o index da opção, ou seja..quantas opçoes ja teve

    var index = parseInt(name) + 1; //index da prox opção a ser inserida
    
    var campo = "<li id=\"liPerg" + indexActive + "_opt" + index + "\" onclick=\"DisableOption('" + index + "', 'divPergunta" + indexActive + "');\">" +
                      "<input type=\"radio\" name=\"radio\" id=\"rdbPerg" + indexActive + "_opt" + index + "\" disabled=\"disabled\" />" +
                      "<input id=\"txtPerg" + indexActive + "_opt" + index + "\" name=\"resposta\" type=\"text\" disabled=\"disabled\" onfocus=\"javascript: this.value = ''\" value=\"Opção " + index + "\" />" +
                "</li>";
    
    $("#"+ nameDiv + " ul").append(campo);
}


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
    if (divTipoTopic == "divExam") {
        enumTopicType = 3;

        //TODO
        var questionData = {}
        var listQuestion = []
        var listPerg = document.getElementsByName('enunciado');
        var isChecked = false;
        for (perg = 0; perg < listPerg.length; perg++) //para cada pergunta
        {
            var item = listPerg[perg];
            questionData["TextQuestion"] = item.value; //pego o conteudo = enunciado   

            var currentPerg = item.id.replace("QuestionPerg",""); //pega o indice da pergunta atual

            var listOpt =[]; 
            var dictionary = new Object();

            $("#divExam li").each(function() {//pego todas as opções
                var id = this.id;                
                if (id.indexOf("liPerg") > -1)
                {
                    listOpt.push(id);
                }
            });

            for (option = 0; option < listOpt.length; option++)
            {
                var rdbOption = document.getElementById('rdbPerg'+currentPerg+'_opt'+ (option + 1));
                var txtOption = document.getElementById('txtPerg' + currentPerg + '_opt' + (option + 1));

                if (txtOption.value != "Opção " + (option + 1) && txtOption.value != "")
                {
                    if (rdbOption.checked == true) {
                        questionData["CorrectAnswer"] = option + 1; //passa o indice da resposta certa + 1 .'. o for começa do zero                    
                        isChecked = true;
                    }
                    dictionary[option + 1] = txtOption.value; //dictionary<int, string>

                    questionData["ChoicesAnswer"] = dictionary;
                }
            }
            listQuestion.push(questionData);
        }
    }

    if (title != "" && shortDescription != "" && channelID > 0 && (text != "" || video != "" || (listQuestion.length > 0 && isChecked == true)))
    {
        ajaxData["Title"] = title;
        ajaxData["ImgUrl"] = ImgUrl;
        ajaxData["Description"] = shortDescription;
        ajaxData["ChannelId"] = channelID;
        ajaxData["TopicType"] = enumTopicType;
        ajaxData["Text"] = text;
        ajaxData["UrlVideo"] = video;
        ajaxData["Question"] = listQuestion;

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

function ajaxAnswerComment(parentId,commentId, channelId, topicId, txtContent) {
    
    var ajaxData = {}
    var text = document.getElementById(txtContent).value;

  
    if (text != "") {
        if (parentId > 0)
            ajaxData["ParentId"] = parentId;
        else
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
                    var div = document.getElementById("divAnswer_" + commentId);
                    if (div === null)
                        div.style.display = 'none';

                    document.getElementById("divComment_" + commentId).style.display = 'none';
                    //liumpar campos
                    document.getElementById(txtContent).value = '';
                },

                400: function () {
                    //erro
                    window.alert("Não foi possível enviar seu comentário. Tente novamente mais tarde.");
                }
            }
        });

    }

}

function ajaxSaveNewComment(topicId, channelId, txtContent)
{
    var ajaxData = {}
    var text = document.getElementById(txtContent).value;


    if (text != "" ) {        
        ajaxData["Text"] = text;
        ajaxData["TopicId"] = topicId;
        ajaxData["ChannelId"] = channelId;

        $.ajax({
            url: "/api/comment/NewComment",
            data: JSON.stringify(ajaxData),
            type: "POST",
            contentType: "application/json;charset=utf-8",
            statusCode: {
                200: function (newData) {
                    document.getElementById("txtacomentario").value = '';
                },

                400: function () {
                    //erro
                    window.alert("Não foi possível enviar seu comentário. Tente novamente mais tarde.");
                }
            }
        });
    }

}

function ajaxDeleteComment(commentId, divName)
{
    var ajaxData = {}

    if (commentId != 0) {
        ajaxData["Id"] = commentId;
        $.ajax({
            url: "/api/comment/DeleteComment",
            data: JSON.stringify(ajaxData),
            type: "DELETE",
            contentType: "application/json;charset=utf-8",
            statusCode: {
                200: function (newData) {
                    if (newData.ParentId > 0)
                    {
                        document.getElementById("lblText_"+ commentId).value = newData.Text;
                        document.getElementById("lblPostedOn_" + commentId).value = newData.PostedOn;
                        document.getElementById("imgTopic_"+ commentId).src = newData.AvatarUrl;
                        document.getElementById("lblUserName_" + commentId).value = newData.UserName;
                        document.getElementById("btnDelete_"+ commentId).style.display = 'none';
                    }
                    else
                    {
                        document.getElementById("divContentComment_" + commentId).style.display = 'none';
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

function ajaxSendMessage(id)
{
    ajaxMessage = {};
    var text = CKEDITOR.instances.txtTextMsg.getData();
    var title = document.getElementById('txtTitleMsg').value;

    if (text != "") {
        ajaxMessage["Text"] = text;
        ajaxMessage["ChannelId"] = id;
        if (title != "")
            ajaxMessage["Title"] = title;
        else
            ajaxMessage["Title"] = "Sem assunto";

        $.ajax({
            url: "/api/Message/SendMessageChannel/" + id, 
            data: JSON.stringify(ajaxMessage),
            type: "POST",
            contentType: "application/json;charset=utf-8",
            statusCode: {
                200: function (newData) {

                    if (newData.Id > 0) {
                        //fechar modal
                        document.getElementById('closeModal').click();
                        //limpar campos
                        text.value = "";
                        title.value = "";
                        //aviso
                        window.alert("Mensagem enviada com sucesso.");
                    }
                },

                400: function () {
                    //erro
                    window.alert("Não foi possível enviar sua mensagem. Tente novamente mais tarde.");
                }
            }
        });
    }
}

function ajaxLoadTags(id) {
    //para cada tag colocar um x delete
    //incluir na div divTags
    //salvar nova tag
    //abrir o modal-tags
    $.ajax({
        url: "/api/Channel/ShowTagChannelEdit/" + id,       
        type: "GET",
        contentType: "application/json;charset=utf-8",
        statusCode: {
            200: function (newData) {
                if (newData.length > 0) {
                    //incluir itens na div
                    var listTag = [];
                    var string ="";
                    for (var i = 0; i < newData.length; i++) {
                        listTag[i]= "<p>#" + newData[i].TagName +
                                        "<a href=\"#\" onclick=\"ajaxdeleteTag("+ newData[i].Id +", "+id+");\">X</a>" +
                                    "</p>";
                    }
                    if (newData.length < 5) {
                        string = "<input id=\"txtNewTag\" type=\"text\" value=\"Nova tag\" onclick=\"this.value=''\" />" +
                                      "<button id=\"btnSaveTag\" onclick=\"ajaxNewTag(" + id + ");\">Adicionar</button>";
                    }
                    else
                    {
                        string = "<p>Você já possui o limite máximo de tags aceitas por canal.</p>";
                    }
                    var includeDiv = "<div>" + listTag + "</div>" +
                                     "<div>" + string + "</div>";

                    document.getElementById('divTags').innerHTML = includeDiv;
                    
                }
            },

            400: function () {
                //erro
                window.alert("Erro ao processar requisição. Tente novamente mais tarde.");
            }
        }
    });

}

function ajaxEditTag(idChannel)
{
}

function ajaxdeleteTag(idTag, idChannel)
{
}