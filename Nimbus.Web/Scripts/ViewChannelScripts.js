//deve conter nesse scprits, os scripts utilizados  APENAS nas views de CHANNEL

/*Métodos de interações gerais*/
function ajaxFollowChannel(id)
{
    $.ajax({
        url: "/api/channel/FollowChannel/" + id,
        type: "POST",
        contentType: "application/json;charset=utf-8",
        statusCode: {
            200: function (newData) {
                count = parseInt(countFollowers);
                if (newData.Follow == true && newData.Accepted == true) {
                    document.getElementById('pFollow').innerHTML = "Deixar de seguir";
                    document.getElementById('pCountFollowers').innerHTML = (count + 1).toString();
                }
                else if (newData.Follow == true && newData.Accepted == false)
                    document.getElementById('pFollow').innerHTML = "Aguardando aprovação";
                else if (newData.Follow == false) {
                    document.getElementById('pFollow').innerHTML = "Seguir";
                    if (count > 1)
                        count = count - 1;
                    else
                        couunt = 0;
                    document.getElementById('pCountFollowers').innerHTML = count.toString();
                }

            },

            500: function () {
                window.alert("Não foi possível realizar esta operação. Tente novamente mais tarde.");
            }
        }
    });
}

function changeStarVote(element, onmouse)
{  
    var starMouse = element.replace("icoStar_", "");

    if (onmouse == 'over')
    {
        if (voteUser < starMouse) {
            if (voteUser == 0)
                voteUser = 1;

            for (var i = voteUser; i <= starMouse; i++) {
                var img = document.getElementById("icoStar_" + i);
                img.className = "imgStarGreen";
            }
        }
        else if(voteUser > starMouse)
        {
            for (var i =(parseInt(starMouse) + 1); i <= 5; i++) {
                var img = document.getElementById("icoStar_" + i);
                img.className = "imgStarGray";
            }
        }
    }
    else if (onmouse == 'out')
    {
        for (var i = 1; i <= voteUser; i++) {
            var img = document.getElementById("icoStar_" + i);
            img.className = "imgStarGreen";
        }
        for (var i = voteUser + 1; i <= 5; i++) {
            var img = document.getElementById("icoStar_" + i);
            img.className = "imgStarGray";
        }
    }
}

function CreatedDivQuestion()
{
    var nextPerg= parseInt(CurrentQuestion) + 1; //variavel global
    var html = 
    "<div id=\"divPergunta" + nextPerg + "\" class=\"divPergEditarNimbus\">" +
                   "<p>Enunciado da questão:"+
    "<input id=\"QuestionPerg" + nextPerg + "\" class=\"enunciado\" type=\"text\" maxlength=\"600\" />" +
    "</p>"+
    "<p>Respostas:</p>"+
     "<div>"+
         "<ul id=\"ulPerg" + nextPerg + "\">" +
           "<li id=\"liPerg" + nextPerg + "_opt1\">" + //ex: pergunta 1 _ opçao 1
                "<input type=\"radio\" checked class=\"rdbPergEditNimbus\" name=\"radio_perg" + nextPerg + "\" id=\"rdbPerg" + nextPerg + "_opt1\" />" +
                "<input id=\"txtPerg" + nextPerg + "_opt1\" class=\"resposta\"  type=\"text\" onfocus=\"javascript: this.value = ''\" value=\"Opção 1\" />" +
            "</li>"+
            "<li id=\"liPerg" + nextPerg + "_opt2\" >" +
                 "<input type=\"radio\" name=\"radio_perg" + nextPerg + "\" id=\"rdbPerg" + nextPerg + "_opt2\" class=\"fakeDisable rdbPergEditNimbus\" />" +
                 "<input id=\"txtPerg" + nextPerg + "_opt2\"  type=\"text\" class=\"fakeDisable\" onclick=\"DisableOption('2', 'divPergunta" + nextPerg + "');\" value=\"Opção 2\" />" +
             "</li>"+
         "</ul>"+ 
    "</div>"+
"</div>";
    
    $("#divExam").append(html);
    CurrentQuestion = nextPerg;
}

