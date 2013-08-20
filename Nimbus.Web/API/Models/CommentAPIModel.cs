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
        public int userComment_ID { get; set; }
        public string UrlImgUser { get; set; }
        public string userCommentName { get; set; }
        public string TextComment { get; set; }
        public int commentFather_ID { get; set; }
        public DateTime DateComment { get; set; }

    }
}