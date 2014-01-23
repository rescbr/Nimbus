/*Este arquivo deve conter apenas os scripts referentes as páginas de profile*/

    function ChoiceViewChannel(divToShow, divToHidden, changeBackground, removeBackground)
    {
        document.getElementById(divToShow).style.display = 'block';
        var divH = document.getElementById(divToHidden);
        if (divH != null) //caso esteja voltando
            divH.style.display = 'none'

        document.getElementById(changeBackground).style.background = '#87c240';
        var cssR = document.getElementById(removeBackground);
        if(cssR != null)
            cssR.style.background = '';
    }

    function SendMessageProfile(receiverId) {
    ajaxMessage = {};
    var text = $("#txtTextMsg").htmlarea('html');
    var title = document.getElementById('inpTitleMsg').value;

    if (document.getElementById("formSendMsgProfile").checkValidity()) {
        {
            ajaxMessage["Text"] = text;
            if (title != "")
                ajaxMessage["Title"] = title;
            else
                ajaxMessage["Title"] = "Sem assunto";

            $.ajax({
                url: "/api/Message/SendMessageUser/" + receiverId,
                data: JSON.stringify(ajaxMessage),
                type: "POST",
                contentType: "application/json;charset=utf-8",
                statusCode: {
                    200: function (newData) {
                        
                        if (newData.Id > 0) {
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
                        document.getElementById('closeModalMessage').click();
                        window.alert("Não foi possível enviar sua mensagem. Tente novamente mais tarde.");
                    }
                }
            });
        }
    }
}

    function SaveEditProfile() {
        if (document.getElementById("formEditProfile").checkValidity()) {
            var firstName = document.getElementById('txtFirstName').value;
            var lastName = document.getElementById('txtLastName').value;
            var city = document.getElementById('txtCity').value;
            var state = document.getElementById('txtState').value;
            var country = document.getElementById('txtCountry').value;
            var occupation = document.getElementById('txtOccupation').value;
            var interest = document.getElementById('txtInterest').value;
            var experience = document.getElementById('txtExperience').value;
            var about = document.getElementById('txtAbout').value;
            var birth = document.getElementById('txtBirthDateEdit').value;

            var pass = document.getElementById("txtNewPassProfile").value;
            var conf = document.getElementById("txtConfirPassProfile").value;
            var hasPass = true;

            if (pass != null  && pass.length >= 6 && conf != null && conf.length >=6 )
            {
                if (pass == conf) {
                    //chamar função de salvar nova senha               
                    ajaxPass = {};
                    ajaxPass['NewPassord'] = pass
                    ajaxPass['ConfirmPassword'] = conf;
                    ajaxPass['Token'] = "logado";

                    $.ajax({
                        url: "/api/user/resetpassword/",
                        data: JSON.stringify(ajaxData),
                        type: "POST",
                        contentType: "application/json;charset=utf-8",
                        statusCode: {
                            200: function (newData) {
                                if (newData > 0) {
                                    haspass = true;
                                }
                            },

                            500: function () {
                                //erro
                                haspass = false;
                                window.alert("Não foi possível alterar sua senha. Tente novamente mais tarde.");
                            }
                        }
                    });
                }
                else
                {
                    window.alert("Os campos 'Nova senha' e 'Confirmar senha' devem ser iguais.");
                    hasPass = false;
                }
            }
            
            if (hasPass == true) {
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
                ajaxData['BirthDate'] = birth;

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
                                if (newData.City != "" && newData.State != "")
                                    place = newData.City + " - " + newData.State;
                                else if (newData.City != "" && newData.State == "")
                                    place = newData.City
                                else if (newData.City == "" && newData.State != "")
                                    place = newData.State
                                document.getElementById('lblCity').value = place;

                                document.getElementById('lblOccupation').value = newData.Occupation;
                                document.getElementById('lblCountry').value = newData.Country;
                                document.getElementById('lblInterest').value = newData.Interest;
                                document.getElementById('lblExperience').value = newData.Experience;
                                document.getElementById('lblAbout').value = newData.About;
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
    }

    function CancelEditProfile(form) {

        var divPassword = document.getElementById("divResetPass");
        if (divPassword.style.display == "block")
        {
            divPassword.style.display = 'none';
        }
        form.reset();
        document.getElementById("closeModalEdit").click(); //fechar o modal

    }

    function ajaxDeleteMsg(id) {
        decisao = confirm("Você deseja deletar esta mensagem?");
        if (decisao) {
            ajaxData = [];
            ajaxData.push(id.replace('deleteMsg_', ''));

            $.ajax({
                url: "/api/Message/DeleteMessages/",
                data: JSON.stringify(ajaxData),
                type: "DELETE",
                contentType: "application/json;charset=utf-8",
                statusCode: {
                    200: function (newData) {

                        if (newData.length > 0) {
                            for (i = 0; i < newData.length; i++) {
                                document.getElementById('divMesg_' + newData[i]).style.display = 'none';
                                var divExpand = document.getElementById('divMesgExpand_' + newData[i]);
                                if (divExpand != null)
                                    divExpand.style.display = 'none';
                            }
                        }
                    },

                    500: function () {
                        //erro
                        window.alert("Não foi possível deletar sua mensagem. Tente novamente mais tarde.");
                    }
                }
            });
        }
        else
        {
            return false;
        }
    }

    function ajaxReportUser(idUserReported, idUserReporter) {
        var text = document.getElementById('txtJustificativa').value;

        if (document.getElementById("formReportUser").checkValidity()) {
            ajaxData = {};
            ajaxData['Justification'] = text;
            ajaxData['UserReportedId'] = idUserReported; //foi reportado
            ajaxData['UserReporterId'] = idUserReporter;

            $.ajax({
                url: "/api/report/ReportUser/",
                data: JSON.stringify(ajaxData),
                type: "POST",
                contentType: "application/json;charset=utf-8",
                statusCode: {
                    200: function (newData) {
                        if (newData.Id > 0) {
                            window.alert("Operação realizada com sucesso!");
                        }
                    },

                    500: function () {
                        //erro
                        window.alert("Não foi possível realizar esta operação. Tente novamente mais tarde.");
                    }
                }
            });
            document.getElementById("closeModalReportUser").click();

        }
    }

    function setDisplayBtnEditAvatar(visible) {
        document.getElementById("divBtnEditAvatar").style.display = visible;
    }

    function createModalAnswerMsg(receivers, idReceivers, title, text)
    {
        var nameReceivers ="";
        for (var i = 0; i < receivers.length; i++) {
            nameReceivers += receivers[i] + " " ;
        };

        if (document.getElementById("modal-answerMsg") != null)
        {
            document.getElementById("pNameReceiversAnswer").innerHTML = nameReceivers;
            document.getElementById("inpTitleAnswerMsg").value = "RE: " + title;
            document.getElementById("txtTextAnswerMsg").innerHTML = "\n\n\n\n---\n" + text;
            document.getElementById("inpSendAnswerMsg").onclick = function () { SendAnswerMsg(idReceivers, 'inpTitleAnswerMsg'); };
        }
        else
        {
             idReceivers = JSON.stringify(idReceivers)
            var string = "<section class=\"semantic-content\" id=\"modal-answerMsg\" tabindex=\"-1\" role=\"dialog\" aria-labelledby=\"modal-label\" aria-hidden=\"true\">" +
                            "<div class=\"modal-inner\">" +
                                "<header id=\"modal-label\">" +
                                    "<!-- Header -->" +
                                    "Enviar mensagem" +
                                "</header>" +
                                "<form id=\"formAnswerMsgProfile\" onsubmit=\"return false;\">" +
                                 "<div class=\"modal-content\">" +
                                     "<!-- The modals content -->" +
                                     "<div class=\"divformModal\">" +
                                         "<p>Para</p>" +
                                         "<p class=\"modalUser\" id=\"pNameReceiversAnswer\">" + nameReceivers + "</p>" +
                                     "</div>" +
                                     "<div class=\"divformModal\">" +
                                         "<p>Assunto</p>" +
                                         "<input id=\"inpTitleAnswerMsg\" name=\"inpAnswerTitleMsg\" type=\"text\" maxlength=\"100\" value=\"RE: " + title + "\" />" +
                                     "</div>" +
                                     "<div class=\"divformModal\">" +
                                         "<p>Mensagem</p>" +
                                         "<textarea id=\"txtTextAnswerMsg\" name=\"txtTextAnswerMsg\" rows=\"10\" cols=\"60\" required>"
                                             +"\n\n\n\n---\n"+ text +
                                         "</textarea>" +
                                     "</div>" +
                                 "</div>" +
                                    "<footer class=\"fooModal\">" +
                                           "<!-- Footer -->" +
                                           "<input type=\"submit\" id=\"inpSendAnswerMsg\" class=\"inputSubmit\" value=\"Enviar\" onclick='SendAnswerMsg(" + idReceivers + ", \"inpTitleAnswerMsg\");\' />" +
                                    "</footer>" +
                                 "</form>" +
                             "</div>" +
                             "<a href=\"#!\" id=\"closeModalAnswerMessage\" class=\"modal-close\" title=\"Close this modal\" data-close=\"Close\" data-dismiss=\"modal\">×</a>" +
                        "</section>";
            //colocar no html
            document.getElementById("divInsertModalAnswerMsg").innerHTML = string;
        }
        
        //chamar a função que abre
        document.getElementById("aClickModalAnswerMsg").click();

    }

    function SendAnswerMsg(listIdReceivers, fieldTitle)
    {
        ajaxMessage = {};
        var text = $("#txtTextAnswerMsg").htmlarea('html');
        var title = document.getElementById(fieldTitle).value;

        if (document.getElementById("formAnswerMsgProfile").checkValidity()) {
            {
                ajaxMessage["Text"] = text;
                if (title != "")
                    ajaxMessage["Title"] = title;
                else
                    ajaxMessage["Title"] = "RE: Sem assunto";

                $.ajax({
                    url: "/api/Message/SendAnswerMessage/?receivers=" + encodeURIComponent(JSON.stringify(listIdReceivers)),
                    data: JSON.stringify(ajaxMessage),
                    type: "POST",
                    contentType: "application/json;charset=utf-8",
                    statusCode: {
                        200: function (newData) {

                            if (newData.Id > 0) {
                                //fechar modal
                                document.getElementById('closeModalAnswerMessage').click();
                                //limpar campos
                                text.value = "";
                                title.value = "";
                                //aviso
                                window.alert("Mensagem enviada com sucesso.");
                            }
                        },

                        500: function () {
                            //erro
                            document.getElementById('closeModalMessage').click();
                            window.alert("Não foi possível enviar sua mensagem. Tente novamente mais tarde.");
                        }
                    }
                });
            }
        }
    }