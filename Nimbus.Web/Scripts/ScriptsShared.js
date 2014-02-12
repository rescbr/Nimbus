
/*Método para mudar divs dentro de outra div que já esta sendo alterada pela funçao changediv
  usada na pagina de channl -> novo tópico*/

function PositionFooter(evt) {
    var footer = document.getElementById("footer");
    var windowHeight = document.getElementsByTagName("html")[0].clientHeight; //apenas viewport
    var headerHeight = document.getElementById("header").offsetHeight;

    var containerBody = document.getElementById("container-body");
    var containerHeight = containerBody.scrollHeight; //inclui nao visivel
    if (containerHeight == 0) containerHeight = GetElementScrollHeightByChildren(containerBody); //devido a bug webkit

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

function GetElementScrollHeightByChildren(element) {
    //função devido a bugs nao corrigidos no webkit
    //https://bugs.webkit.org/show_bug.cgi?id=61124
    //http://code.google.com/p/chromium/issues/detail?id=34224
    var childrenHeights = 0;
    var cbChildren = element.children;
    for (var i = 0; i < cbChildren.length; i++) {
        childrenHeights += cbChildren[i].scrollHeight;
    }

    return childrenHeights;
}

function FadeOut(id, time)
{
    fade(id, time, 100, 0);
}

function FadeIn(id, time) {
    fade(id, time, 0, 100);
}

function setAlpha(target, alpha) {
    target.style.filter = "alpha(opacity=" + alpha + ")";
    target.style.opacity = alpha / 100;
}

function fade(id, time, ini, fin)
{
    var target = document.getElementById(id);
    var alpha = ini;
    var inc;
    if (fin >= ini) {
        inc = 2;
    } else {
        inc = -2;
    }
    timer = (time * 1000) / 50;
    var i = setInterval(
        function () {
            if ((inc > 0 && alpha >= fin) || (inc < 0 && alpha <= fin)) {
                clearInterval(i);
            }
            setAlpha(target, alpha);
            alpha += inc;
        }, timer);
}

function setDivLoad(display)
{
    var divLoad = document.getElementById("divFadeLoad");
    divLoad.style.position = "fixed";
    divLoad.style.height = document.getElementsByTagName("html")[0].clientHeight + "px";
    divLoad.style.width = document.getElementsByTagName("html")[0].clientWidth + "px";
    divLoad.style.background = "#F2F2F2";
    if (display == 'none') //fadeOut
    {
        FadeOut('divFadeLoad', 1);
    }
    else
    {
        FadeIn('divFadeLoad', 0.3);
    }
    divLoad.style.display = display;
}

function openModalFeedback(tipo)
{
    var field = document.getElementById('pTypeFeedback');
    var text = '';
    var modal = document.getElementById('pTextContentModal');
    document.getElementById('txtaFeedback').value = '';

    if (tipo == 'positive')
    {
        text = "Feedback positivo";
        modal.innerHTML = 'Nós apreciamos o seu feedback. O que você gostou?';
        document.getElementById('imgEmoticon').src = "/images/utils/emoticonfeliz.png"
    }
    else if (tipo == 'negative')
    {
        text = "Feedback negativo";
        modal.innerHTML = 'Nós apreciamos o seu feedback. O que você acha que poderíamos melhorar?';
        document.getElementById('imgEmoticon').src = "/images/utils/emoticontriste.png"
    }
    field.innerHTML = text;
    typeFeedback = tipo == 'positive' ? 1 : 0;
    window.location.href = '#modal-feedback';
}

function ajaxSendFeedback()
{
    var message = document.getElementById('txtaFeedback').value;
    $.ajax({
        url: "/api/user/SendFeedback/" + "?type=" + typeFeedback + "&message=" + JSON.stringify(message) ,
        type: "POST",
        contentType: "application/json;charset=utf-8",
        statusCode: {
            200: function (newData) {
                if(newData == true)
                    window.alert("Obrigado por nos enviar seu feedback.");
                else
                    window.alert("Não foi possível enviar seu feedback neste momento. Tente novamente mais tarde.");

                document.getElementById('closeModalRFeedback').click();
            },

            500: function () {               
                window.alert("Não foi possível enviar seu feedback neste momento. Tente novamente mais tarde.");
            }
        }
    });
    document.getElementById('closeModalRFeedback').click();
}

function EnableDiv(newDiv, tipoGlobal, fieldRequired, topBar) { 

    if (tipoGlobal == 'currentDiv')
    {
        document.getElementById("li" + currentDiv).className = 'press';
        if(topBar == 'profile')
            document.getElementById("li" + newDiv).className = 'press profTopBarActived';
        else if (topBar == 'channel') {
            document.getElementById("li" + newDiv).className = 'press chanTopBarActived';
            var nameP = currentDiv.replace("div", "p");
            var newP = newDiv.replace("div", "p");
            document.getElementById(nameP).style.color = "";
            document.getElementById(newP).style.color = "#87c240";
        }

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
        if (newDiv == 'divExam')
            document.getElementById('btnAddNewQuestion').style.display = 'block';
        else
            document.getElementById('btnAddNewQuestion').style.display = 'none';
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
    function seeMoreComments(id, nameDiv, global, page, button) {
       
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
                        button.style.display = "none";
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


    function ajaxForgotPassword() {
        var email = document.getElementById('inptEmailPass').value;
                $.ajax({
                    url: "/api/user/checkValidEmail/" +"?email=" + JSON.stringify(email),
                type: "GET",
                contentType: "application/json;charset=utf-8",
                statusCode: {
                    200: function (newData) {
                        if (newData == true) {
                            window.alert("Um e-mail foi enviado para sua conta de login com o código de redefinição de senha.");
                        }
                        else { window.alert("Não conseguimos enviar o código de redefinição de senha para seu email. \n Verifique se digitou o e-mail correto ou tente novamente mais tarde."); }
                    },

                    500: function () {
                        //erro
                        window.alert("Não foi possível alterar sua senha. Tente novamente mais tarde.");
                    }
                }
            });

    }

    function getTopTopics()
    {
        /*if (skip < 0)*/ skip = 0;
        /*if (take <= 0)*/ take = 12;
        setDivLoad("block");

        var select = document.getElementById("slcCategoryTop").selectedIndex;
        var option = document.getElementById("slcCategoryTop").options;
        var category = option[select].value;

        
        $.ajax({
            url: "/api/topic/TopTopicHtml/" + "?take=" + take + "&categoryID=" + category + "&skip=" + skip,
            type: "GET",
            contentType: "application/json;charset=utf-8",
            statusCode: {
                200: function (newData) {
                    if (newData.Count > 0) {
                        document.getElementById('divTrendingTopics').innerHTML = newData.Html;
                    }
                    else
                    {
                        document.getElementById('divTrendingTopics').innerHTML = '<p>Nenhum resultado encontrado para esta categoria.</p>';
                    }
                    setDivLoad("none");
                },

                500: function () {
                    //erro
                    window.alert("Não foi possível realizar esta operação. Tente novamente mais tarde.");
                }
            }
        });
    }

    function getMoreCategory(option)
    {
        //region load
        var divLoad = document.getElementById("divCtgLoad");
       
        var skip = Number(countCat);
        var totalCat = document.getElementById('hdnTotalCategory').value;
        var flag = false;

        if (skip != totalCat - 1) {
            FadeIn('divCtgLoad', 0.3);
            divLoad.style.display = 'block';
        }

        if (option == 'back')
        {
            document.getElementById('imgNextCtg').style.opacity = '1';
            if (skip == 0) {
                document.getElementById('imgBackCtg').style.opacity = '0.5';
                flag = false;
            }
            else {
                skip = skip - 1;
                if (skip == 0)
                    document.getElementById('imgBackCtg').style.opacity = '0.5';
                else
                    document.getElementById('imgBackCtg').style.opacity = '1';
                flag = true;
            }
        }
        else if (option == 'next')
        {
            document.getElementById('imgBackCtg').style.opacity = '1';

            if (skip == (totalCat - 1))
            {
                document.getElementById('').style.opacity = '0.5';
                flag = false;
            }
            else if(skip < (totalCat - 1)) //pq skip começa do zero
            {
                skip = skip + 1;
                if(skip == (totalCat - 1))
                    document.getElementById('imgNextCtg').style.opacity = '0.5';
                else
                    document.getElementById('imgNextCtg').style.opacity = '1';               
                flag = true;
            }
        }

        if (flag == true)
        {
            $.ajax({
                url: "/api/category/showCategoryToPage/" + "?skip=" + skip,
                type: "GET",
                contentType: "application/json;charset=utf-8",
                statusCode: {
                    200: function (newData) {
                        if (newData.length > 0) {
                            if (option == 'next')
                                countCat = countCat + 1;
                            else if(option == 'back')
                                countCat = countCat - 1;
                            var string = '';

                            for (var i = 0; i < newData.length; i++) {
                                string += "<div class=\"imgCategoryPage\" onclick=\"getAllChannels(" + newData[i].Id + ", '" + newData[i].Name + "');\" >" +
                                            "<img src=\""+newData[i].ImageUrl +"\" title=\""+newData[i].Name+"\" alt=\""+newData[i].Name+"\" width='180' height='100' />" +
                                         "</div>";
                            }

                            document.getElementById('divShowCat').innerHTML = string;
                        }
                    },

                    500: function () {
                        //erro
                        window.alert("Não foi possível realizar esta operação. Tente novamente mais tarde.");
                    }
                }
            });
        }

        FadeOut('divCtgLoad', 1);
        divLoad.style.display = 'none';
    }

    function getAllChannels(id, nameCat)
    {
        document.getElementById('hTitleCategory').innerHTML = '';
        $.ajax({
            url: "/api/search/AbstChannelHtml/" +id + "?nameCat=" + JSON.stringify(nameCat),
            type: "GET",
            contentType: "application/json;charset=utf-8",
            statusCode: {
                200: function (newData) {
                    if (newData.Count > 0) {
                        if (newData.Count > 1) {
                            document.getElementById('hTitleCategory').innerHTML = "Categoria " + nameCat + ": " + newData.Count + " resultados encontrados.";
                        }
                        else
                        {
                            document.getElementById('hTitleCategory').innerHTML = "Categoria " + nameCat + ": " + newData.Count + " resultado encontrado.";
                        }
                        document.getElementById('divAllChnCategory').innerHTML = newData.Html;
                    }
                    else {
                        document.getElementById('divAllChnCategory').innerHTML = '<p>Nenhum resultado encontrado para a categoria '+nameCat+'.</p>';
                    }
                },

                500: function () {
                    //erro
                    window.alert("Não foi possível realizar esta operação. Tente novamente mais tarde.");
                }
            }
        });
    }

    function validPassword(senha) {
        var forca = 0; var color = "";
        var resultado = "";
        if ((senha.length >= 4) && (senha.length <= 7)) {
            forca += 10;
        } else if (senha.length > 7) {
            forca += 25;
        }
        if (senha.match(/[a-z]+/)) {
            forca += 10;
        }
        if (senha.match(/[A-Z]+/)) {
            forca += 20;
        }
        if (senha.match(/\d+/)) {
            forca += 20;
        }
        if (senha.match(/\W+/)) {
            forca += 25;
        }
       
        if (forca < 30) {
            resultado = "Nível: senha fraca";
            color = "#be1e2d";
        } else if ((forca >= 30) && (forca < 60)) {
            resultado = "Nível: senha mediana";
            color = "#cb5907";
        } else if ((forca >= 60) && (forca < 85)) {
            resultado = "Nível: senha forte";
            color = "#2b79a1";
        } else {
            resultado = "Nível: senha muito forte";
            color = "#223d98";
        }
        document.getElementById('pPowerPass').style.color = color;
        document.getElementById('pPowerPass').innerHTML = resultado;
    }

    function validLengthString(min, max, campo, string)
    {
        var dontSave = false;

        if (campo == 'pErrorDate')
        {
            max = max - 1;
            if (string < min && string > max) {
                document.getElementById(campo).innerHTML = '*O ano permitido é de: 1930 até ' + max;
                dontSave = true;
            }
            else { dontSave = false;}
        }
        else {
            if (string.length < min) {
                document.getElementById(campo).innerHTML = '*Mínimo de caracteres: ' + min;
                dontSave = true;
            }
            else if (string.length > max) {
                document.getElementById(campo).innerHTML = '*Máximo de caracteres: ' + max;
                dontSave = true;
            }
            else
            {
                document.getElementById(campo).innerHTML = '';
                dontSave = false;
            }
        }

        if (dontSave == true)
        {
            document.getElementById('salvarNewAccount').disabled = true;
        }
        else
        {
            document.getElementById('salvarNewAccount').disabled = false;
        }
    }


   