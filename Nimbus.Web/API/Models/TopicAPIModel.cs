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
        text,
        video,
        discussion,
        exam,
        add
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
    public class TopicVideoAPI
    {
        public int topic_ID {get;set;}
        public string TopicName{get;set;}
        public TopicType TopicType{get;set;}
        public string TopicURLVideo {get;set;}
        public string UrlImgTopic{get;set;}
        public string UrlImgBanner { get; set; }
        public string ShortDescriptionTopic { get; set; }
        public int CountFavorites { get; set; }
        public List<RelatedTopicAPI> RelatedTopicList { get; set; }
    }

    /// <summary>
    /// Retorna todas as informações de um tópico de texto
    /// </summary>
    public class TopicTextAPI
    {
        public int topic_ID { get; set; }
        public string TopicName { get; set; }
        public TopicType TopicType { get; set; }
        public string TopicText { get; set; }
        public string UrlImgTopic { get; set; }
        public string UrlImgBanner { get; set; }
        public string ShortDescriptionTopic { get; set; }
        public int CountFavorites { get; set; }
        public List<RelatedTopicAPI> RelatedTopicList { get; set; }
    }

    /// <summary>
    /// Retorna todas as informações de um tópico de discussão
    /// </summary>
    public class TopicDiscussionAPI
    {
        public int topic_ID { get; set; }
        public string TopicName { get; set; }
        public TopicType TopicType { get; set; }
        public string TopicDiscussion { get; set; }
        public string UrlImgTopic { get; set; }
        public string UrlImgBanner { get; set; }
        public string ShortDescriptionTopic { get; set; }
        public int CountFavorites { get; set; }
        public List<RelatedTopicAPI> RelatedTopicList { get; set; }
    }

    /// <summary>
    /// Retorna todas as informações de um tópico de avaliação
    /// </summary>
    public class TopicExamAPI
    {
        public int topic_ID { get; set; }
        public string TopicName { get; set; }
        public TopicType TopicType { get; set; }
        public List<QuestionTopicAPI> TopicExam { get; set; }
        public string UrlImgTopic { get; set; }
        public string UrlImgBanner { get; set; }
        public string ShortDescriptionTopic { get; set; }
        public List<RelatedTopicAPI> RelatedTopicList { get; set; }
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
        public List<OptionQuestionaPI> Options { get; set; }
        public int correctOption_ID { get; set; }

    }

    /// <summary>
    /// Todas as opções de uma questão
    /// </summary>
    public class OptionQuestionaPI
    {
        public string TextOptionQuestion { get; set; }
        public int question_ID { get; set; }
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
    /// retorna uma lista com os comentários feitos para o tópico
    /// </summary>
    public class CommentTopicAPI 
    {
        public List<Models.Comment.CommentAPIModel> CommentTopic { get; set; }
    }




}