using Nimbus.Web.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ServiceStack.OrmLite;
using Nimbus.Model.ORM;
using Nimbus.Model.Bags;
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
        public Comment NewComment(Comment comment)
        {
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    using (var trans = db.OpenTransaction(System.Data.IsolationLevel.ReadCommitted))
                    {
                        try
                        {
                            bool isOwner = db.SelectParam<Role>(r => r.ChannelId == comment.ChannelId).Exists(u => u.UserId == NimbusUser.UserId
                                                                                                                                  && u.IsOwner == true);

                            bool isManager = db.SelectParam<Role>(r => r.ChannelId == comment.ChannelId).Exists(u => u.UserId == NimbusUser.UserId &&
                                                                                     (u.TopicManager == true || u.ChannelMagager == true));
                            comment.Text = HttpUtility.HtmlEncode(comment.Text);

                            //caso o usuário seja adm/dono deve marcar como IsAnswer true, pois na hora de mostrar no canal quais sao os novos comentários
                            // o método irá ignorar as 'respostas' (pois foram realizadas pelo próprio usuário)
                            if (isManager || isOwner == true)
                                comment.IsAnswer = true;
                            else
                                comment.IsAnswer = false;

                            comment.ParentId = null;
                            comment.PostedOn = DateTime.Now;
                            comment.UserId = NimbusUser.UserId;
                            comment.Visible = true;
                            comment.IsNew = true;
                            db.Insert(comment);

                            comment.Id = (int)db.GetLastInsertId();
                            trans.Commit();
                        }
                        catch (Exception ex)
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
                                // o método irá ignorar as 'respostas' (pois foram realizadas pelo próprio usuário)
                                    if (isManager || isOwner == true)
                                        answer.IsAnswer = true;
                                    else
                                        answer.IsAnswer = false;

                                    db.Insert(answer);
                                    answer.Id = (int)db.GetLastInsertId();
                                   
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
        public CommentBag DeleteComment(Comment comment)
        {
            
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    using (var trans = db.OpenTransaction(System.Data.IsolationLevel.ReadCommitted))
                    {
                        try
                        {
                            //se ele é pai => apaga ele e os filhos
                            // se ele é um filho => deixa visible, coloca foto do usuario como avatar padrao, tira o nome e coloca texto como: comentario removido
                            Comment cmt = new Comment();
                            CommentBag bag = new CommentBag();
                            cmt = db.SelectParam<Comment>(c => c.Id == comment.Id).FirstOrDefault();
                            if (cmt.ParentId > 0) // é filho
                            {

                                cmt.Text = "Comentário removido";

                                db.Update<Comment>(cmt, c => c.Id == cmt.Id);
                                db.Save(cmt);

                                bag.Text = cmt.Text;
                                bag.ParentId = cmt.ParentId;
                                bag.AvatarUrl = "/images/Utils/person_icon.png";
                                bag.UserName = "Nome do usuário";
                            }
                            else
                            {
                                List<int> idChilds = db.SelectParam<Comment>(c => c.ParentId == comment.Id).Select(c => c.Id).ToList();
                                foreach (int item in idChilds)
                                {
                                    var dado = new Comment()
                                    {
                                        Visible = false
                                    };
                                    db.Update<Comment>(dado, c => c.Id == item);
                                    db.Save(dado);
                                }
                                cmt.Visible = false;
                                db.Update<Comment>(cmt, c => c.Id == cmt.Id);
                                db.Save(cmt);

                                bag.Id = cmt.Id;
                                bag.ParentId = cmt.ParentId;
                            }
                         
                            trans.Commit();
                            return bag;
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
                    //pega todos comentários 'pai'
                    List<Comment> comment = db.SelectParam<Comment>(cmt => cmt.TopicId == id && cmt.Visible == true && cmt.ParentId == null);

                    foreach (Comment item in comment)
                    {
                        User user = db.SelectParam<User>(u => u.Id == item.UserId).FirstOrDefault();

                        //busco todos os filhos desse comentário
                        List<Comment> cmtChild = db.SelectParam<Comment>(c => c.ParentId == item.Id && item.Visible == true);
                        List<CommentBag> listChild = new List<CommentBag>();                        
                        
                        foreach (var itemChild in cmtChild)
                        {                            
                            User userChild = db.SelectParam<User>(u => u.Id == itemChild.UserId).FirstOrDefault();
                            string name = "";
                            if (itemChild.Text == "Comentário removido")
                            {
                                userChild.AvatarUrl = "/images/Utils/person_icon.png";
                                name = "Nome do usuário";

                            }
                            else
                                name = HttpUtility.HtmlDecode(userChild.FirstName + " " + userChild.LastName);
                            CommentBag child = new CommentBag()
                            {
                                AvatarUrl = userChild.AvatarUrl,
                                UserName = name,
                                UserId = userChild.Id,
                                Id = itemChild.Id,
                                Text = HttpUtility.HtmlDecode(itemChild.Text),
                                ParentId = itemChild.ParentId,
                                PostedOn = itemChild.PostedOn,
                                IsNew = itemChild.IsNew,
                                IsAnswer = itemChild.IsAnswer,
                                TopicId = itemChild.TopicId,
                                IsParent = false,
                                ChannelId = itemChild.ChannelId,
                                TopicName = HttpUtility.HtmlDecode(db.SelectParam<Topic>(t => t.Id == itemChild.TopicId).Select(t => t.Title).FirstOrDefault())
                            };
                            listChild.Add(child);
                        }

                        //crio o objeto para o comentario 
                        CommentBag bag = new CommentBag()
                        {                            
                            AvatarUrl = user.AvatarUrl,
                            UserName = HttpUtility.HtmlDecode(user.FirstName + " " + user.LastName),
                            UserId = user.Id,
                            Id = item.Id,
                            Text = HttpUtility.HtmlDecode(item.Text),
                            ParentId = item.ParentId,
                            PostedOn = item.PostedOn,
                            IsNew = item.IsNew,
                            IsAnswer = item.IsAnswer,
                            TopicId = item.TopicId,
                            IsParent = item.ParentId > 0 ? false : true,
                            ChannelId = item.ChannelId,
                            CommentChild = listChild,
                            TopicName =HttpUtility.HtmlDecode(db.SelectParam<Topic>(t => t.Id == item.TopicId).Select(t => t.Title).FirstOrDefault())
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
                    using (var trans = db.OpenTransaction(System.Data.IsolationLevel.ReadCommitted))
                    {
                        try
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

                                    //para cada comentário que vai ser mostrado, atualizar o BD sinalizando-o como não novo.
                                    var dado = new Nimbus.Model.Comment() { IsNew = false };
                                    db.Update<Nimbus.Model.Comment>(dado, cmt => cmt.Id == item.Id);
                                    trans.Commit();
                                }
                            }
                        }
                        catch( Exception ex)
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
            return listComments;
        }

    }
}
