using Nimbus.Web.API.Models.Topic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ServiceStack.OrmLite;
using Nimbus.DB.ORM;
using Nimbus.Web.API.Models.Comment;
using Nimbus.Web.API.Models;

namespace Nimbus.Web.API.Controllers
{
    /// <summary>
    /// Controle sobre todas as funções realizadas para os Tópicos
    /// </summary>
    public class TopicAPIController : NimbusApiController
    {
        /// <summary>
        /// Criar um novo tópico
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPut]
        public Topic newTopic(Topic topic)
        {
            try
            {
                using(var db = DatabaseFactory.OpenDbConnection())
                {
                    db.Insert(topic);
                    db.Save(topic);
                }
                return topic;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
        }

        /// <summary>
        /// método de exibir tópicos em resumo, filtra por categoriam modificação e popularidade
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public List<AbstractTopicAPI>AbstTopic(int channelID, TopicList viewBy, int categoryID)
        {
            List<AbstractTopicAPI> tpcList = new List<AbstractTopicAPI>();
            try
            {
                List<int> idChannel = new List<int>();
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                   int idOrg = NimbusOrganization.Id ;
                   if (categoryID > 0)
                   {
                       idChannel = db.SelectParam<Channel>(ch => ch.OrganizationId == idOrg && ch.Visible == true && ch.CategoryId == categoryID)
                                                                           .Select(ch => ch.Id).ToList();

                   }
                   else
                   {
                       idChannel = db.SelectParam<Channel>(ch => ch.OrganizationId == idOrg && ch.Visible == true)
                                                                             .Select(ch => ch.Id).ToList();
                   }


                   tpcList = (from tpc in db.SelectParam<Topic>(tp => tp.Visibility == true && idChannel.Contains(tp.ChannelId))
                              from count in db.SelectParam<ViewByTopic>(vt => vt.TopicId == tpc.Id).Select(vt => vt.CountView)
                              select new AbstractTopicAPI()
                              {
                                  topicId = tpc.Id,
                                  shortTextTopic = tpc.Description,
                                  Title = tpc.Title,
                                  Type = tpc.TopicType.ToString(),
                                  UrlImgTopic = tpc.ImgUrl,
                                  ModifiedOn = tpc.LastModified,
                                  Count = count
                              }).ToList();               
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }

            if (viewBy == TopicList.byModified)
                return tpcList.OrderBy(tp => tp.ModifiedOn).ToList();
            else if (viewBy == TopicList.byPopularity)
               return tpcList.OrderBy(tp => tp.Count).ToList();
            else
               return tpcList;
        }
             
        /// <summary>
        /// método de favoritar/desfavoritar o tópico
        /// </summary>
        /// <param name="idTopic"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public bool TopicFavorite(int idTopic)
        {
            bool flag = false;
            try
            {
                using(var db = DatabaseFactory.OpenDbConnection())
                {
                    UserTopicFavorite user = new UserTopicFavorite();
                    user = db.SelectParam<UserTopicFavorite>(us => us.UserId == NimbusUser.UserId && us.TopicId == idTopic).FirstOrDefault();
                    UserTopicFavorite usrFavorite = new UserTopicFavorite();
                    if (user == null) //nunca favoritou
                    {
                            usrFavorite.UserId = NimbusUser.UserId;
                            usrFavorite.TopicId = idTopic;
                            usrFavorite.FavoritedOn = DateTime.Now;
                            usrFavorite.Visible = true;
                        db.Insert(usrFavorite);
                        flag = true;
                    }
                    else
                    {
                        user.FavoritedOn = DateTime.Now;
                        user.Visible = (user.Visible == true) ? false : true;
                        db.Update(user);
                        flag = true;
                    }
                }
            }
            catch (Exception ex)
            {
                flag = false;
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }

            return flag;
        }

        /// <summary>
        /// Add/retirar tópico da lista de ler mais tarde 
        /// </summary>
        /// <param name="topicID"></param>
        /// <param name="readOn"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public bool ReadChannelLater(int topicID, DateTime readOn)
        {
            bool operation = false;
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    //se ja existir = retirar//se não existir = criar
                    UserTopicReadLater user = db.SelectParam<UserTopicReadLater>(rl => rl.UserId == NimbusUser.UserId && rl.TopicId == topicID).FirstOrDefault();
                    if (user != null)
                    {
                        user.Visible = false;
                        user.ReadOn = null;
                    }
                    else
                    {
                        user.Visible = true;
                        user.UserId = NimbusUser.UserId;
                        user.ReadOn = readOn;
                        user.TopicId = topicID;
                    }
                    db.Save(user);
                }
                operation = true;
            }
            catch (Exception ex)
            {
                operation = false;
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }

