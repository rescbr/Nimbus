
/*Método para mudar divs dentro de outra div que já esta sendo alterada pela funçao changediv
  usada na pagina de channl -> novo tópico*/

function PositionFooter(evt) {
    var footer = document.getElementById("footer");
    var windowHeight = document.getElementsByTagName("html")[0].clientHeight; //apenas viewport
    var headerHeight = document.getElementById("header").offsetHeight;
    var containerHeight = document.getElementById("container-body").scrollHeight; //inclui nao visivel
    var footerHeight = footer.offsetHeight; //inclui border
    var footerHeightWithoutBorder = footer.scrollHeight;

    var contentHeight = headerHeight + containerHeight + footerHeight;
    if (contentHeight < windowHeight) {
        footer.style.position = "absolute";
        footer.style.top = windowHeight - footerHeightWithoutBorder;
    } else {
        footer.style.position = "static";
    }
}

function GerenciarFooter() {
    window.addEventListener("resize", PositionFooter);
    document.getElementById("header").addEventListener("DOMSubtreeModified", PositionFooter);
    document.getElementById("container-body").addEventListener("DOMSubtreeModified", PositionFooter);
    PositionFooter();
}

function EnableDiv(newDiv, tipoGlobal, fieldRequired, topBar) { 

    if (tipoGlobal == 'currentDiv')
    {
        document.getElementById("li" + currentDiv).className = 'press';
        if(topBar == 'profile')
            document.getElementById("li" + newDiv).className = 'press profTopBarActived';
        else if (topBar == 'channel')
            document.getElementById("li" + newDiv).className = 'press chanTopBarActived';

        document.getElementById(currentDiv).style.display = 'none';
        currentDiv = newDiv;
    }
    else if (tipoGlobal == 'divTipoTopic')
    {
        if (divTipoTopic != '')
        {
            document.getElementById(divTipoTopic).style.display = 'none';
            if(fieldRequired != '')
            SetFieldRequired(fieldRequired);
        }
        divTipoTopic = newDiv;
    }
    
    document.getElementById(newDiv).style.display = 'block';
}

function EnableTwoDiv(newDiv, tipoGlobal, divTwo, fieldRequired)
{
    document.getElementById(divTwo).style.display = 'block';
    EnableDiv(newDiv, tipoGlobal, fieldRequired, '');   
    
}

