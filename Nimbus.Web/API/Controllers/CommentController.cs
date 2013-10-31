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
using System.Web;

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
                    comment.IsNew = true;
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
                    using (var trans = db.OpenTransaction(System.Data.IsolationLevel.ReadCommitted))
                    {
                        try
                        {
                            if (db.SelectParam<Channel>(c => c.Visible == true).Exists(c => c.Id == answer.ChannelId))
                            {
                                
                                    bool isOwner = db.SelectParam<Role>(r => r.ChannelId == answer.ChannelId).Exists(u => u.UserId == NimbusUser.UserId 
                                                                                                                          && u.IsOwner == true );

                                    bool isManager = db.SelectParam<Role>(r => r.ChannelId == answer.ChannelId).Exists(u => u.UserId == NimbusUser.UserId &&
                                                                                             (u.TopicManager == true || u.ChannelMagager == true));

                                    answer.Text = HttpUtility.HtmlEncode(answer.Text);
                                    answer.PostedOn = DateTime.Now;
                                    answer.UserId = NimbusUser.UserId;
                                    answer.Visible = true;
                                    answer.IsNew = true;
                                //caso o usuário seja adm/dono deve marcar como IsAnswer true, pois na hora de mostrar no canal quais sao os novos comentários
                                // o método irá ignorar as respostas (pois foram realizadas pelo próprio usuário)
                                    if (isManager || isOwner == true)
                                        answer.IsAnswer = true;
                                    else
                                        answer.IsAnswer = false;

                                    db.Insert(answer);
                                    answer.Id = (int)db.GetLastInsertId();

                                     var dado = new Nimbus.DB.Comment()
                                                  { IsNew = false };
                                    db.Update<Nimbus.DB.Comment>(dado, cmt => cmt.Id == answer.ParentId);
                                   
                                    trans.Commit();
                                
                            }
                        }
                        catch(Exception ex)
                        {
                            trans.Rollback();
                            throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
                        }
                    }
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
                        User user = db.SelectParam<User>(u => u.Id == item.UserId).FirstOrDefault();
                        
                        CommentBag bag = new CommentBag()
                        {
                            AvatarUrl = user.AvatarUrl,
                            UserName = user.FirstName + " " + user.LastName,
                            UserId = user.Id,
                            Id = item.Id,
                            Text = item.Text,
                            ParentId = item.ParentId,
                            PostedOn = item.PostedOn,
                            TopicId = item.TopicId,
                            ChannelId = item.ChannelId,
                            TopicName =db.SelectParam<Topic>(t => t.Id == item.TopicId).Select(t => t.Title).FirstOrDefault()
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

        /// <summary>
        /// Método que mostra para o usuário dono/moderador do canal os comentarios novos que surgiram
        /// o método ignora as respostas realizada pelo dono/moderator
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public List<CommentBag> ShowChannelComment(int id)
        {
            List<CommentBag> listComments = new List<CommentBag>();
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    if (db.SelectParam<Channel>(c => c.Visible == true).Exists(c => c.Id == id))
                    {
                        List<Comment> comment = db.SelectParam<Comment>(cmt => cmt.Visible == true && cmt.ChannelId == id
                                                                            && cmt.IsNew == true && cmt.IsAnswer == false);

                        foreach (Comment item in comment)
                        {
                            User user = db.SelectParam<User>(u => u.Id == item.UserId).FirstOrDefault();
                            CommentBag bag = new CommentBag()
                            {
                                AvatarUrl = user.AvatarUrl,
                                UserName = user.FirstName + " " + user.LastName,
                                UserId = user.Id,
                                Id = item.Id,
                                Text = item.Text,
                                ParentId = item.ParentId,
                                PostedOn = item.PostedOn,
                                TopicId = item.TopicId,
                                ChannelId = item.ChannelId,
                                TopicName = db.SelectParam<Topic>(t => t.Id == item.TopicId).Select(t => t.Title).FirstOrDefault()
                            };
                            listComments.Add(bag);
                        }
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
