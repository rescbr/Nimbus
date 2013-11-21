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
        text = CKEDITOR.instances.txtaArticle.getData();
    }
    if (topicType == 'discussion') {
        text = CKEDITOR.instances.txtaTextMsg.getData();
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
                            //TODO
                        }                                             

                        document.getElementById('divRenderEdit').style.display = 'none';
                        document.getElementById(divOld).style.display = 'block';

                        document.getElementById('iptNewTitle').value = newData.Title;
                        document.getElementById('imgNewPrevia').src = newData.ImgUrl;
                        document.getElementById('iptNewDescription').value = newData.Description;
                    }
                },

                400: function () {
                    //erro
                    window.alert("Não foi possível enviar seu comentário. Tente novamente mais tarde.");
                }
            }
        });

    }

}

function examEdit()
{  
        var questionData = {}
        var listQuestion = []
        var listNewPerg = document.getElementsByName('enunciado');
        var listOldPerg = document.getElementsByName('enunciadoOld');
        var isChecked = false;
        var isCheckedOld = false;

        //para novas perguntas
        for (perg = 0; perg < listNewPerg.length; perg++) //para cada pergunta
        {
            var item = listNewPerg[perg];
            questionData["TextQuestion"] = item.value; //pego o conteudo = enunciado   

            var currentPerg = item.id.replace("QuestionPerg", ""); //pega o indice da pergunta atual
            var listOpt = [];
            var dictionary = new Object();

            $("#divExam li").each(function () {//pego todas as opções novas
                var id = this.id;
                if (id.indexOf("liPerg") > -1) {
                    listOpt.push(id);
                }
            });

            for (option = 0; option < listOpt.length; option++) {
                var rdbOption = document.getElementById('rdbPerg' + currentPerg + '_opt' + (option + 1));
                var txtOption = document.getElementById('txtPerg' + currentPerg + '_opt' + (option + 1));

                if (txtOption.value != "Opção " + (option + 1) && txtOption.value != "") {
                    if (rdbOption.checked == true) {
                        questionData["CorrectAnswer"] = option + 1; //passa o indice da resposta certa + 1 .'. o for começa do zero                    
                        isChecked = true;
                    }
                    dictionary[option + 1] = txtOption.value; //dictionary<int, string>

                    questionData["ChoicesAnswer"] = dictionary;
                }
            }
            if (isChecked == true) {
                listQuestion.push(questionData);
            }
        }

            //para velhas perguntas
            for (perg = 0; perg < listOldPerg.length; perg++) //para cada pergunta
            {
                var item = listOldPerg[perg];
                questionData["TextQuestion"] = item.value; //pego o conteudo = enunciado   

                 if (item.style.display != "none") { 
                    var currentPerg = item.id.replace("QuestionPerg", ""); //pega o indice da pergunta atual

                    var listOldOpt = [];
                    var dictionary = new Object();

                    $("#divNewExam li").each(function () {//pego todas as opções que existiam
                        var id = this.id;
                        if (id.indexOf("liPerg_") > -1) {
                            listOldOpt.push(id);
                        }
                    });

                    for (option = 0; option < listOldOpt.length; option++) {

                        var rdbOption = document.getElementById('rdbPerg' + currentPerg + '_opt' + (option + 1));
                        var txtOption = document.getElementById('txtPerg' + currentPerg + '_opt' + (option + 1));

                        if (rdbOption.style.display != "none" && txtOption.style.display != "none") {
                            if (txtOption.value != "Opção " + (option + 1) && txtOption.value != "") {
                                if (rdbOption.checked == true) {
                                    questionData["CorrectAnswer"] = option + 1; //passa o indice da resposta certa + 1 .'. o for começa do zero                    
                                    isCheckedOld = true;
                                }
                                dictionary[option + 1] = txtOption.value; //dictionary<int, string>

                                questionData["ChoicesAnswer"] = dictionary;
                            }
                        }
                    }
                    if (isCheckedOld == true) {
                        listQuestion.push(questionData);
                    }
                }
            }

        return listQuestion;
}