            return operation;
        }

        /// <summary>
        /// Verifica se o tópico é privado ou pago e se o usuário possui permissão para visualizar o conteúdo
        /// </summary>
        /// <param name="topicID"></param>
        /// <returns></returns>
        [NonAction]
        public bool validateShowTopic(int topicID)
        {
            bool allow = false; 
            try
            {
                //ver permissao p vizualizar => se é pago = ter pagado, se é privado = ser aceito
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    Topic tpc = db.SelectParam<Topic>(tp => tp.Id == topicID).FirstOrDefault();
                    bool chnPrivate = db.SelectParam<Channel>(ch => ch.Id == tpc.ChannelId).Select(ch => ch.IsPrivate).FirstOrDefault();
                    if (chnPrivate == false)
                    {
                        allow = true;
                    }
                    else
                    {
                        bool? pending = db.SelectParam<ChannelUser>(ch => ch.ChannelId == tpc.ChannelId && ch.UserId == NimbusUser.UserId)
                                                                                                        .Select(ch => ch.Accepted).FirstOrDefault();
                        if (pending == false && pending != null) //não esta pendente = já foi aceito
                        {
                            allow = true;
                        }
                        else
                        {
                            allow = false;
                        }
                    }
                    if (tpc.Price > 0)
                    {
                        bool paid = db.SelectParam<RoleTopic>(tp => tp.ChannelId == tpc.ChannelId && tp.TopicId == topicID)
                                                                             .Select(tp => tp.Paid).FirstOrDefault();
                        if (paid == true)
                        {
                            allow = true;
                        }
                        else
                        {
                            allow = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
            return allow;
        }

        /// <summary>
        /// Busca todos os comentarios de um tópico
        /// </summary>
        /// <param name="topicID"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public List<CommentAPIModel> showComments(int topicID)
        {
            List<CommentAPIModel> listComments = new List<CommentAPIModel>();
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    List<Comment> comments = db.SelectParam<Comment>(cm => cm.TopicId == topicID && cm.Visible == true);
                    listComments = new List<CommentAPIModel>();
                    foreach (Comment item in comments)
                    {
                        CommentAPIModel dados = new CommentAPIModel();
                        dados.AvatarUrl = db.SelectParam<User>(us => us.Id == item.UserId).Select(us => us.AvatarUrl).FirstOrDefault();
                        dados.comment_ID = item.Id;
                        dados.Name = db.SelectParam<User>(us => us.Id == item.UserId).Select(us => us.FirstName + " " + us.LastName).FirstOrDefault();
                        dados.ParentID = item.ParentId;
                        dados.PostedOn = item.PostedOn;
                        dados.Text = item.Text;
                        dados.TopicId = item.TopicId;

                        listComments.Add(dados);
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
        /// Método de retornar o numero de favoritos de um tópico
        /// </summary>
        /// <param name="topicID"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public int CountFavorite(int topicID)
        {
            int count = 0;
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    count = db.SelectParam<UserTopicFavorite>(fv => fv.TopicId == topicID && fv.Visible == true).Count();
                }
            }
            catch (Exception ex)
            {                
                 throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
            return count;
        }

        /// <summary>
        /// Retorna as informações da categoria que o tópico pertence
        /// </summary>
        /// <param name="channelID"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public Category CategoryTopic(int channelID)
        {
            Category category = new Category();
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    int catID = db.SelectParam<Channel>(ch => ch.Id == channelID && ch.Visible == true).Select(ch => ch.CategoryId).FirstOrDefault();
                    category = db.SelectParam<Category>(ct => ct.Id == catID).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
            return category;
        }

        //TODO terminar essa funçao: parte de mostras os topicos relacionados
        /// <summary>
        /// carregar informações gerais de um tópico, chammar a função de comentarios e count favoritos
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public Topic showTopic(int topicID)
        {
            Topic topic = new Topic();
            try
            {
                using(var db = DatabaseFactory.OpenDbConnection())
                {
                    bool allow = validateShowTopic(topicID);

                    if (allow == true)
                    {
                        topic = db.SelectParam<Topic>(tp => tp.Id == topicID).FirstOrDefault();

                       if (topic.TopicType == Nimbus.DB.Enums.TopicType.Exam)
                        {
                            #region exam
                            //verificar se o usuario já fez o exame
                            int ChannelID = db.SelectParam<Channel>(ch => ch.Id == topic.ChannelId).Select(ch => ch.OrganizationId).FirstOrDefault();                         
                            UserExam userExam = validateExam(topicID, ChannelID);

                            bool isPrivate = db.SelectParam<Channel>(ch => ch.Id == topic.ChannelId).Select(ch => ch.IsPrivate).FirstOrDefault();
                            
                            if (userExam == null || isPrivate == false)
                            {
                                //se nunca tiver feito o exame, pode fazer. Canal privado = pode limitar. Canal free = sempre aberto 
                                //caso seja um teste free, o 'bool' já permite refazer - apagar as respostas
                                foreach (Nimbus.DB.Question item in topic.Question)
                                {
                                    item.CorrectAnswer = 0;
                                }
                            }
                            #endregion
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
            return topic;     
        }

        /// <summary>
        /// Função para pegar os ads por categoria específica ou genérica.
        /// </summary>
        /// <param name="idCategory"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public List<Ad> showAds(int idCategory)
        {
            List<Ad> listAds = new List<Ad>();
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    if (idCategory == -1) //não tem categoria, anuncio generico
                    {
                        listAds = db.SelectParam<Ad>(ad => ad.Visible == true);                                               
                    }
                    else
                    {
                        listAds = db.SelectParam<Ad>(ad => ad.Visible == true && ad.CategoryId == idCategory);                               
                    }               
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }

            return listAds;
        }

        //TODO: listar tópicos relacionados 
        /// <summary>
        /// Verifica se o usuario já fez o exame. Se já fez, retornar o objeto com data e nota
        /// </summary>
        /// <param name="topicID"></param>
        /// <param name="organizationID"></param>
        /// <returns></returns>
        [NonAction]
        [HttpGet]
        public UserExam validateExam(int topicID, int organizationID)
        {
            UserExam userExam = new UserExam();
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    userExam = db.SelectParam<UserExam>(ex => ex.ExamId == topicID && ex.UserId == NimbusUser.UserId
                                                                        && ex.OrganizationId == organizationID).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                userExam = null;
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }
            return userExam;
        }



    }
}
