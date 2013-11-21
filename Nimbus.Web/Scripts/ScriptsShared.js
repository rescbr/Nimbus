﻿
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