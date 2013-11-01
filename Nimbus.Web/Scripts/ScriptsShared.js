
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
//param: nome do campo que vai ter a string do video, nome do frame que vai receber a string como src
function getUrlVideo(nomecampo, nomeframe)
{
    var text = document.getElementById(nomecampo).value;
    var queryurl;
    if (text.length >= 11)
    {
        if (text.indexOf("youtube.be/") > 0)
        {
            var params = text.search.substr(text.search.indexof("be/")+3);
            queryurl = "//www.youtube.com/embed/" + params;           
        }
         else     
        if (text.indexOf("youtube.com"))
        {
            var params = text.search("v=");
            params = text.slice(params + 2);
            queryurl = "//www.youtube.com/embed/" + params;            
        }
        if (queryurl.length > 34) {
            document.getElementsByTagName("IFRAME")[0].setAttribute('src', queryurl);            
        }
       
    }
    
}


