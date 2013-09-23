using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nimbus.Web.API.Models.Comment
{
    /// <summary>
    /// Exibe os  comentários tópico.
    /// </summary>
    public class CommentAPIModel
    {
        public string AvatarUrl { get; set; }
        public string Name { get; set; }
        public string userID { get; set; }

        public int comment_ID { get; set; }
        public string Text { get; set; }
        public int ParentID { get; set; }
        public DateTime PostedOn { get; set; }
        public int TopicId { get; set; }
    }

    /// <summary>
    /// Método para criar um comentário 
    /// </summary>
    public class newCommentAPIModel
    {
        public int topic_ID { get; set; }
        public string Comment { get; set; }
    }

    /// <summary>
    /// Método para responder um comentário, tornando-se a resposta um comentario 'filho'
    /// </summary>
    public class answerCommentAPIModel
    {
        public int topic_ID { get; set; }
        public string Comment { get; set; }
        public int parent_ID { get; set; }
    }
}