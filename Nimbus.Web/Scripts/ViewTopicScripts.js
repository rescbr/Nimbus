/*Este arquivo deve conter apenas métodos utilizados nas views de tópicos*/

function clickEditTopic(id, topicType)
{
    if (topicType == "video")
    {
        document.getElementById('divCurrent_'+topicType).style.display = 'none';
    }
    else
        if (topicType == "text" || topicType == "discussion")
    { 
            document.getElementById('divCurrent_'+topicType).style.display = 'none';
    }
    else
    if (topicType == "exam") {
        document.getElementById('divCurrent_'+topicType).style.display = 'none';
    }
    document.getElementById('divRenderEdit').style.display = 'block';
}

function ajaxEditTopic(id, topicType, divOld) {
    var video; 
    var text; var exam = [];
    var ajaxData = {}  

    if (topicType == 'video') {
        video = document.getElementById('iframeNewVideo').src;
    }
    if (topicType == 'text') {
        text = $("#txtaArticle").htmlarea('html');
    }
    if (topicType == 'discussion') {
        text = $("#txtaTextMsg").htmlarea('html');
    }
    if (topicType == 'exam') {
        exam = examEdit();
    }

    title = document.getElementById('iptNewTitle').value;

    if (title != "" && id > 0 && (text != "" || video != "" || exam.length > 0)) {

        ajaxData["Title"] = title
        ajaxData["Id"] = id;
        ajaxData["ChannelId"] = channelID;
        ajaxData["ImgUrl"] = document.getElementById('imgNewPrevia').src;
        ajaxData["Description"] = document.getElementById('iptNewDescription').value;
        ajaxData["TopicType"] = topicType;
        ajaxData["Text"] = text;
        ajaxData["UrlVideo"] = video;
        ajaxData["Question"] = exam;

        $.ajax({
            url: "/api/topic/EditTopic",
            data: JSON.stringify(ajaxData),
            type: "POST",
            contentType: "application/json;charset=utf-8",
            statusCode: {
                200: function (newData) {
                    /* 0 = text, 1= video, 2=discussion, 3=exam*/
                    if (newData != null)
                    {                      
                        if (newData.TopicType == 1)
                        {
                            document.getElementById('iFrameVideo').src = newData.UrlVideo;
                        }
                        else if (newData.TopicType == 0 || newData.TopicType == 2)
                        {
                            document.getElementById('pTopicText').innerHTML = newData.Text;
                        }
                        else if (newData.TopicType == 3) {
                            updateFieldsEcam(newData);
                        }                                             

                        document.getElementById('divRenderEdit').style.display = 'none';
                        document.getElementById(divOld).style.display = 'block';

                        document.getElementById('iptNewTitle').value = newData.Title;
                        document.getElementById('imgNewPrevia').src = newData.ImgUrl;
                        document.getElementById('iptNewDescription').value = newData.Description;
                    }
                },

                500: function () {
                    //erro
                    window.alert("Não foi possível enviar seu comentário. Tente novamente mais tarde.");
                }
            }
        });

    }

}

function examEdit() {
    var listQuestion = [];
    var listPerg = document.getElementsByClassName('divPergEditarNimbus');
    var isChecked = false;
  
    //para novas perguntas
    for (perg = 0; perg < listPerg.length; perg++) //para cada pergunta
    {
        var dictionary = new Object();
        var questionData = {};
        var item = listPerg[perg];
        var enunciado = item.getElementsByClassName('enunciado')[0].value; //pego o conteudo = enunciado 

        if (enunciado != "") {
            questionData["TextQuestion"] = enunciado;

            //pega todas os conjuto de respostas
            var conjRespostas = item.getElementsByClassName('resposta');

            //para cada conjunto pegas as resposta
            for (conj = 0; conj < conjRespostas.length; conj++) {

                var rdb = item.getElementsByClassName('rdbPergEditNimbus')[conj];
                var ipt = item.getElementsByClassName('resposta')[conj];

                var texto = ipt.value;

                if (texto != "" && ipt.className.indexOf('fakeDisable') == -1 && ipt.style.display != 'none') {
                    if (rdb.checked == true) {
                        questionData["CorrectAnswer"] = conj + 1;
                        isChecked = true;
                    }
                    dictionary[conj + 1] = ipt.value; //valor digitado original //dictionary<int, string>

                    questionData["ChoicesAnswer"] = dictionary;
                }
            }
            if (isChecked == true) {
                listQuestion.push(questionData);
            }
        }
    }
    return listQuestion;
}

