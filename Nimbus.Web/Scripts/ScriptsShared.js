
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
    if (global == 'skipPopular')
        value = skipPopular;
    else if (global == 'skipAll')
        value = skipAll;

    $.ajax({
        url: "/api/topic/AbstTopicHtml/" + id +"?viewBy=" + orderBy + "&categoryID=" + category + "&skip=" +value ,
        type: "GET",
        contentType: "application/json;charset=utf-8",
        statusCode: {
            200: function (newData) {

                if (global == 'skipPopular')
                    skipPopular = skipPopular + 1;
                else if (global == 'skipAll')
                    skipAll = skipAll + 1;

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

//método que busca os tópicos de read later
function verMaisReadLater(id, category, global) {
    if (global == 'skipReadLater')
        value = skipReadLater;
 

    $.ajax({
        url: "/api/channel/AbstChannelHtml/" + id + "?viewBy=readLater" + "&categoryID=" + category + "&skip=" + value,
        type: "GET",
        contentType: "application/json;charset=utf-8",
        statusCode: {
            200: function (newData) {

                if (global == 'skipReadLater')
                    skipReadLater = skipReadLater + 1;

                document.getElementById("readLater").innerHTML += newData.Html;

                if (newData.Count < 15) {
                    document.getElementById("btn_" + global).style.display = "none";
                }
            },

            500: function () {
                //erro
                window.alert("Erro ao processar requisição. Tente novamente mais tarde.");
            }
        }
    });
}

//método que busca  as mensagens por paginaçao e/ou enviadas
function viewMessages(global, viewBy, typeBtn) {
    if (global == 'skipMessageSend')
        value = skipMessageSend;
    if (global == 'skipMessageReceived')
        value = skipMessageReceived;

    var btn = document.getElementById('btn_moreMessages');

    if (typeBtn == 'send')
        btn.setAttribute('onclick', 'viewMessages(\'skipMessageSend\', \'messageSend\', \'send\');');
    else
        btn.setAttribute('onclick', 'viewMessages(\'skipMessageReceived\', \'messageReceived\', \'\');');

    $.ajax({
        url: "/api/message/MessagesHtml/?viewBy=" + viewBy + "&skip=" + value,
        type: "GET",
        contentType: "application/json;charset=utf-8",
        statusCode: {
            200: function (newData) {

                if (global == 'skipMessageSend')
                    skipMessageSend = skipMessageSend + 1;
                if (global == 'skipMessageReceived')
                    skipMessageReceived = skipMessageReceived + 1;

                document.getElementById('divSeeMessages').innerHTML += newData.Html;

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
