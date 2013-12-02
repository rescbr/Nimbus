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
                        document.getElementById('closeModalMessage').click();
                        //limpar campos
                        text.value = "";
                        title.value = "";
                        //aviso
                        window.alert("Mensagem enviada com sucesso.");
                    }
                },

                500: function () {
                    //erro
                    window.alert("Não foi possível enviar sua mensagem. Tente novamente mais tarde.");
                }
            }
        });
    }
}

function SaveEditProfile()
{
    var firstName = document.getElementById('txtFirstName').value;
    var lastName = document.getElementById('txtLastName').value;
    var city = document.getElementById('txtCity').value;
    var state = document.getElementById('txtState').value;
    var country = document.getElementById('txtCountry').value;
    var occupation = document.getElementById('txtOccupation').value;
    var interest = document.getElementById('txtInterest').value;
    var experience = document.getElementById('txtExperience').value;
    var about = document.getElementById('txtAbout').value;
    allow = true;
    if(firstName == "")
    {
        allow = false;
    }//colocar aviso
    if(lastName == "")
    {
        allow = false;
    }

    if (allow == true)
    {
        var ajaxData = {};
        ajaxData['FirstName'] = firstName;
        ajaxData['LastName'] = lastName;
        ajaxData['City'] = city;
        ajaxData['State'] = state;
        ajaxData['Country'] = country;
        ajaxData['Occupation'] = occupation;
        ajaxData['Interest'] = interest;
        ajaxData['Experience'] = experience;
        ajaxData['About'] = about;

        $.ajax({
            url: "/api/user/EditProfile",
            data: JSON.stringify(ajaxData),
            type: "POST",
            contentType: "application/json;charset=utf-8",
            statusCode: {
                200: function (newData) {

                    if (newData.Id > 0) {
                        //fechar modal
                        document.getElementById('closeModalEdit').click();
                        //limpar campos
                        firstName.value = "";
                        lastName.value = "";
                        city.value = "";
                        state.value = "";
                        country.value = "";
                        occupation.value = "";
                        interest.value = "";
                        experience.value = "";
                        document.getElementById('lblName').value = newData.FirstName + " " + newData.LastName;

                        var place = "";
                        if(newData.City != "" && newData.State != "")
                            place = newData.City + " - " + newData.State;
                        else if(newData.City != "" && newData.State == "")
                            place = newData.City
                        else if(newData.City == "" && newData.State != "")
                            place = newData.State
                        document.getElementById('lblCity').value = place;
                        
                        document.getElementById('lblOccupation').value = newData.Occupation;
                        document.getElementById('lblCountry').value = newData.Country;
                        document.getElementById('lblInterest').value = newData.Interest;
                        document.getElementById('lblExperience').value = newData.Experience;
                        document.getElementById('lblAbout').value = newData.de;
                    }
                },
                500: function () {
                    //erro
                    window.alert("Não foi possível alterar seu perfil. Tente novamente mais tarde.");
                }
            }
        });
    }
    
}