function ajaxDeleteChannel(id, idUser)
{
    $.ajax({
        url: "/api/channel/DeleteChannel/" + id,
        type: "DELETE",
        contentType: "application/json;charset=utf-8",
        statusCode: {
            200: function (newData) {
                if (newData.indexOf('userprofile') > 0)
                {
                    window.location.href = newData;
                }
                else if (newData.indexOf('transferido') > 0) {
                    window.alert(newData);
                    window.location.href = "/userprofile/index/"+ idUser;
                }               
                else {
                    //avisar que nao tem permissao
                    window.alert("Você não possui permissão para deletar este canal.");
                }
            },

            500: function () {
                //erro
                window.alert("Erro ao tentar deletar este canal. Tente novamente mais tarde.");
            }
        }
    });
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
    rdb.setAttribute('class', 'rdbPergEditNimbus');
    var txt = document.getElementById("txtPerg" + indexActive + "_opt" + currentOpt);
    txt.setAttribute('class', 'resposta');
    txt.setAttribute('onclick','');
    txt.value = "";
    
    var name = $("ul#ulPerg" + indexActive + " li:last-child").attr("id");
    name = name.replace("liPerg" + indexActive + "_opt", ""); //retorna o index da opção, ou seja..quantas opçoes ja teve

    var index = parseInt(name) + 1; //index da prox opção a ser inserida
    
    var campo = "<li id=\"liPerg" + indexActive + "_opt" + index + "\" >" +
                      "<input type=\"radio\" name=\"radio\" id=\"rdbPerg" + indexActive + "_opt" + index + "\" class=\"fakeDisable rdbPergEditNimbus\" />" +
                      "<input id=\"txtPerg" + indexActive + "_opt" + index + "\" onclick=\"DisableOption('" + index + "', 'divPergunta" + indexActive + "');\"  type=\"text\" class=\"fakeDisable\" value=\"Opção " + index + "\" />" +
                "</li>";
    
    $("#"+ nameDiv + " ul").append(campo);
}

/*Métodos sobre os tópicos*/
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
        text = $("#txtaArticle").htmlarea('html');
        enumTopicType = 0;
    }
    if (divTipoTopic == "divDiscussion")
    {
        text = $("#txtaTextMsg").htmlarea('html');
        enumTopicType = 2;
    }
    if (divTipoTopic == "divExam") {
        enumTopicType = 3;
        var listQuestion = examEdit();
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

                500: function () {
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

/*Edit, delete, answer commentarios*/
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
                   // var div = document.getElementById("divAnswer_" + commentId);
                   // if (div === null)
                   //    div.style.display = 'none';

                    document.getElementById("divComment_" + commentId).style.display = 'none';
                    //liumpar campos
                    document.getElementById(txtContent).value = '';
                },

                500: function () {
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

                500: function () {
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

                500: function () {
                    //erro
                    window.alert("Não foi possível enviar seu comentário. Tente novamente mais tarde.");
                }
            }
        });
    }

}

/*Mensagens*/
function ajaxSendMessage(id)
{
    ajaxMessage = {};
    var text = $("#txtTextMsg").htmlarea('html');
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

                500: function () {
                    //erro
                    window.alert("Não foi possível enviar sua mensagem. Tente novamente mais tarde.");
                }
            }
        });
    }
}

