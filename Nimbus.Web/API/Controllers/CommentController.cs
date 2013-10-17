using Nimbus.Web.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ServiceStack.OrmLite;
using Nimbus.DB.ORM;
using Nimbus.DB.Bags;

namespace Nimbus.Web.API.Controllers
{
    /// <summary>
    /// Controle sobre todas as funções realizadas para os comentários de canais e tópicos.
    /// </summary>
    public class CommentController : NimbusApiController
    {
        /// <summary>
        /// Cria um comentario 
        /// </summary>
        /// <param name="comment"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public Comment NewComment(Comment comment )
        {
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    comment.ParentId = 0;
                    comment.PostedOn = DateTime.Now;
                    comment.UserId = NimbusUser.UserId;
                    comment.Visible = true;
                    db.Insert(comment);
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }

            return comment;
        }
        
        /// <summary>
        /// Cria uma resposta a um comentario, deve ter um idPai
        /// </summary>
        /// <param name="answer"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public Comment AnswerComment(Comment answer)
        {            
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    answer.PostedOn = DateTime.Now;
                    answer.UserId = NimbusUser.UserId;
                    answer.Visible = true;                   
                    
                    db.Insert(answer);
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }

            return answer;
        }
        
        /// <summary>
        /// Troca a visibilidade/excluir de um comentario
        /// </summary>
        /// <param name="item_ID"></param>
        /// <returns></returns>
        [Authorize]
        [HttpDelete]
        public bool DeleteComment(int id)
        {
            bool success = false;
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    var dado = new Nimbus.DB.Comment()
                                  { Visible = false };

                    db.Update<Nimbus.DB.Comment>(dado, cmt => cmt.Id == id);
                    db.Save(dado);
                    success = true;
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
            return success;
        }
        
        /// <summary>
        /// Visualizar todos os comentarios de um tópico
        /// </summary>
        /// <param name="topicID"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public List<CommentBag> ShowTopicComment(int id)
        {
            List<CommentBag> listComments = new List<CommentBag>();
            try
            {
                using(var db= DatabaseFactory.OpenDbConnection())
                {
                    List<Comment> comment = db.SelectParam<Comment>(cmt => cmt.TopicId == id && cmt.Visible == true);

                    foreach (Comment item in comment)
                    {
                        User user = db.Select<User>("SELECT User.Id, User.AvatarUrl, User.FirstName, User.LastName FROM User WHERE User.Id = {0}", item.UserId).FirstOrDefault();
                        CommentBag bag = new CommentBag()
                        {
                            AvatarUrl = user.AvatarUrl,
                            Name = user.FirstName + " " + user.LastName,
                            UserId = user.Id,
                            Id = item.Id,
                            Text = item.Text,
                            ParentId = item.ParentId,
                            PostedOn = item.PostedOn,
                            TopicId = item.TopicId,
                            ChannelId = item.ChannelId
                        };
                        listComments.Add(bag);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
            return listComments;
        }

        [Authorize]
        [HttpGet]
        public List<CommentBag> ShowChannelComment(int id)
        {
            List<CommentBag> listComments = new List<CommentBag>();
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    List<Comment> comment = db.SelectParam<Comment>(cmt => cmt.Visible == true && cmt.ChannelId == id);

                    foreach (Comment item in comment)
                    {
                        User user = db.Select<User>("SELECT User.Id, User.AvatarUrl, User.FirstName, User.LastName FROM User WHERE User.Id = {0}", item.UserId).FirstOrDefault();
                        CommentBag bag = new CommentBag()
                        {
                            AvatarUrl = user.AvatarUrl,
                            Name = user.FirstName + " " + user.LastName,
                            UserId = user.Id,
                            Id = item.Id,
                            Text = item.Text,
                            ParentId = item.ParentId,
                            PostedOn = item.PostedOn,
                            TopicId = item.TopicId,
                            ChannelId = item.ChannelId                            
                        };
                        listComments.Add(bag);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
            return listComments;
        }

    }
}
