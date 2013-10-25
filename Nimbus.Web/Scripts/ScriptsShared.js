
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

function ChangeToGrade(lista)
{
    document.getElementById(lista).attr("class");

}