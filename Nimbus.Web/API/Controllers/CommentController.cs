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
using Nimbus.Web.Utils;

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
            CommentBag bag = new CommentBag();
            using (var db = DatabaseFactory.OpenDbConnection())
            {
                Comment cmt = new Comment();
                cmt = db.SelectParam<Comment>(c => c.Id == comment.Id).FirstOrDefault();
                //pegar permissões
                Channel channel = db.Where<Channel>(c => c.Id == cmt.ChannelId && c.Visible == true).Where(c => c!= null).FirstOrDefault();
                var rolesUser = db.Where<Role>(r => r.ChannelId == cmt.ChannelId && r.UserId == NimbusUser.UserId).Where(r => r != null).FirstOrDefault();
                
                bool isAllow = false;
                if (rolesUser != null)
                {
                    isAllow = rolesUser.ChannelMagager == true || rolesUser.TopicManager == true;
                }

                if (NimbusUser.UserId == channel.OwnerId || cmt.UserId == NimbusUser.UserId || isAllow == true)
                {
                    using (var trans = db.OpenTransaction(System.Data.IsolationLevel.ReadCommitted))
                    {
                        try
                        {
                            //se ele é pai => apaga ele e os filhos
                            // se ele é um filho => deixa visible, coloca fo to do usuario como avatar padrao, tira o nome e coloca texto como: comentario removido
                            if (cmt.ParentId > 0) // é filho
                            {

                                cmt.Text = "Comentário removido";

                                db.Update<Comment>(cmt, c => c.Id == cmt.Id);
                                db.Save(cmt);

                                bag.Text = "Comentário removido";
                                bag.ParentId = cmt.ParentId;
                                bag.AvatarUrl = "/images/Utils/person_icon.png";
                                bag.UserName = "[removido]";
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
                            throw;
                        }
                    }
                }
            }
            return bag;
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

        public class CommentHtmlWrapper
        {
            public int Count { get; set; }
            public string Html { get; set; }
        }

        [HttpGet]
        public CommentHtmlWrapper CommentsHtml(int id = 0, int skip = 0, string type = null)
        {
            List<CommentBag> comments = new List<CommentBag>();
            string partial = "~/Website/Views/CommentPartials/PartialComment.cshtml";

            if (type == "channel")
                comments = ShowChannelComment(id, skip);
            else if (type == "topic")
                comments = ShowTopicComment(id, skip);
            else if (type == "child")
                comments = ShowMoreCommentChild(id, skip);
            else if (type == "oneparent")
            {
                comments = ShowParentComment(id, 0);
                //comments.FirstOrDefault().IsRazorEngine = true;
                partial = "~/Website/Views/CommentPartials/PartialTopicComment.cshtml";
            }
            else if (type == "onechild")
                comments = ShowChildComment(id);

            var rz = new RazorTemplate();
            string html = "";

            foreach (var cmt in comments)
            {
                html += rz.ParseRazorTemplate<CommentBag>(partial, cmt);
            }

            return new CommentHtmlWrapper { Html = html, Count = comments.Count };
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
                                CommentBag child = new CommentBag();
                                User userChild = db.SelectParam<User>(u => u.Id == itemChild.UserId).FirstOrDefault();
                                string name = "";
                                if (itemChild.Text == "Comentário removido")
                                {
                                    userChild.AvatarUrl = "/images/Utils/person_icon.png";
                                    name = "[removido]";
                                    child.IsDeletable = false;
                                    child.IsRepotable = false;

                                }
                                else
                                {
                                    name = userChild.FirstName + " " + userChild.LastName;
                                    child.IsDeletable = (userChild.Id == NimbusUser.UserId || isOwnerOrManager);
                                    child.IsRepotable = !isOwnerOrManager;
                                }

                                child.AvatarUrl = userChild.AvatarUrl;
                                child.UserName = name;
                                child.UserId = userChild.Id;
                                child.Id = itemChild.Id;
                                child.Text = itemChild.Text;
                                child.ParentId = itemChild.ParentId;
                                child.PostedOn = itemChild.PostedOn;
                                child.IsNew = itemChild.IsNew;
                                child.IsAnswer = itemChild.IsAnswer;
                                child.TopicId = itemChild.TopicId;
                                child.IsParent = false;
                                child.ChannelId = itemChild.ChannelId;
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
                                IsDeletable = (user.Id == NimbusUser.UserId || isOwnerOrManager),
                                IsRepotable = !isOwnerOrManager
                            };
                            listComments.Add(bag);
                        }
                    }
                }          
            
            return listComments;
        }

        /// <summary>
        /// método que retornar mais comentarios filhos
        /// </summary>
        /// <param name="id"></param>
        /// <param name="skip"></param>
        /// <returns></returns>
        [HttpGet]
        public List<CommentBag> ShowMoreCommentChild(int id, int skip)
        {
            List<CommentBag> listChild = new List<CommentBag>();
            using (var db = DatabaseFactory.OpenDbConnection())
            {
                //pega o comentário 'pai'
                Comment comments = db.Where<Comment>(cmt =>cmt.Visible == true && cmt.Id == id).FirstOrDefault();

                if (comments != null)
                {
                    Channel chn = db.Where<Channel>(c => c.Id == comments.ChannelId).FirstOrDefault();
                    ChannelController cc = ClonedContextInstance<ChannelController>();
                    var userRoles = cc.ReturnRolesUser(chn.Id);
                    bool isOwnerOrManager = (NimbusUser.UserId == chn.OwnerId ||
                        userRoles.Contains("channelmanager") ||
                        userRoles.Contains("topicmanager"));

                    User user = db.SelectParam<User>(u => u.Id == comments.UserId).FirstOrDefault();

                    //busco todos os filhos desse comentário
                    List<Comment> cmtChild = db.SelectParam<Comment>(c => c.ParentId == comments.Id && comments.Visible == true).Skip(3 * skip).Take(3).ToList();

                    foreach (var itemChild in cmtChild)
                    {
                        CommentBag child = new CommentBag();

                        User userChild = db.SelectParam<User>(u => u.Id == itemChild.UserId).FirstOrDefault();
                        string name = "";
                        if (itemChild.Text == "Comentário removido")
                        {
                            userChild.AvatarUrl = "/images/Utils/person_icon.png";
                            name = "[removido]";
                            child.IsDeletable = false;
                            child.IsRepotable = false;
                        }
                        else
                        {
                            name = userChild.FirstName + " " + userChild.LastName;
                            child.IsDeletable = (userChild.Id == NimbusUser.UserId || isOwnerOrManager);
                            child.IsRepotable = !isOwnerOrManager;
                        }

                        child.AvatarUrl = userChild.AvatarUrl;
                        child.UserName = name;
                        child.UserId = userChild.Id;
                        child.Id = itemChild.Id;
                        child.Text = itemChild.Text;
                        child.ParentId = itemChild.ParentId;
                        child.PostedOn = itemChild.PostedOn;
                        child.IsNew = itemChild.IsNew;
                        child.IsAnswer = itemChild.IsAnswer;
                        child.TopicId = itemChild.TopicId;
                        child.IsParent = false;
                        child.ChannelId = itemChild.ChannelId;

                        listChild.Add(child);
                    }
                }
            }

            return listChild;
        }

        /// <summary>
        /// Método que mostra para o usuário dono/moderador do canal os comentarios novos que surgiram
        /// o método ignora as respostas realizada pelo dono/moderator
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public List<CommentBag> ShowChannelComment(int id, int skip)
        {
            List<CommentBag> listComments = new List<CommentBag>();         
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    using (var trans = db.OpenTransaction(System.Data.IsolationLevel.ReadCommitted))
                    {
                        try
                        {
                            if (db.SelectParam<Channel>(c => c.Visible == true).Exists(c => c.Id == id))
                            {
                                List<Comment> comment = db.SelectParam<Comment>(cmt => cmt.Visible == true && cmt.ChannelId == id
                                                                                    && cmt.IsNew == true && cmt.ParentId == null).Skip(5 * skip).Take(5).ToList();

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
            return listComments;
        }

        /// <summary>
        /// Visualizar o comentario pai
        /// </summary>
        /// <param name="id">id coment</param>
        /// <returns></returns>
        [HttpGet]
        public List<CommentBag> ShowParentComment(int id, int skip = 0)
        {
            List<CommentBag> listComments = new List<CommentBag>();
            using (var db = DatabaseFactory.OpenDbConnection())
            {
                //pega todos comentários 'pai'
                List<Comment> comments = db.SelectParam<Comment>(cmt => cmt.Id == id && cmt.Visible == true && cmt.ParentId == null).ToList();

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
                            CommentBag child = new CommentBag();
                            User userChild = db.SelectParam<User>(u => u.Id == itemChild.UserId).FirstOrDefault();
                            string name = "";
                            if (itemChild.Text == "Comentário removido")
                            {
                                userChild.AvatarUrl = "/images/Utils/person_icon.png";
                                name = "[removido]";
                                child.IsDeletable = false;
                                child.IsRepotable = false;

                            }
                            else
                            {
                                name = userChild.FirstName + " " + userChild.LastName;
                                child.IsDeletable = (userChild.Id == NimbusUser.UserId || isOwnerOrManager);
                                child.IsRepotable = !isOwnerOrManager;
                            }

                            child.AvatarUrl = userChild.AvatarUrl;
                            child.UserName = name;
                            child.UserId = userChild.Id;
                            child.Id = itemChild.Id;
                            child.Text = itemChild.Text;
                            child.ParentId = itemChild.ParentId;
                            child.PostedOn = itemChild.PostedOn;
                            child.IsNew = itemChild.IsNew;
                            child.IsAnswer = itemChild.IsAnswer;
                            child.TopicId = itemChild.TopicId;
                            child.IsParent = false;
                            child.ChannelId = itemChild.ChannelId;
                            listChild.Add(child);
                        }

                        //crio o objeto para o comentario 
                        CommentBag bag = new CommentBag()
                        {
                            AvatarUrl = user.AvatarUrl,
                            UserName = user.FirstName + " " + user.LastName,
                            UserId = user.Id,
                            Id = item.Id,
                            Text = item.Text,
                            ParentId = item.ParentId,
                            PostedOn = item.PostedOn,
                            IsNew = item.IsNew,
                            IsAnswer = item.IsAnswer,
                            TopicId = item.TopicId,
                            IsParent = item.ParentId > 0 ? false : true,
                            ChannelId = item.ChannelId,
                            CommentChild = listChild,
                            IsDeletable = (user.Id == NimbusUser.UserId || isOwnerOrManager),
                            IsRepotable = !isOwnerOrManager
                        };
                        listComments.Add(bag);
                    }
                }
            }

            return listComments;
        }


        /// <summary>
        /// método que retornar mais comentarios filhos
        /// </summary>
        /// <param name="id"></param>
        /// <param name="skip"></param>
        /// <returns></returns>
        [HttpGet]
        public List<CommentBag> ShowChildComment(int id)
        {
            List<CommentBag> listChild = new List<CommentBag>();
            using (var db = DatabaseFactory.OpenDbConnection())
            {
                //pega o comentário 
                Comment comment = db.Where<Comment>(cmt => cmt.Visible == true && cmt.Id == id).FirstOrDefault();

                if (comment != null)
                {
                    Channel chn = db.Where<Channel>(c => c.Id == comment.ChannelId).FirstOrDefault();
                    ChannelController cc = ClonedContextInstance<ChannelController>();
                    var userRoles = cc.ReturnRolesUser(chn.Id);
                    bool isOwnerOrManager = (NimbusUser.UserId == chn.OwnerId ||
                        userRoles.Contains("channelmanager") ||
                        userRoles.Contains("topicmanager"));

                    User user = db.SelectParam<User>(u => u.Id == comment.UserId).FirstOrDefault();


                    CommentBag child = new CommentBag();

                    string name = "";
                    if (comment.Text == "Comentário removido")
                    {
                        user.AvatarUrl = "/images/Utils/person_icon.png";
                        name = "[removido]";
                        child.IsDeletable = false;
                        child.IsRepotable = false;
                    }
                    else
                    {
                        name = user.FirstName + " " + user.LastName;
                        child.IsDeletable = (user.Id == NimbusUser.UserId || isOwnerOrManager);
                        child.IsRepotable = !isOwnerOrManager;
                    }

                    child.AvatarUrl = user.AvatarUrl;
                    child.UserName = name;
                    child.UserId = user.Id;
                    child.Id = comment.Id;
                    child.Text = comment.Text;
                    child.ParentId = comment.ParentId;
                    child.PostedOn = comment.PostedOn;
                    child.IsNew = comment.IsNew;
                    child.IsAnswer = comment.IsAnswer;
                    child.TopicId = comment.TopicId;
                    child.IsParent = false;
                    child.ChannelId = comment.ChannelId;

                    listChild.Add(child);
                    
                }
            }

            return listChild;
        }

        public class CommentUser
        {
            public Comment Comment { get; set; }
            public User User { get; set; }
        }
    }
}
