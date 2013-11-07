/*Este arquivo deve conter apenas os scripts referentes as páginas de profile*/

function SendMessageProfile(receiverId)
{
    ajaxMessage = {};    
    var text = CKEDITOR.instances.txtTextMsg.getData();
    var title = document.getElementById('inpTitleMsg').value;

    if (text != "")
    {
        ajaxMessage["Text"] = text;
        if (title != "")
            ajaxMessage["Title"] = title;
        else
            ajaxMessage["Title"] = "Sem assunto";       

        $.ajax({                        
            url: "/api/Message/SendMessageUser/"+receiverId,
            data: JSON.stringify(ajaxMessage),
            type: "POST",
            contentType: "application/json;charset=utf-8",
            statusCode: {
                200: function (newData) {
                   
                    if (newData.Id > 0)
                    {
                        //fechar modal
                        document.getElementById('closeModal').click();
                        //limpar campos
                        text.value = "";
                        title.value = "";
                        //aviso
                        window.alert("Mensagem enviada com sucesso.");
                    }
                },

                400: function () {
                    //erro
                    window.alert("Não foi possível enviar sua mensagem. Tente novamente mais tarde.");
                }
            }
        });
    }
}

function EditProfile()
{

}