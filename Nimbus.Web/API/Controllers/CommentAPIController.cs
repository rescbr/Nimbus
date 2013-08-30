using Nimbus.Web.API.Models;
using Nimbus.Web.API.Models.Comment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ServiceStack.OrmLite;

namespace Nimbus.Web.API.Controllers
{
    /// <summary>
    /// Controle sobre todas as funções realizadas para os comentários de canais e tópicos.
    /// </summary>
    public class CommentAPIController : NimbusApiController
    {
        /// <summary>
        /// Cria um comentario 
        /// </summary>
        /// <param name="comment"></param>
        /// <returns></returns>
        [Authorize]
        public string newComment(newCommentAPIModel comment )
        {
            AlertGeneral  msg = new AlertGeneral();
            string alert = msg.ErrorMessage;

            try
            {
                using(var db = DatabaseFactory.OpenDbConnection())
                {
                    var dados = new Nimbus.DB.Comment 
                                               {
                                                   ParentId = 0,
                                                   PostedOn = DateTime.Now,
                                                   Text = comment.Comment,
                                                   TopicId = comment.topic_ID,
                                                   UserId  = NimbusUser.UserId,
                                                   Visible = true
                                               };
                    alert = "";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return alert;
        }
        
        /// <summary>
        /// Cria uma resposta a um comentario, deve ter um idPai
        /// </summary>
        /// <param name="answer"></param>
        /// <returns></returns>
        [Authorize]
        public string answerComment(answerCommentAPIModel answer)
        {
            AlertGeneral msg = new AlertGeneral();
            string alert = msg.ErrorMessage;

            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    var dados = new Nimbus.DB.Comment
                    {
                        ParentId = answer.parent_ID,
                        PostedOn = DateTime.Now,
                        Text = answer.Comment,
                        TopicId = answer.topic_ID,
                        UserId = NimbusUser.UserId,
                        Visible = true
                    };
                    alert = "";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return alert;
        }
        
        /// <summary>
        /// Troca a visibilidade/excluir de um comentario
        /// </summary>
        /// <param name="item_ID"></param>
        /// <returns></returns>
        [Authorize]
        public bool deleteComment(int item_ID)
        {
            bool success = false;
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    var dado = new Nimbus.DB.Comment()
                                  { Visible = false };

                    db.Update<Nimbus.DB.Comment>(dado, cmt => cmt.Id == item_ID);
                    db.Save(dado);
                    success = true;
                }
            }
            catch (Exception ex)
            {                
                throw ex;
            }
            return success;
        }
        
        /// <summary>
        /// Visualizar todos os comentarios de um tópico
        /// </summary>
        /// <param name="topicID"></param>
        /// <returns></returns>
        [Authorize]
        public List<CommentAPIModel> showComment(int topicID)
        {
            List<CommentAPIModel> listComments = new List<CommentAPIModel>();
            try
            {
                using(var db= DatabaseFactory.OpenDbConnection())
                {

                    listComments = db.Select<CommentAPIModel>("SELECT User.Id, User.AvatarUrl, User.Name," +
                                                            "       Comment.Id as comment_ID, Comment.Text, Comment.ParentID, Comment.PostedOn, Comment.TopicId" +
                                                            "FROM User, Comment " +
                                                            "WHERE Comment.Id = {0} AND Comment.Visible = true", topicID
                                                             );
                }
            }
            catch (Exception ex)
            {                
                throw ex;
            }
            return listComments;
        }
        
      





    }
}