/*Métodos sobre Moderadores*/
function ajaxLoadModeratorEdit(id)
{
    $.ajax({
        url: "/api/Channel/ShowModeratorsEdit/" + id,
        type: "GET",
        contentType: "application/json;charset=utf-8",
        statusCode: {
            200: function (newData) {
                var listModerador = "";
                var string = "";
                var includeDiv = "";
                var autocomplete = false;

                if (newData.length > 0) {                   
                    //incluir itens na div                    
                    for (var i = 0; i < newData.length; i++) {
                        listModerador += "<div id=\"divModerator_" + newData[i].Id + "\">" +
                                         "<p>" +
                                         "<img src=\"" + newData[i].AvatarUrl + "\" title=\"" + newData[i].FirstName + "\" style=\"height:40px; width:40px;\" />" +
                                         "<label>" + newData[i].FirstName + " " + newData[i].LastName + "</label>" +
                                         "<input id=\"inputUser_" + newData[i].Id + "\" type=\"text\" disabled value=\"" + newData[i].RoleInChannel + "\" />" +
                                         "<div id=\"divUser_" + newData[i].Id + "\"></div>" +
                                         "<img src=\"/images/Utils/edit.png\" onclick=\"ajaxEditModerator(" + id + "," + newData[i].Id + ");\" title=\"Editar\" />" +
                                         "<img src=\"/images/Utils/delete.png\" onclick=\"ajaxDeleteModerator(" + id + "," + newData[i].Id + ");\" title=\"Deletar\" />" +
                                        "</p>" +
                                    "</div>";
                    }                   
                }
                if (newData.length < 5) {
                    //atualiza a global
                    countModerator = newData.length;
                    //coloca o campo de buscar um novo moderador, esse campo é preenchido pelo método acima value=\"Adicionar moderador\" onclick=\"this.value=''\"
                    string = "Nome:<input id=\"search\" type=\"text\"/>" +
                              "<br/>Permissão:"+
                                    "<div id=\"divEditModerator\">" +
                                         "<select id=\"permissionSelect\">" +
                                          "<option value=\"0\">Todas</option>" +
                                          "<option value=\"1\">Moderar mensagens</option>" +
                                          "<option value=\"2\">Moderar moderadores</option>" +
                                          "<option value=\"3\">Moderar tópicos</option>" +
                                          "<option value=\"4\">Moderar usuários</option>" +
                                        "</select>" +
                                    "</div>"+
                           "<button id=\"btnAddModerator\" onclick=\"ajaxNewModerator();\">Adicionar</button>";
                    autocomplete = true;
                }
                else {
                    string = "<p>Você já possui o limite máximo de moderadores aceitos por canal.</p>";
                }
                if (listModerador === "") {
                    includeDiv = "<div id=\"divModalModerators\">Adicione um moderador para seu canal.</div>" +
                                      "<div>" + string + "</div>";
                }
                else
                {                  
                   includeDiv = "<div id=\"divModalModerators\">" + listModerador + "</div>" +
                                   "<div>" + string + "</div>";
                }
                document.getElementById('divEditModerators').innerHTML = includeDiv;
                if (autocomplete) addAutocompleteToSearch();

            },

            500: function () {
                //erro
                window.alert("Erro ao processar requisição. Tente novamente mais tarde.");
            }
        }
    });
}

function addAutocompleteToSearch() {
    /*método q busca os moderadores*/
    $('#search').autocomplete({
        serviceUrl: '/api/user/SearchNewModerador/' + currentChannel,
        paramName: "q",
        onSelect: function (suggestion) {
            newModeratorId = suggestion.data.Id;
        }
    });
}

function AcceptOrNotBeModerator(id, accepted) {    
    $.ajax({
        url: "/api/channel/AcceptOrNotBeModerator/" + id + "?accepted=" + accepted,
        data: JSON.stringify(ajaxMessage),
        type: "POST",
        contentType: "application/json;charset=utf-8",
        statusCode: {
            200: function (newData) {
                if (newData == true) {
                    //TODO COLOCAR O Q ACONTECE NA NOTIFICAÇAO
                }
                else if (newData == false)
                { }
            },

            500: function () {
                //erro
                window.alert("Não foi possível enviar sua mensagem. Tente novamente mais tarde.");
            }
        }
    });
}

