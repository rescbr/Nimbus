
/*Método para mudar divs dentro de outra div que já esta sendo alterada pela funçao changediv
  usada na pagina de channl -> novo tópico*/
function EnableDiv(newDiv, tipoGlobal) { 

    if (tipoGlobal == 'currentDiv')
    {
        document.getElementById(currentDiv).style.display = 'none';
        currentDiv = newDiv;
    }
    else if (tipoGlobal == 'divTipoTopic')
    {
        if (divTipoTopic != '')
        {
            document.getElementById(divTipoTopic).style.display = 'none';
        }
        divTipoTopic = newDiv;
    }
    
    document.getElementById(newDiv).style.display = 'block';
}

function EnableTwoDiv(newDiv, tipoGlobal, divTwo)
{
    document.getElementById(divTwo).style.display = 'block';
    EnableDiv(newDiv, tipoGlobal);   
    
}

function EnableDivHiddenBtn(nameDiv, nameBtn)
{
    document.getElementById(nameDiv).style.display = 'block';
    document.getElementById(nameBtn).style.display = 'none';
}

function DisabledBtn(nameButton)
{
    var btn = document.getElementById(nameButton);
    if (btn.disabled == true) {
        btn.disabled = false;
    }
    else {
        btn.disabled = true;
    }
}

//método que coloca url para visualização do video antes de salvar
//param: nome do campo que vai ter a string do video, nome da div do frame que vai receber a string como src
function getUrlVideo(nomeCampo, nomeDiv, nomeFrame) {
    var text = document.getElementById(nomeCampo).value;
    var queryurl;
    if (text.indexOf("/embed") <= 0) {
        if (text.length >= 11) {
            if (text.indexOf("youtube.be/") > 0) {
                var params = text.search.substr(text.search.indexOf("be/") + 3);
                queryurl = "//www.youtube.com/embed/" + params;
            }
            else
                if (text.indexOf("youtube.com")) {
                    var params = text.search("v=");
                    params = text.slice(params + 2);
                    queryurl = "//www.youtube.com/embed/" + params;
                }

        }
    }
    else {
        if (text.indexOf("http:") > 0) {
            params = text.slice(params + 5);
            queryurl = params;
        }
    }

    if (queryurl.length > 34) {
        document.getElementById(nomeFrame).src = queryurl;
        document.getElementById(nomeDiv).style.display = 'block';
    }  

}

//Mudar de Lista pra Grade e Grade pra lista
function ChangeTo(tipoPost, tipoAtual)
{
    if (tipoPost == tipoAtual) {
        //nao faz nada
    }
    else if (tipoPost == 'post_lista') {
        document.getElementById("lista").attr("class-antiga","class-nova");

    }
    else {

    }
    

}

//método que busca os tópicos de um canal
function verMaisTopics(id, orderBy, category, global)
{
    var type = "abstopic";
    if (global == 'skipPopular')
        value = skipPopular;
    else if (global == 'skipAll')
        value = skipAll;
    else if (global == 'skipReadLater') {
        value = skipReadLater;
        type = "marcado";
    }

    $.ajax({
        url: "/api/topic/AbstTopicHtml/" + id +"?viewBy=" + orderBy + "&categoryID=" + category + "&skip=" +value + "&type=" + type,
        type: "GET",
        contentType: "application/json;charset=utf-8",
        statusCode: {
            200: function (newData) {

                if (global == 'skipPopular')
                    skipPopular = skipPopular + 1;
                else if (global == 'skipAll')
                    skipAll = skipAll + 1;
                else if (global == 'skipReadLater')
                    skipReadLater = skipReadLater + 1;

                document.getElementById(orderBy).innerHTML += newData.Html;

                if (newData.Count < 15) {
                    document.getElementById("btn_" + global).style.display = "none";
                }
            },

            500: function () {
                //erro
                window.alert("Erro ao obter mais tópicos. Tente novamente mais tarde.");
            }
        }
    });
}

//método que busca os comentarios de 15 em 15 e 'filhos' de 5 em 5
function seeMoreComments(id, nameDiv, global, page) {
       
    var limite = 0;
    var value = -1;
    if (global == 'skipComments')
        value = skipComments;
    else if (global == 'skipCommentsChild')
        value = skipCommentsChild;

    if (page == 'topic') {
        limite = 15;
    }
    else if (page == 'channel') {
        limite = 5;
    }
    else if (page == 'child')
    {
        limite = 3;
    }

    var stringUrl = "/api/comment/CommentsHtml/" + id + "?skip=" + value + "&type=" + page;

        $.ajax({
            url: stringUrl,
            type: "GET",
            contentType: "application/json;charset=utf-8",
            statusCode: {
                200: function (newData) {
                    if (newData.Count > 0) {
                            if (global == 'skipComments')
                                skipComments = skipComments + 1;
                            else if (global == 'skipCommentsChild')
                                skipCommentsChild = skipCommentsChild + 1;

                            document.getElementById(nameDiv).innerHTML += newData.Html;
                        }
                        if (newData.Count < limite) {
                            document.getElementById("btn_" + global).style.display = "none";
                        }
                  
                },
                500: function () {
                    //erro
                    window.alert("Erro ao carregar mais comentários. Tente novamente mais tarde.");
                }
            }
        });
    
}

