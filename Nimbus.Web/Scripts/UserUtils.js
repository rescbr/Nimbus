/*Este arquivo deve conter apenas os scripts referentes as páginas de profile*/

function ChangeDiv(newDiv)
{
    //currentDiv: variavel global que passa o nome da div ativa
    document.getElementById(currentDiv).style.display = 'none';
    document.getElementById(newDiv).style.display = 'block';
    currentDiv = newDiv;
}