function ajaxNewModerator()
{
    var select = document.getElementById("permissionSelect").selectedIndex;
    var option = document.getElementById("permissionSelect").options;
    var permission = option[select].value;

    ajaxData = {};
    id = currentChannel;
    ajaxData["ChannelId"] = id;
    ajaxData["UserId"] = newModeratorId;

    if (permission == "0")
        ajaxData["ChannelMagager"] = true;
    else
        ajaxData["ChannelMagager"] = false;

    if (permission == "1")
        ajaxData["MessageManager"] = true;
    else
        ajaxData["MessageManager"] = false;

    if (permission == "2")
        ajaxData["ModeratorManager"] = true;
    else
        ajaxData["ModeratorManager"] = false;

    if (permission == "3")
        ajaxData["TopicManager"] = true;
    else
        ajaxData["TopicManager"] = false;

    if (permission == "4")
        ajaxData["UserManager"] = true;
    else
        ajaxData["UserManager"] = false;

    if (newModeratorId != null) {
        $.ajax({
            url: "/api/Channel/AddModerator",
            type: "POST",
            data: JSON.stringify(ajaxData),
            contentType: "application/json;charset=utf-8",
            statusCode: {
                200: function (newData) {
                    if (newData.Id > 0) {
                        if (countModerator < 5) {
                            //coloca o campo de buscar um novo moderador, esse campo é preenchido pelo método acima value=\"Adicionar moderador\" onclick=\"this.value=''\"
                            var string = "<br/>Nome:<input id=\"search\" type=\"text\"/>" +
                                      "<br/>Permissão:" +
                                            "<div id=\"divEditModerator\">" +
                                                 "<select id=\"permissionSelect\">" +
                                                  "<option value=\"0\">Todas</option>" +
                                                  "<option value=\"1\">Moderar mensagens</option>" +
                                                  "<option value=\"2\">Moderar moderadores</option>" +
                                                  "<option value=\"3\">Moderar tópicos</option>" +
                                                  "<option value=\"4\">Moderar usuários</option>" +
                                                "</select>" +
                                            "</div>" +
                                   "<button id=\"btnAddModerator\" onclick=\"ajaxNewModerator();\">Adicionar</button>";
                            autocomplete = true;
                        }
                        else {
                            string = "<p>Este canal já possui a quantidade máxima de moderadores permitido.</p>";
                        }

                        var newDivModerator = "<div id=\"divModerator_" + newData.Id + "\">" +
                                             "<p>" +
                                             "<img src=\"" + newData.AvatarUrl + "\" title=\"" + newData.FirstName + "\" style=\"height:40px; width:40px;\" />" +
                                             "<label>" + newData.FirstName + " " + newData.LastName + "</label>" +
                                             "<input id=\"inputUser_" + newData.Id + "\" type=\"text\" disabled value=\"" + newData.RoleInChannel + "\" />" +
                                             "<div id=\"divUser_" + newData.Id + "\"></div>" +
                                             "<img src=\"/images/Utils/edit.png\" onclick=\"ajaxEditModerator(" + id + "," + newData.Id + ");\" title=\"Editar\" />" +
                                             "<img src=\"/images/Utils/delete.png\" onclick=\"ajaxDeleteModerator(" + id + "," + newData.Id + ");\" title=\"Deletar\" />" +
                                            "</p>" +
                                        "</div>" +
                                        "<div id=\"divEditModerator_" + newData.Id + "\">" +
                                        "</div>"+
                                        "<div>"+ string + "</div>";  

                        if (autocomplete) addAutocompleteToSearch();  

                        document.getElementById('divEditModerators').innerHTML = newDivModerator ;

                        newModeretor = "<p>" +
                                         "<a href=\"/userprofile/index/" + newData.Id + "\" class=\"nameMod\">" + newData.FirstName + " " + newData.LastName + "</a>" +
                                       "</p>";
                        document.getElementById('divAllModerators').innerHTML = newModeretor;
                    }
                },

                500: function () {
                    //erro
                    window.alert("Não foi possível realizar esta operação. Tente novamente mais tarde.");
                }
            }
        });
    }

} 

function ajaxEditModerator(id, idUser) {
    /*trocar o input por um select*/
    select =  "<select id=\"newPermissionSelect_"+idUser+"\">" +
                       "<option value=\"0\">Todas</option>" +
                       "<option value=\"1\">Moderar mensagens</option>" +
                       "<option value=\"2\">Moderar moderadores</option>" +
                       "<option value=\"3\">Moderar tópicos</option>" +
                       "<option value=\"4\">Moderar usuários</option>" +
                       "</select>";

    document.getElementById('inputUser_' + idUser).style.display = 'none';
    document.getElementById('divUser_' + idUser).innerHTML = select;
}

function ajaxDeleteModerator(id, idUser)
{
    $.ajax({
        url: "/api/Channel/DeleteModeratorChannel/" + id + "?userID=" + idUser,
        type: "DELETE",
        contentType: "application/json;charset=utf-8",
        statusCode: {
            200: function (newData) {
                if (newData == true) {
                    document.getElementById('divModerator_' + idUser).style.display = "none";
                    countModerator = (countModerator - 1);
                }
            },

            500: function () {
                //erro
                window.alert("Não foi possível realizar esta operação. Tente novamente mais tarde.");
            }
        }
    });
}