function updateFieldsEcam(data)
{
    var divContent = document.getElementById('divAllQuestions');
    var stringQuestion = "";

    for(i = 0; i < data.Question.length; i++)
    {
        var question = data.Question[i];
        var answers = question.ChoicesAnswer;       

        var stringAswer = ""; 
        for (var a in answers)
        {
            stringAswer += "<input type=\"radio\" name=\"" + i + "\" value=\"" + a.Key + "\" />" + a.Value + "<br />";
        }

        stringQuestion += "<section>" +
                            "<p><strong>" + (i + 1) + "</strong> - " + question.TextQuestion + "</p>" +
                            stringAswer +
                          "</section>";

        divContent.innerHTML = stringQuestion;
    }
}

function ajaxFinishExam(id)
{
    var ajaxData = {};
    var listQuestion = [];
    var listChoice = [];
    var listPerg = document.getElementsByClassName('divPergExam');
    
    //para novas perguntas
    for (perg = 0; perg < listPerg.length; perg++) //para cada pergunta
    {
        var dictionary = new Object();
        var questionData = {};
        var item = listPerg[perg];

        questionData["TopicId"] = id;

        //pega todas os conjuto de respostas
        var conjRespostas = item.getElementsByClassName('resposta');

        //para cada conjunto pegas as resposta
        for (conj = 0; conj < conjRespostas.length; conj++) {

            var rdb = item.getElementsByClassName('rdbPergExam')[conj];

            if (rdb.checked == true)
            {
                listChoice.push(rdb.value);
            }
        }
        if (listChoice.length > 0) {
            questionData["Choice"] = listChoice;
            listQuestion.push(questionData);
        }
    }

    if (listQuestion.length > 0)
    {
        $.ajax({
            url: "/api/topic/FinishExam",
            data: JSON.stringify(questionData),
            type: "POST",
            contentType: "application/json;charset=utf-8",
            statusCode: {
                200: function (newData) {                  
                    var porcentagem = (newData/listQuestion.length)*100;
                        alert("Sua nota foi de " + newData + " com " + porcentagem + "% de acerto.");
                    }
                },

                500: function () {
                    //erro
                    window.alert("Não foi possível enviar seu comentário. Tente novamente mais tarde.");
                }
        });
    }
}

function ajaxDeleteTopic(id, idChn)
{
    $.ajax({
        url: "/api/topic/DeleteTopic/" + id,
        type: "DELETE",
        contentType: "application/json;charset=utf-8",
        statusCode: {
            200: function (newData) {
                if (newData == true) {
                    window.location.href = "/channel/index/" + idChn;
                }
                else
                {
                    window.alert("Você não possui permissão para realizar essa operação.");
                }
            }
        },

        500: function () {
            //erro
            window.alert("Não foi possível enviar seu comentário. Tente novamente mais tarde.");
        }
    });
}