function SetFieldRequired(namefield)
{
    document.getElementById(namefield).required = true;
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

    ////Mudar de Lista pra Grade e Grade pra lista
    //function ChangeTo(tipoPost, tipoAtual)
    //{
    //    if (tipoPost == tipoAtual) {
    //        //nao faz nada
    //    }
    //    else if (tipoPost == 'post_lista') {
    //        var item;
    //        var div = document.getElementsByClassName('divTopic-grade');
    //        for (item = 0; item < div.length; item++) {
    //            div[item].setAttribute('class', 'divTopic-lista');
    //        }
    //        document.getElementByClass("headerTopic-grade").setAttribute("class", "headerTopic-lista");
    //        document.getElementByClass("imgTopic-grade").setAttribute("class", "imgTopic-lista");
    //        document.getElementByClass("contentTopic-grade").setAttribute("class", "contentTopic-lista");
    //        document.getElementByClass("footerTopic-grade").setAttribute("class", "footerTopic-lista");
    //        document.getElementByclass("btnLeftTopic-grade").setAttribute("class", "btnLeftTopic-lista");
    //        document.getElementByclass("btnRightTopic-grade").setAttribute("class", "btnRightTopic-lista");
    //    }
    //    else {

    //    }
    //    else {

    //    }
    

    //}

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
        else if (global == 'skipTopicFavorite') {
            value = skipTopicFavorite;
            type = "favorited";
        }

        var namediv = global.replace("skip", "");
        var divLoad = document.getElementById("img" + namediv + "Load");
        if(divLoad != null)
           divLoad.style.display = "block";

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
                    else if (global == 'skipTopicFavorite')
                        skipTopicFavorite = skipTopicFavorite + 1;

                    document.getElementById(orderBy).innerHTML += newData.Html;

                    if (newData.Count < 15) {
                        document.getElementById("btn_" + global).style.display = "none";
                    }
                    if(divLoad != null)
                        divLoad.style.display = "none";
                },

                500: function () {
                    //erro
                    if (divLoad != null)
                        divLoad.style.display = "none";
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

        if (global == 'skipMyChannelsMannager')
            value = skipMyChannelsMannager;

        var nameLoad = "img" + global.replace("skip", "") + "Load";

        document.getElementById(nameLoad).style.display = 'block';

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
                    if (global == 'skipMyChannelsMannager')
                        skipMyChannelsMannager = skipMyChannelsMannager + 1;

                    document.getElementById(orderBy).innerHTML += newData.Html;

                    if (newData.Count < 15) {
                        document.getElementById("btn_" + global).style.display = "none";
                    }
                    document.getElementById(nameLoad).style.display = 'none';
                },

                500: function () {
                    //erro
                    document.getElementById(nameLoad).style.display = 'none';
                    window.alert("Erro ao obter mais canais. Tente novamente mais tarde.");
                }
            }
        });
    }


    //método que busca  as mensagens por paginaçao e/ou enviadas
    function viewMessages(global, viewBy, typeBtn, typeClick) {
        var divB; var divN; var nameBtn; var load;

        if (typeClick == 'firstGetSend') {
            load = document.getElementById("imgTopBarLoad");
        }
        else {
            load = document.getElementById("img" + viewBy + "Load")
        }
       
        //arrumando os parametros dos cliques, divs e buttons
        if (global == 'skipMessageSend') {
            value = skipMessageSend;
            divLoad = "divSeeMore";           
            divB = 'divSeeMessagesSend';
            divN = 'divSeeMessages';
            nameBtn = 'btn_moreMsgSend';
            if (countSend < 15)
                btnStyle = 'none';
            else btnStyle = 'block';
            document.getElementById('liMsgSend').onclick = function () { viewMessages('skipMessageSend', 'messageSend', 'send', 'back'); }
        }

        if (global == 'skipMessageReceived') {
            divLoad = "divSeeMoreR";   
            value = skipMessageReceived;       
            divN = 'divSeeMessagesSend';
            divB = 'divSeeMessages';
            nameBtn = 'btn_moreMsgReceveid';
            if (countReceived < 15)
                btnStyle = 'none';
            else btnStyle = 'block';
            document.getElementById('liMsgReceived').onclick = function () { viewMessages('skipMessageReceived', 'messageReceived', 'received', 'back'); }
        }
        
        //configurando o onclick de acordo com a acao do usuario       
        var btn = document.getElementById(nameBtn);
        if (btn != null) {
            if (typeBtn == 'send') {
                btn.onclick = function () { viewMessages('skipMessageSend', 'messageSend', 'send', 'seeMore'); }               
            }
            else {
                btn.onclick = function () { viewMessages('skipMessageReceived', 'messageReceived', '', 'seeMore'); }
            }
        }

        if (typeClick == 'seeMore' || typeClick == 'firstGetSend') {
            
            if(typeClick == 'seeMore') {              
                document.getElementById(nameBtn).style.display = 'none';
            }
            if (load != null) {
                load.style.display = 'block';
            }
            
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
                            if (load != null)
                                load.style.display = 'none';
                        }
                        else
                            if (global == 'skipMessageReceived') {
                                skipMessageReceived = skipMessageReceived + 1;
                                divN = "divSeeMessagesSend";
                                divB = "divSeeMessages";
                                countReceived = newData.Count;
                                if (load != null)
                                    load.style.display = 'none';
                            }

                        document.getElementById(divB).style.display = 'block';
                        document.getElementById(divN).style.display = 'none';

                        document.getElementById(divB + "Content").innerHTML += newData.Html;

                        if (newData.Count < 15) {
                            btn.style.display = "none";
                        }
                        else {
                            if(btn != null)
                            btn.style.display = "block";
                        }
                        if (load != null)
                            load.style.display = 'none';
                    },

                    500: function () {
                        //erro
                        if (load != null)
                            load.style.display = 'none';
                        window.alert("Erro ao processar requisição. Tente novamente mais tarde.");
                    }
                }
            });
        }
        else if (typeClick == 'back') {
            document.getElementById(divB).style.display = 'block';
            document.getElementById(divN).style.display = 'none';            
            if (btn != null) {
                btn.style.display = btnStyle;
            }
        }

        //arrumando a cor do btn da top bar
        ChangeBackgroundChoiceMessage(global, divB, divN);
    }

    function ChangeBackgroundChoiceMessage(global, divToShow, divToHidden)
    {
        var btnActive; var btnStayGrey;
        if (global == 'skipMessageSend') {
            btnActive = 'liMsgSend';
            btnStayGrey = 'liMsgReceived';
        }
        else if (global == 'skipMessageReceived') {
            btnStayGrey = 'liMsgSend';
            btnActive = 'liMsgReceived';
        }

        ChoiceViewChannel(divToShow, divToHidden, btnActive, btnStayGrey);
    }

    function setOnClickSeeMesg(id, starNameDiv)
    {
        countClick++;
        var ok = false;
        var divM = document.getElementById('divMesg_' + idMsgExpand);
        var divE = document.getElementById("divMesgExpand_" + idMsgExpand);

        if (idMsgExpand == id) {
            if (countClick <= 1 && countClick > 0) {
                ok = true;
            }
            else {
                ok = false;                
                if (divE && divM != null) {
                    divE.style.display = 'none';
                    divM.className = 'post_m';
                    countClick = 0;
                    tipoMessAtual = 'normal';
                }
            }
        }
        else
            ok = true;

        if (ok == true) {
            if (tipoMessAtual == 'normal') {
                tipoMessAtual = "expandido";
                idMsgExpand = id;
                ajaxSeeMsg(id, starNameDiv);
            }
            else if (tipoMessAtual == "expandido") {
                tipoMessAtual = 'normal';

                if (idMsgExpand == id) { //minimiza
                    document.getElementById('divMesg_' + id).className = 'post_m';
                    document.getElementById('divMesgExpand_' + id).style.display = 'none';
                    idMsgExpand = 0; //global que controla qm está expandido
                    countClick = 0;
                }
                else //minimiza a q está aberta e abre a nova
                {                   
                    if (divE && divM != null) {
                        divE.style.display = 'none';
                        divM.className = 'post_m';
                        countClick = 0;
                    }
                    return setOnClickSeeMesg(id, starNameDiv);
                }
            }
        }
    }

    function ajaxSeeMsg(id, starNameDiv)
    {
        var divNew = document.getElementById('divMesgExpand_' + id);
        var divOld = document.getElementById('divMesg_' + id).style.background = "#d1d3d4";

        if (divNew != null) {
            //muda pra expand
            tipoMessAtual = "expandido";
            divNew.className = 'post_m_exp';
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
                        //muda pra expand
                        tipoMessAtual = "expandido";
                        div.style.display = 'block';
                        countClick = 0;
                    },

                    500: function () {
                        //erro
                        countClick = 0;
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
        var divSearch = document.getElementById('divBarSearch');
        if (divSearch.style.display == 'block')
        {
            divSearch.style.display = 'none';
            document.getElementById('aOpenDivSearch').className = "BSearch";
        }
        else
        {
            divSearch.style.display = 'block';
            document.getElementById('aOpenDivSearch').className = "BTopBarLinkSelected";
        }
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
 
    function ajaxResetPassword(newPass, confirmPass) {
        if (document.getElementById("formResetPass").checkValidity()) {

            ajaxData = {};
            ajaxData['NewPassord'] = document.getElementById(newPass).value;
            ajaxData['ConfirmPassword'] = document.getElementById(confirmPass).value;
            ajaxData['Token'] = document.getElementById('hdnToken').value;
         
            $.ajax({
                url: "/api/user/resetpassword/",
                data: JSON.stringify(ajaxData),
                type: "POST",
                contentType: "application/json;charset=utf-8",
                statusCode: {
                    200: function (newData) {
                        if (newData > 0) {
                            window.location.href = "/login";
                        }
                    },

                    500: function () {
                        //erro
                        window.alert("Não foi possível alterar sua senha. Tente novamente mais tarde.");
                    }
                }
            });
        }

    }