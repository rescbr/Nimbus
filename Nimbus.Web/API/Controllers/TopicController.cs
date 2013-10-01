using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ServiceStack.OrmLite;
using Nimbus.DB.ORM;
using Nimbus.Web.API.Models;
using Nimbus.DB.Bags;

namespace Nimbus.Web.API.Controllers
{
    /// <summary>
    /// Controle sobre todas as funções realizadas para os Tópicos
    /// </summary>
    public class TopicController : NimbusApiController
    {
        /// <summary>
        /// Criar um novo tópico
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public Topic NewTopic(Topic topic)
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
        public List<TopicBag> AbstTopic(string viewBy = null, int channelID = 0, int categoryID = 0)
        {
            List<TopicBag> tpcList = new List<TopicBag>();
            try
            {
                List<int> idChannel = new List<int>();
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                   int idOrg = NimbusOrganization.Id ;

                   //busca todos topicos do canal da organizacao
                   if (channelID > 0)
                   {
                       bool isValid = db.SelectParam<Channel>(ch => ch.Id == channelID && ch.Visible == true && ch.OrganizationId == idOrg)
                                                                   .Exists(ch => ch.Id == channelID);
                       if (isValid)
                       {
                           idChannel.Add(channelID);
                       }
                       else
                       {
                           idChannel = null;
                       }
                   }
                   //busca todos os topicos da categoria da organizacao
                   else if (categoryID > 0)
                   {

                       idChannel = db.SelectParam<Channel>(ch => ch.OrganizationId == idOrg && ch.Visible == true && ch.CategoryId == categoryID)
                                                                           .Select(ch => ch.Id).ToList();
                   }
                   //busca todos os topicos em geral da organizacao                      
                   else
                   {
                       idChannel = db.SelectParam<Channel>(ch => ch.OrganizationId == idOrg && ch.Visible == true)
                                                                             .Select(ch => ch.Id).ToList();
                   }
                                                    

                   tpcList = (from tpc in db.SelectParam<Topic>(tp => tp.Visibility == true && idChannel.Contains(tp.ChannelId))
                              from count in db.SelectParam<ViewByTopic>(vt => vt.TopicId == tpc.Id).Select(vt => vt.CountView)
                              select new TopicBag()
                              {
                                  Id = tpc.Id,
                                  Description = tpc.Description,
                                  Title = tpc.Title,
                                  TopicType = tpc.TopicType,
                                  ImgUrl = tpc.ImgUrl,
                                  LastModified = tpc.LastModified,
                                  Count = count
                              }).ToList();               
                }
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex));
            }

            if (viewBy.ToLower() == "bymodified")
                return tpcList.OrderBy(tp => tp.LastModified).ToList();
            else if (viewBy.ToLower() == "bypopularity")
               return tpcList.OrderBy(tp => tp.Count).ToList();
            else
               return tpcList;
        }
             
        /// <summary>
        /// método de favoritar/desfavoritar o tópico
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPut]
        public bool TopicFavorite(int id)
        {
            bool flag = false;
            try
            {
                using(var db = DatabaseFactory.OpenDbConnection())
                {
                    UserTopicFavorite user = new UserTopicFavorite();
                    user = db.SelectParam<UserTopicFavorite>(us => us.UserId == NimbusUser.UserId && us.TopicId == id).FirstOrDefault();
                    UserTopicFavorite usrFavorite = new UserTopicFavorite();
                    if (user == null) //nunca favoritou
                    {
                            usrFavorite.UserId = NimbusUser.UserId;
                            usrFavorite.TopicId = id;
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
        public bool ReadChannelLater(int id, DateTime? readOn = null)
        {
            bool operation = false;
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    //se ja existir = retirar//se não existir = criar
                    UserTopicReadLater user = db.SelectParam<UserTopicReadLater>(rl => rl.UserId == NimbusUser.UserId && rl.TopicId == id).FirstOrDefault();
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
                        user.TopicId = id;
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
        public bool ValidateShowTopic(int id)
        {
            bool allow = false; 
            try
            {
                //ver permissao p vizualizar => se é pago = ter pagado, se é privado = ser aceito
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    Topic tpc = db.SelectParam<Topic>(tp => tp.Id == id).FirstOrDefault();
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
                        bool paid = db.SelectParam<RoleTopic>(tp => tp.ChannelId == tpc.ChannelId && tp.TopicId == id)
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
        /// Método de retornar o numero de favoritos de um tópico
        /// </summary>
        /// <param name="topicID"></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public int CountFavorite(int id)
        {
            int count = 0;
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    count = db.SelectParam<UserTopicFavorite>(fv => fv.TopicId == id && fv.Visible == true).Count();
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
        public Category CategoryTopic(int id)
        {
            Category category = new Category();
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    int catID = db.SelectParam<Channel>(ch => ch.Id == id && ch.Visible == true).Select(ch => ch.CategoryId).FirstOrDefault();
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
        public Topic ShowTopic(int id)
        {
            Topic topic = new Topic();
            try
            {
                using(var db = DatabaseFactory.OpenDbConnection())
                {
                    bool allow = ValidateShowTopic(id);

                    if (allow == true)
                    {
                        topic = db.SelectParam<Topic>(tp => tp.Id == id).FirstOrDefault();

                       if (topic.TopicType == Nimbus.DB.Enums.TopicType.Exam)
                        {
                            #region exam
                            //verificar se o usuario já fez o exame
                            int ChannelID = db.SelectParam<Channel>(ch => ch.Id == topic.ChannelId).Select(ch => ch.OrganizationId).FirstOrDefault();
                            UserExam userExam = ValidateExam(id);

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
        public List<Ad> ShowAds(int id = -1)
        {
            List<Ad> listAds = new List<Ad>();
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    if (id == -1) //não tem categoria, anuncio generico
                    {
                        listAds = db.SelectParam<Ad>(ad => ad.Visible == true);                                               
                    }
                    else
                    {
                        listAds = db.SelectParam<Ad>(ad => ad.Visible == true && ad.CategoryId == id);                               
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
        /// <returns></returns>
        [NonAction]
        [HttpGet]
        public UserExam ValidateExam(int id)
        {
            UserExam userExam = new UserExam();
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    int idOrg = NimbusOrganization.Id;
                    userExam = db.SelectParam<UserExam>(ex => ex.ExamId == id && ex.UserId == NimbusUser.UserId
                                                                        && ex.OrganizationId == idOrg).FirstOrDefault();
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