/*Métodos sobre tags*/
function ajaxLoadTags(id) { 
    $.ajax({
        url: "/api/Channel/ShowTagChannelEdit/" + id,       
        type: "GET",
        contentType: "application/json;charset=utf-8",
        statusCode: {
            200: function (newData) {
                if (newData.length > 0) {
                    //incluir itens na div
                    var listTag = "";
                    var string ="";
                    for (var i = 0; i < newData.length; i++) {
                        listTag += "<div name=\"nameDivTags\" id=\"divTag_" + newData[i].Id + "\">" +
                                        "<p>#" + newData[i].TagName +
                                           "<input type=\"button\" onclick=\"ajaxdeleteTag(" + newData[i].Id + ", " + id + ");\" value=\"X\"></input>" +
                                        "</p>" +
                                   "</div>";
                    }
                    if (newData.length < 5) {
                        string = "<input id=\"txtNewTag\" type=\"text\" value=\"Nova tag\" onclick=\"this.value=''\" />" +
                                      "<button id=\"btnSaveTag\" onclick=\"ajaxNewTag(" + id + ");\">Adicionar</button>";
                    }
                    else
                    {
                        string = "<p>Você já possui o limite máximo de tags aceitas por canal.</p>";
                    }
                    var includeDiv = "<div id=\"divModalTags\">" + listTag + "</div>" +
                                     "<div id=\"divResultTags\">" + string + "</div>";

                    document.getElementById('divTags').innerHTML = includeDiv;                   
                }
            },

            500: function () {
                //erro
                window.alert("Erro ao processar requisição. Tente novamente mais tarde.");
            }
        }
    });
}

function ajaxNewTag(id) {
    var txt = document.getElementById('txtNewTag').value;
    if (txt != null && txt != "")
    {
      
        $.ajax({
            url: "/api/Channel/AddTagsChannel/" + id + "?tag=" + encodeURIComponent(txt),
            type: "POST",
            contentType: "application/json;charset=utf-8",
            statusCode: {
                200: function (newData) {
                    if (newData.Id > 0) {
                        var tagInfo = "<label id=\"lblTag_" + newData.Id + "\">" + newData.TagName + "</label>";
                        document.getElementById('tagChannel').innerHTML += tagInfo;
                        
                        var newTag = "<div name=\"nameDivTags\" id=\"divTag_" + newData.Id + "\">" +
                                        "<p>#" + newData.TagName +
                                           "<input type=\"button\" onclick=\"ajaxdeleteTag(" + newData.Id + ", " + id + ");\" value=\"X\"></input>" +
                                        "</p>" +
                                   "</div>";
                        document.getElementById('divModalTags').innerHTML += newTag;

                        var divTags = document.getElementsByName('nameDivTags');
                        if (divTags.length >= 5)
                        {
                            document.getElementById('divResultTags').innerHTML = '';
                        }
                    }
                },

                500: function () {
                    //erro
                    window.alert("Não foi possível realizar esta operação. Tente novamente mais tarde.");
                }
            }
        });
    }
}

function ajaxdeleteTag(idTag, id)
{
    $.ajax({
        url: "/api/Channel/DeleteTagChannel/" + id + "?tagID=" + idTag,
        type: "DELETE",
        contentType: "application/json;charset=utf-8",
        statusCode: {
            200: function (newData) {
                if (newData.isDelete == true) {
                    document.getElementById('divTag_' + idTag).style.display = "none";
                    document.getElementById("lblTag_" + idTag).style.display = "none";

                    if (newData.Count < 5) {
                        string = "<input id=\"txtNewTag\" type=\"text\" value=\"Nova tag\" onclick=\"this.value=''\" />" +
                                   "<button id=\"btnSaveTag\" onclick=\"ajaxNewTag(" + id + ");\">Adicionar</button>";

                        document.getElementById('divResultTags').innerHTML = '';
                        document.getElementById('divResultTags').innerHTML = string;
                    }
                }
            },

            500: function () {
                //erro
                window.alert("Não foi possível realizar esta operação. Tente novamente mais tarde.");
            }
        }
    });
}

/*Métodos gerais de edição*/
function ajaxLoadEditInfo(id, isOwner)
{
    if (roles.indexOf("channelmanager") > -1 || isOwner == true)
        ajaxLoadTags(id);

    if (roles.indexOf("moderatormanager") > -1|| roles.indexOf("channelmanager") > -1 || isOwner == true )
       ajaxLoadModeratorEdit(id);  
}

