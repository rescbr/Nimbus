function ChangeDiv(newDiv) {
    //currentDiv: variavel global que passa o nome da div ativa
    document.getElementById(currentDiv).style.display = 'none';
    document.getElementById(newDiv).style.display = 'block';
    currentDiv = newDiv;
}

/*Método para mudar divs dentro de outra div que já esta sendo alterada pela funçao changediv
  usada na pagina de channl -> novo tópico*/
function EnableDiv(newDiv) {
    //currentDiv: variavel global que passa o nome da div ativa   
    if (divTipoTopic != '') {
        document.getElementById(divTipoTopic).style.display = 'none';
    }
    document.getElementById(newDiv).style.display = 'block';
    divTipoTopic = newDiv;
}

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


