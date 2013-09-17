﻿using Nimbus.Web.API.Models.Comment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nimbus.Web.API.Models.Topic
{
    /// <summary>
    /// Tipos de tópicos permitidos
    /// </summary>
    public enum TopicType
    {
        Text,
        Video,
        Discussion,
        Exam,
        Add
    }

    /// <summary>
    /// Tipo de visualizações dos tópicos de um canal
    /// </summary>
    public enum TopicList
    {
        byAll,
        byModified,
        byPopularity
    }

    public class ShowTopicAPI
    {    
        public TopicExamAPI Exam { get; set; }
        public GeneralTopicAPI generalTopic { get; set; }
    }

    /// <summary>
    /// Criar um novo tópico
    /// </summary>
    public class NewTopicAPIModel
    {
        public string TopicName { get; set; }
        public TopicType TopicType { get;set;}
        public string TopicText {get;set;}
        public string TopicURLVideo {get;set;}
        public string TopicDiscussion{get;set;}
        public List<QuestionTopicAPI> TopicQuestionList { get; set; }
        public string UrlImgTopic { get; set; }
        public string UrlImgBanne { get; set; }
        public string ShortDescriptionTopic { get; set; }
        public List<RelatedTopicAPI> newRelateTopic { get;set;}

    }

    /// <summary>
    /// Retorna todas as informações de um tópico de vídeo
    /// </summary>
    public class GeneralTopicAPI
    {
        public int topic_ID {get;set;}
        public string TopicName{get;set;}
        public string TopicType{get;set;}
        public string TopicContent {get;set;}
        public string UrlImgTopic{get;set;}
        public string UrlImgBanner { get; set; }
        public string ShortDescriptionTopic { get; set; }
        public int CountFavorites { get; set; }
        public List<RelatedTopicAPI> RelatedTopicList { get; set; }
        public List<CommentAPIModel> Comments { get; set; }
    }

    /// <summary>
    /// Retorna todas as informações de um tópico de avaliação
    /// </summary>
    public class TopicExamAPI
    {
        public int topic_ID { get; set; }
        public string TopicName { get; set; }
        public string TopicType { get; set; }
        public List<QuestionTopicAPI> Questions { get; set; }
        public string UrlImgTopic { get; set; }
        public string UrlImgBanner { get; set; }
        public string ShortDescriptionTopic { get; set; }
        public List<RelatedTopicAPI> RelatedTopicList { get; set; }
        public List<CommentAPIModel> Comments { get; set; }
        public UserExamAPI examDone { get; set; }
    }

    /// <summary>
    /// retorna as informações básicas de um tópico, 
    /// serve para preencher as exibições/listas de tópicos.
    /// Quando for anúncios externos UrlAdd vem preenchido com a url do anunciante.
    /// </summary>
    public class AbstractTopicAPI
    {
        public int topic_ID { get; set; }
        public string UrlImgTopic { get; set; }
        public string shortTextTopic { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public int Count { get; set; }
        public DateTime ModifiedOn { get; set; }
    }
    
    /// <summary>
    /// Questão + Lista de opções e resposta correda da questão
    /// </summary>
    public class QuestionTopicAPI
    {
        public string Question { get; set; }
        public Dictionary<int , string> Options { get; set; }
        public int correctOption_ID { get; set; }
    }


    /// <summary>
    /// Preenche dados para os tópicos relacionados
    /// </summary>
    public class RelatedTopicAPI
    {
        public int topic_ID{get;set;}
        public TopicType TopicType { get; set; }
        public string TopicName{get;set;}
        public string UrlImgTopic{get;set;}
    }

    /// <summary>
    /// Método para retornar o aviso que o teste já foi realizado, quando o usuário acessar um tópico do tipo exame
    /// </summary>
    public class UserExamAPI
    {
        public int Grade { get; set; }
        public DateTime dateRealized { get; set; }
        public int ExamID { get; set; }
    }
    

}