function ajaxSaveAllEdit(id)
{
    //tags e novos moderadores -> são salvas assim que são criadas
    var success = false;
    //alterou somente a permissao
    var obj = $("select[id*='newPermissionSelect_']");

    if (obj.length > 0) {
        for (var i = 0; i < obj.length; i++) {
            idUser = obj[i].id.replace("newPermissionSelect_", "");

            var select = obj[i].selectedIndex;
            var option = obj[i].options;
            var permission = option[select].value;
            $.ajax({
                url: "/api/Channel/EditPermissionModerator/" + id + "?userId=" + idUser + "&permission=" + permission,
                type: "POST",
                contentType: "application/json;charset=utf-8",
                statusCode: {
                    200: function (newData) {
                        success = true;
                    },

                    500: function () {
                        //erro
                        window.alert("Não foi possível realizar esta operação. Tente novamente mais tarde.");
                        success = false;
                    }
                }
            });
        }
    }
    else {
        success = true;
    }
    //salvar nome
    if (success == true) {
        title = document.getElementById('txtEditTitle').value;
        ajaxData = {};
        ajaxData['Name'] = title;
        ajaxData['Id'] = id;
        if(document.getElementsByName('openComment')[0].isChecked)
            ajaxData['OpenToComments'] = true;
        if (document.getElementsByName('openComment')[1].isChecked)
            ajaxData['OpenToComments'] = false;
        
        var category = document.getElementById('slcCategory');
        var select = category.selectedIndex;
        var option = category.options;
        var categoryId = option[select].value;

        ajaxData['CategoryId'] = categoryId;
        if (title != null && title != "") {
            $.ajax({
                url: "/api/Channel/EditChannel",
                type: "POST",
                data: JSON.stringify(ajaxData),
                contentType: "application/json;charset=utf-8",
                statusCode: {
                    200: function (newData) {
                        document.getElementById('closeModalEdit').click();
                        document.getElementById('hChannelName').innerHTML = newData.Title;
                        document.getElementById('imgCapa').src = newData.ImgUrl;
                    },

                    500: function () {
                        //erro
                        window.alert("Não foi possível realizar esta operação. Tente novamente mais tarde.");
                        success = false;
                    }
                }
            });
        }
    }
}

function ajaxVoteChannel(id, vote)
{
    $.ajax({
        url: "/api/Channel/VoteChannel/" + id + "?vote=" + vote,
        type: "POST",
        contentType: "application/json;charset=utf-8",
        statusCode: {
            200: function (newData) {
                if (newData > 0) { //deu certo
                    //retornar as estrelinhas                  
                    for (var s = 1; s <= vote; s++) {
                        var img = document.getElementById("icoStar_" + s);
                        img.className = "imgStarGreen";
                    }
                    for (var i = vote; i <= 5; i++) {
                        var img = document.getElementById("icoStar_" + i);
                        img.className = "imgStarGray";
                    }

                    //mudar nota do channel
                    document.getElementById('countVtsChannel').innerHTML = newData;
                }
            },
            400: function () {
                //erro
                window.alert("Não foi possível realizar esta operação. Tente novamente mais tarde.");
            }
        }
    });
}

function ajaxReportChannel(idUserReporter, idUserReported, idChannel) {

    var text = document.getElementById('txtJustificativa').value;

    if (text.replace(" ", "") != '' && text.length > 10) {
        ajaxData = {};
        ajaxData['Justification'] = text;
        ajaxData['UserReportedId'] = idUserReported; //foi reportado
        ajaxData['UserReporterId'] = idUserReporter;
        ajaxData['ChannelReportedId'] = idChannel;

        $.ajax({
            url: "/api/report/ReportChannel/",
            data: JSON.stringify(ajaxData),
            type: "POST",
            contentType: "application/json;charset=utf-8",
            statusCode: {
                200: function (newData) {
                    if (newData.Id > 0) {
                        window.alert("Operação realizada com sucesso!");
                        document.getElementById('txtJustificativa').value = '';
                    }
                },

                500: function () {
                    //erro
                    window.alert("Não foi possível realizar esta operação. Tente novamente mais tarde.");
                }
            }
        });

    }
    else {
        document.getElementById('lblAvisoJustificativa').innerHTML = "* Campo obrigatório! Verifique se sua mensagem contém pelo menos 10 caracteres.";
    }
}