//método que busca canais de 15 em 15
function verMaisChannels(id, orderBy, category, global) {
    if (global == 'skipMyChannels')
        value = skipMyChannels;
    if (global == 'skipChannelsFollow')
        value = skipChannelsFollow;

    $.ajax({
        url: "/api/channel/AbstChannelHtml/" + id + "?viewBy=" + orderBy + "&categoryID=" + category + "&skip=" + value,
        type: "GET",
        contentType: "application/json;charset=utf-8",
        statusCode: {
            200: function (newData) {

                if (global == 'skipMyChannels')
                    skipMyChannels = skipMyChannels + 1;
                if (global == 'skipChannelsFollow')
                    skipChannelsFollow = skipChannelsFollow + 1;

                document.getElementById(orderBy).innerHTML += newData.Html;

                if (newData.Count < 15) {
                    document.getElementById("btn_" + global).style.display = "none";
                }
            },

            500: function () {
                //erro
                window.alert("Erro ao obter mais canais. Tente novamente mais tarde.");
            }
        }
    });
}


//método que busca  as mensagens por paginaçao e/ou enviadas
function viewMessages(global, viewBy, typeBtn, typeClick) {
    var divB; var divN;
    if (global == 'skipMessageSend') {
        value = skipMessageSend;
        divB = 'divSeeMessagesSend';
        divN = 'divSeeMessages';
        if (countSend < 15)
            btnStyle = 'none';
        else btnStyle = 'block';
        document.getElementById('liMsgSend').onclick = function () { viewMessages('skipMessageSend', 'messageSend', 'send', 'back'); }
    }
    if (global == 'skipMessageReceived') {
        value = skipMessageReceived;       
        divN = 'divSeeMessagesSend';
        divB = 'divSeeMessages';
        if (countReceived < 15)
            btnStyle = 'none';
        else btnStyle = 'block';
        document.getElementById('liMsgReceived').onclick = function () { viewMessages('skipMessageReceived', 'messageReceived', 'received', 'back'); }
    }

    var btn = document.getElementById('btn_moreMessages');

    if (typeBtn == 'send')
        btn.onclick = function(){ viewMessages('skipMessageSend', 'messageSend', 'send','seeMore');}
    else
        btn.onclick = function () { viewMessages('skipMessageReceived', 'messageReceived', '', 'seeMore'); }

    if (typeClick == 'seeMore' || typeClick == 'firstGetSend') {
        $.ajax({
            url: "/api/message/MessagesHtml/?viewBy=" + viewBy + "&skip=" + value,
            type: "GET",
            contentType: "application/json;charset=utf-8",
            statusCode: {
                200: function (newData) {
                    if (global == 'skipMessageSend') {
                        skipMessageSend = skipMessageSend + 1;
                        divB = "divSeeMessagesSend";
                        divN = "divSeeMessages";
                        countSend = newData.Count;
                    }
                    else
                        if (global == 'skipMessageReceived') {
                            skipMessageReceived = skipMessageReceived + 1;
                            divN = "divSeeMessagesSend";
                            divB = "divSeeMessages";
                            countReceived = newData.Count;
                        }

                    document.getElementById(divB).style.display = 'block';
                    document.getElementById(divN).style.display = 'none';

                    document.getElementById(divB).innerHTML += newData.Html;

                    if (newData.Count < 15) {
                        btn.style.display = "none";
                    }
                    else {
                        btn.style.display = "block";
                    }
                },

                500: function () {
                    //erro
                    window.alert("Erro ao processar requisição. Tente novamente mais tarde.");
                }
            }
        });
    }
    else if(typeClick == 'back')
    {
        document.getElementById(divB).style.display = 'block';
        document.getElementById(divN).style.display = 'none';
        btn.style.display = btnStyle;
    }
}

function ajaxSeeMsg(id, starNameDiv)
{
    var divNew = document.getElementById('divMesgExpand_' + id);
    var divOld = document.getElementById('divMesg_' + id).style.background = "rgb(190, 30, 45)";
    if (divNew != null)
    {
        divNew.style.display = 'block';
    }
    else {
        $.ajax({
            url: "/api/message/MessageExpandHtml/" + id,
            type: "GET",
            contentType: "application/json;charset=utf-8",
            statusCode: {
                200: function (newData) {
                    var div = document.getElementById(starNameDiv + id);

                    div.parentElement.innerHTML += newData.Html;
                },

                500: function () {
                    //erro
                    window.alert("Erro abrir sua mensagem. Tente novamente mais tarde.");
                }
            }
        });
    }
}

function ajaxHiddeMsg(div)
{
    document.getElementById(div).style.display = 'none';
}

function openDivSearch()
{  
    document.getElementById('divBarSearch').style.display = 'block';
    document.getElementById('aOpenDivSearch').className = "BTopBarLinkSelected";
    document.getElementById('linkTop').className = "BTop";
    document.getElementById('linkTrending').className = "BTrTop";
}

function ajaxSearch(campo)
{
    var text = document.getElementById('txtFieldSearch' + campo);
    text = text.value;

    if (text != '' && text != null) {
        var select = document.getElementById("slcFilterSearch" + campo).selectedIndex;
        var option = document.getElementById("slcFilterSearch" + campo).options;
        var typeSearch = option[select].value;

        var stringUrl;
        if (typeSearch == '0') {
            window.location.href = "/search/index/?text=" + text + "&filter=0";
        }
        else if (typeSearch == '1') {
            window.location.href = "/search/index/?text=" + text + "&filter=1";
        }
        else if (typeSearch == '2') {
            window.location.href = "/search/index/?text=" + text + "&filter=2";
        }
        else if (typeSearch == '3') {
            window.location.href = "/search/index/?text=" + text + "&filter=3";
        }
    }

}