function ajaxFavoritedTopic(id, typePage)
{
    $.ajax({
        url: "/api/topic/TopicFavorite/" + id,
        type: "POST",
        contentType: "application/json;charset=utf-8",
        statusCode: {
            200: function (newData) {
               

                if (newData == true) {
                    if (typePage == 'pageTopic') {
                        var count = document.getElementById('lblCountFavorite').innerHTML;
                        document.getElementById('pFavoritar').innerHTML = "Desfavoritar";
                        count = parseInt(count) + 1;
                        document.getElementById('lblCountFavorite').innerHTML = count;
                    }
                    else
                    {
                        document.getElementById('imgFvt_' + id).src = "/images/utils/starv.png";
                        document.getElementById('divTpfvt_' + id).className = "btnRightTopicIsFavorited-grade";
                    }
                }
                else {
                    if (typePage == 'pageTopic') {
                        var count = document.getElementById('lblCountFavorite').innerHTML;
                        document.getElementById('pFavoritar').innerHTML = "Favoritar";
                        count = parseInt(count) - 1;
                        document.getElementById('lblCountFavorite').innerHTML = count;
                    }
                    else
                    {
                        document.getElementById('imgFvt_' + id).src = "/images/utils/starc.png";
                        document.getElementById('divTpfvt_' + id).className = "btnRightTopic-grade";
                    }
                }
            }
        },

        500: function () {
            //erro
            window.alert("Não foi possível enviar seu comentário. Tente novamente mais tarde.");
        }
    });
}

function ajaxLikeUnLike(id, type, userCondition)
{
    $.ajax({
        url: "/api/topic/LikeTopic/" + id + "?type=" + type,
        type: "POST",
        contentType: "application/json;charset=utf-8",
        statusCode: {
            200: function (newData) {
                //true= like; false = unlike, null = erro
                var countLike = document.getElementById('lblLike').innerHTML;
                var countNLike = document.getElementById('lblUnLike').innerHTML;
                countLike = parseInt(countLike);
                countNLike = parseInt(countNLike);

                if (newData == true) { //deu like
                    document.getElementById('linkLike').onclick = function () { };

                    if (userCondition == 'deulike') { //coloca método de unlike
                        document.getElementById('linkUnLike').onclick = function () {
                            ajaxLikeUnLike(id, "unlike", "deuunlike");
                        };
                        countLike = countLike + 1;
                        countNLike = countNLike - 1; //retira o unlike anterior
                    }
                    else if (userCondition == 'null')
                    {
                        countLike = countLike + 1; //nunca votou msm
                    }                   
                }
                else {
                    document.getElementById('linkUnLike').onclick = function () { };

                    if (userCondition == 'deuunlike') {
                        document.getElementById('linkLike').onclick = function () {
                            ajaxLikeUnLike(id, "like", "deulike");
                        };

                        countNLike = countNLike + 1;
                        countLike = countLike - 1;
                    }
                    else if (userCondition == 'null') {
                        countNLike = countNLike - 1;
                    }
                }
                document.getElementById('lblLike').innerHTML = countLike;
                document.getElementById('lblUnLike').innerHTML = countNLike;
            }
        },

        500: function () {
            //erro
            window.alert("Não foi possível enviar seu comentário. Tente novamente mais tarde.");
        }
    });
}

function ajaxReportTopic(idUserReporter, idUserReported, idTopic) {

    var text = document.getElementById('txtJustificativa').value;

    if (text.replace(" ", "") != '' && text.length > 10) {
        ajaxData = {};
        ajaxData['Justification'] = text;
        ajaxData['UserReportedId'] = idUserReported; //foi reportado
        ajaxData['UserReporterId'] = idUserReporter;
        ajaxData['TopicReportedId'] = idTopic;
        ajaxData['TopicReportedId'] = idTopic;

        $.ajax({
            url: "/api/report/ReportTopic/",
            data: JSON.stringify(ajaxData),
            type: "POST",
            contentType: "application/json;charset=utf-8",
            statusCode: {
                200: function (newData) {
                    if (newData.Id > 0) {
                        window.alert("Operação realizada com sucesso!");
                        document.getElementById('txtJustificativa').value = '';
                    }
                },

                500: function () {
                    //erro
                    window.alert("Não foi possível realizar esta operação. Tente novamente mais tarde.");
                }
            }
        });

    }
    else {
        document.getElementById('lblAvisoJustificativa').innerHTML = "* Campo obrigatório! Verifique se sua mensagem contém pelo menos 10 caracteres.";
    }
}