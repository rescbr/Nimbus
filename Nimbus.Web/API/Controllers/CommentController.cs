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
    [NimbusAuthorize]
    public class CommentController : NimbusApiController
    {
        /// <summary>
        /// Cria um comentario 
        /// </summary>
        /// <param name="comments"></param>
        /// <returns></returns>
        [HttpPost]
        public Comment NewComment(Comment comment)
        {
            //A diferença entre NewComment e AnswerComment é que ParentId = null.
            comment.ParentId = null;
            return AnswerComment(comment);
        }

        /// <summary>
        /// Cria uma resposta a um comentario, deve ter um idPai
        /// </summary>
        /// <param name="answer"></param>
        /// <returns></returns>
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
                                var role = db.SelectParam<Role>(r => r.ChannelId == answer.ChannelId).ToList();

                                bool isOwner = role.Exists(u => u.UserId == NimbusUser.UserId && u.IsOwner == true);
                                bool isManager = role.Exists(u => u.UserId == NimbusUser.UserId && (u.TopicManager == true || u.ChannelMagager == true));

                                //Verifica se existe um comentario pai e ele está visível, caso contrário, faz virar pai
                                Comment parentComment = db.Where<Comment>(c => c.Id == answer.ParentId).FirstOrDefault();
                                if (parentComment == null || parentComment.Visible == false)
                                    answer.ParentId = null;

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

            //deu tudo certo, passa a bola pra notificação notificar
            var cNotif = new Notifications.CommentNotification();
            cNotif.NewComment(answer);
            return answer;
        }

        /// <summary>
        /// Troca a visibilidade/excluir de um comentario
        /// </summary>
        /// <param name="item_ID"></param>
        /// <returns></returns>
        [HttpDelete]
        public CommentBag DeleteComment(Comment comment)
        {
            //TODO: Adicionar restrição!
            //if (Model.CurrentUser.UserId == Model.CurrentChannel.OwnerId ||
            //                  comments.UserId == Model.CurrentUser.UserId || Model.RolesCurrentUser.Contains("channelmanager")
            //                                                             || Model.RolesCurrentUser.Contains("topicmanager"))
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    using (var trans = db.OpenTransaction(System.Data.IsolationLevel.ReadCommitted))
                    {
                        try
                        {
                            //se ele é pai => apaga ele e os filhos
                            // se ele é um filho => deixa visible, coloca fo to do usuario como avatar padrao, tira o nome e coloca texto como: comentario removido
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
                                IEnumerable<int> idChilds = db.SelectParam<Comment>(c => c.ParentId == comment.Id).Select(c => c.Id);
                                foreach (int item in idChilds)
                                {
                                    Comment ct = new Comment();
                                    ct = db.SelectParam<Comment>(c => c.Id == item).FirstOrDefault();
                                    if (ct != null)
                                    {
                                        ct.Visible = false;
                                        db.Update<Comment>(ct, c => c.Id == item);
                                        db.Save(ct);
                                    }
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
        }

        /// <summary>
        /// Pega um comentario sem filhos
        /// </summary>
        /// <param name="id">id do comentario</param>
        /// <returns>um comentario, uai</returns>
        [HttpGet]
        public CommentBag GetComment(int id)
        {
            using (var db = DatabaseFactory.OpenDbConnection())
            {
                Comment comment = db.Where<Comment>(c => c.Id == id).FirstOrDefault();
                if (comment == null) throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.NotFound, "Comment not found"));

                Channel chn = db.Where<Channel>(c => c.Id == comment.ChannelId).FirstOrDefault();
                User user = db.Where<User>(u => u.Id == comment.UserId).FirstOrDefault();
                ChannelController cc = ClonedContextInstance<ChannelController>();
                var userRoles = cc.ReturnRolesUser(chn.Id);
                bool isOwnerOrManager = (NimbusUser.UserId == chn.OwnerId ||
                    userRoles.Contains("channelmanager") ||
                    userRoles.Contains("topicmanager"));

                CommentBag bag = new CommentBag()
                {
                    AvatarUrl = user.AvatarUrl,
                    UserName = HttpUtility.HtmlDecode(user.FirstName + " " + user.LastName),
                    UserId = user.Id,
                    Id = comment.Id,
                    Text = HttpUtility.HtmlDecode(comment.Text),
                    ParentId = comment.ParentId,
                    PostedOn = comment.PostedOn,
                    IsNew = comment.IsNew,
                    IsAnswer = comment.IsAnswer,
                    TopicId = comment.TopicId,
                    IsParent = comment.ParentId > 0 ? false : true,
                    ChannelId = comment.ChannelId,
                    CommentChild = null,
                    IsDeletable = (user.Id == NimbusUser.UserId || isOwnerOrManager)
                };

                return bag;
            }
        }

        [HttpGet]
        public CommentBag GetParentComment(int id)
        {
            using (var db = DatabaseFactory.OpenDbConnection())
            {
                //pega os comentarios e usuarios de uma vez so
                var parentComment = db.Where<Comment>(c => c.Id == id && c.Visible == true)
                    .Select(s => new CommentUser()
                    {
                        Comment = s,
                        User = db.Where<User>(u => u.Id == s.UserId).FirstOrDefault()
                    });

                return BuildListCommentBag(parentComment).FirstOrDefault();
            }
        }

        /// <summary>
        /// Retorna lista de todos os comentários do tópico
        /// </summary>
        /// <param name="id">id do topico</param>
        /// <returns></returns>
        [HttpGet]
        public List<CommentBag> AllTopicComments(int id)
        {
            using (var db = DatabaseFactory.OpenDbConnection())
            {
                //pega os comentarios e usuarios de uma vez so
                var topicComments = db.Where<Comment>(c => c.TopicId == id && c.Visible == true)
                    .Select(s => new CommentUser(){ 
                        Comment = s, 
                        User = db.Where<User>(u => u.Id == s.UserId).FirstOrDefault()
                    });

                var commentBags = BuildListCommentBag(topicComments);
                if (commentBags == null) return new List<CommentBag>(); // retorna lista vazia
                else return commentBags;
            }
        }

        List<CommentBag> BuildListCommentBag(IEnumerable<CommentUser> comments)
        {
            using (var db = DatabaseFactory.OpenDbConnection())
            {
                if (comments.Count() > 0)
                {
                    Channel chn = db.Where<Channel>(c => c.Id == comments.FirstOrDefault().Comment.ChannelId).FirstOrDefault();
                    bool isOwnerOrManager = IsCurrentUserOwnerOrManager(chn);

                    Dictionary<int, CommentBag> parentComments = new Dictionary<int, CommentBag>();
                    foreach (var comment in comments)
                    {
                        if (comment.Comment.ParentId == null)
                        {
                            #region Adiciona novo comentario pai
                            parentComments[comment.Comment.Id] = new CommentBag()
                            {
                                AvatarUrl = comment.User.AvatarUrl,
                                UserName = HttpUtility.HtmlDecode(comment.User.FirstName + " " + comment.User.LastName),
                                UserId = comment.User.Id,
                                Id = comment.Comment.Id,
                                Text = HttpUtility.HtmlDecode(comment.Comment.Text),
                                ParentId = comment.Comment.ParentId,
                                PostedOn = comment.Comment.PostedOn,
                                IsNew = comment.Comment.IsNew,
                                IsAnswer = comment.Comment.IsAnswer,
                                TopicId = comment.Comment.TopicId,
                                IsParent = comment.Comment.ParentId > 0 ? false : true,
                                ChannelId = comment.Comment.ChannelId,
                                CommentChild = new List<CommentBag>(),
                                IsDeletable = (comment.User.Id == NimbusUser.UserId || isOwnerOrManager)
                            };
                            #endregion
                        }
                        else
                        {
                            #region adiciona novo comentario filho no comentario pai
                            int parentKey = comment.Comment.ParentId.Value;
                            if (parentComments.ContainsKey(parentKey))
                            {
                                parentComments[parentKey].CommentChild.Add(new CommentBag()
                                {
                                    AvatarUrl = comment.User.AvatarUrl,
                                    UserName = HttpUtility.HtmlDecode(comment.User.FirstName + " " + comment.User.LastName),
                                    UserId = comment.User.Id,
                                    Id = comment.Comment.Id,
                                    Text = HttpUtility.HtmlDecode(comment.Comment.Text),
                                    ParentId = comment.Comment.ParentId,
                                    PostedOn = comment.Comment.PostedOn,
                                    IsNew = comment.Comment.IsNew,
                                    IsAnswer = comment.Comment.IsAnswer,
                                    TopicId = comment.Comment.TopicId,
                                    IsParent = comment.Comment.ParentId > 0 ? false : true,
                                    ChannelId = comment.Comment.ChannelId,
                                    CommentChild = null,
                                    IsDeletable = (comment.User.Id == NimbusUser.UserId || isOwnerOrManager)
                                });
                            }
                            #endregion
                        }
                    }

                    return parentComments.Values.ToList();
                }
                else return null;
            }
        }

        bool IsCurrentUserOwnerOrManager(Channel chn)
        {
            ChannelController cc = ClonedContextInstance<ChannelController>();
            var userRoles = cc.ReturnRolesUser(chn.Id);
            bool isOwnerOrManager = (NimbusUser.UserId == chn.OwnerId ||
                userRoles.Contains("channelmanager") ||
                userRoles.Contains("topicmanager"));

            return isOwnerOrManager;
        }

        /// <summary>
        /// Visualizar todos os comentarios de um tópico
        /// </summary>
        /// <param name="topicID"></param>
        /// <returns></returns>
        [HttpGet]
        public List<CommentBag> ShowTopicComment(int id, int skip)
        {
            List<CommentBag> listComments = new List<CommentBag>();
            using (var db = DatabaseFactory.OpenDbConnection())
                {
                    //pega todos comentários 'pai'
                    List<Comment> comments = db.SelectParam<Comment>(cmt => cmt.TopicId == id && cmt.Visible == true && cmt.ParentId == null).Skip(5* skip).Take(5).ToList();
                  
                    if (comments.Count > 0)
                    {
                        Channel chn = db.Where<Channel>(c => c.Id == comments.FirstOrDefault().ChannelId).FirstOrDefault();
                        ChannelController cc = ClonedContextInstance<ChannelController>();
                        var userRoles = cc.ReturnRolesUser(chn.Id);
                        bool isOwnerOrManager = (NimbusUser.UserId == chn.OwnerId ||
                            userRoles.Contains("channelmanager") ||
                            userRoles.Contains("topicmanager"));

                        foreach (Comment item in comments)
                        {
                            User user = db.SelectParam<User>(u => u.Id == item.UserId).FirstOrDefault();

                            //busco todos os filhos desse comentário
                            List<Comment> cmtChild = db.SelectParam<Comment>(c => c.ParentId == item.Id && item.Visible == true).Take(3).ToList();
                            List<CommentBag> listChild = new List<CommentBag>();

                            foreach (var itemChild in cmtChild)
                            {
                                User userChild = db.SelectParam<User>(u => u.Id == itemChild.UserId).FirstOrDefault();
                                string name = "";
                                if (itemChild.Text == "Comentário removido")
                                {
                                    userChild.AvatarUrl = "/images/Utils/person_icon.png";
                                    name = "[removido]";
                                }
                                else
                                    name = userChild.FirstName + " " + userChild.LastName;

                                CommentBag child = new CommentBag()
                                {
                                    AvatarUrl = userChild.AvatarUrl,
                                    UserName = name,
                                    UserId = userChild.Id,
                                    Id = itemChild.Id,
                                    Text = itemChild.Text,
                                    ParentId = itemChild.ParentId,
                                    PostedOn = itemChild.PostedOn,
                                    IsNew = itemChild.IsNew,
                                    IsAnswer = itemChild.IsAnswer,
                                    TopicId = itemChild.TopicId,
                                    IsParent = false,
                                    ChannelId = itemChild.ChannelId,
                                    IsDeletable = (userChild.Id == NimbusUser.UserId || isOwnerOrManager)
                                };
                                listChild.Add(child);
                            }

                            //crio o objeto para o comentario 
                            CommentBag bag = new CommentBag()
                            {
                                AvatarUrl = user.AvatarUrl,
                                UserName = user.FirstName + " " + user.LastName,
                                UserId = user.Id,
                                Id = item.Id,
                                Text =item.Text,
                                ParentId = item.ParentId,
                                PostedOn = item.PostedOn,
                                IsNew = item.IsNew,
                                IsAnswer = item.IsAnswer,
                                TopicId = item.TopicId,
                                IsParent = item.ParentId > 0 ? false : true,
                                ChannelId = item.ChannelId,
                                CommentChild = listChild,
                                IsDeletable = (user.Id == NimbusUser.UserId || isOwnerOrManager)
                            };
                            listComments.Add(bag);
                        }
                    }
                }          
            
            return listComments;
        }

        /// <summary>
        /// Método que mostra para o usuário dono/moderador do canal os comentarios novos que surgiram
        /// o método ignora as respostas realizada pelo dono/moderator
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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
                                                                                    && cmt.IsNew == true && cmt.ParentId == null);

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
                                    //var dado = new Nimbus.Model.Comment() { IsNew = false };
                                    //db.Update<Nimbus.Model.Comment>(dado, cmt => cmt.Id == item.Id);
                                    //trans.Commit();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            //trans.Rollback();
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


        public class CommentUser
        {
            public Comment Comment { get; set; }
            public User User { get; set; }
        }
    }
}
