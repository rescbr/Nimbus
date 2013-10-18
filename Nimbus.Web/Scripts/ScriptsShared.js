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