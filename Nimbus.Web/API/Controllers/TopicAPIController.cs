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
        /// método de exibir tópicos em resumo, filtra por categoriam modificação e popularidade
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public List<AbstractTopicAPI> abstTopic(int channelID, TopicList viewBy, int categoryID)
        {
            List<AbstractTopicAPI> list = new List<AbstractTopicAPI>();
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    List<AbstractTopicAPI> tpcList = new List<AbstractTopicAPI>();
                    if (categoryID > 0)
                    {
                        tpcList = db.Query<AbstractTopicAPI>("SELECT Topic.Id, Topic.Description, Topic.Title, Topic.TopicType, Topic.ImgUrl, " +
                                                                     "Topic.LastModified, ViewByTopic.CountView " +
                                                                     "INNER JOIN ViewByTopic ON ViewByTopic.TopicID = Topic.Id " +
                                                                     "INNER JOIN Channel ON Channel.Id = Topic.ChannelId " +
                                                                     "FROM Topic WHERE Topic.Visible = true AND Topic.ChannelId = @idChannel AND Channel.CategoryId = @idCategory",
                                                                     new { idChannel = channelID, idCategory = categoryID });
                    }
                    else 
                    {
                        tpcList = db.Query<AbstractTopicAPI>("SELECT Topic.Id, Topic.Description, Topic.Title, Topic.TopicType, Topic.ImgUrl, " +
                                                                                    "Topic.LastModified, ViewByTopic.CountView " +
                                                                                    "INNER JOIN ViewByTopic ON ViewByTopic.TopicID = Topic.Id" +
                                                                                    "FROM Topic WHERE Topic.Visible = true AND Topic.ChannelId = {0}", channelID);
                    }
                   
                    foreach (var item in tpcList)
                    {
                        AbstractTopicAPI absTopic = new AbstractTopicAPI
                        {
                            topic_ID = item.topic_ID,
                            Title = item.Title,
                            shortTextTopic = item.shortTextTopic,
                            UrlImgTopic = item.UrlImgTopic,
                            Type = item.Type.ToString(),
                            ModifiedOn = item.ModifiedOn,
                            Count = item.Count
                        };
                        list.Add(absTopic);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (viewBy == TopicList.byModified)
                return list.OrderBy(tp => tp.ModifiedOn).ToList();
            else if (viewBy == TopicList.byPopularity)
                return list.OrderBy(tp => tp.Count).ToList();
            else
               return list;
        }
             
        /// <summary>
        /// método de favoritar/desfavoritar o tópico
        /// </summary>
        /// <param name="idTopic"></param>
        /// <returns></returns>
        [Authorize]
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
                throw ex;
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
                throw;
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
                                                                                                        .Select(ch => ch.Pending).FirstOrDefault();
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
                throw ex;
            }
            return allow;
        }

        //TODO terminar essa funçao: parte de mostras os topicos relacionados
        /// <summary>
        /// carregar informações gerais de um tópico
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public ShowTopicAPI showTopic(int topicID)
        {
            ShowTopicAPI topic = new ShowTopicAPI();
            try
            {
                using(var db = DatabaseFactory.OpenDbConnection())
                {
                    bool allow = validateShowTopic(topicID);

                    if (allow == true)
                    {
                        Topic tpc = db.SelectParam<Topic>(tp => tp.Id == topicID).FirstOrDefault();

                        #region comments
                        List<Comment> comments = db.SelectParam<Comment>(cm => cm.TopicId == topicID && cm.Visible == true);
                        List<CommentAPIModel> listComments = new List<CommentAPIModel>();
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
                        #endregion

                        if (tpc.TopicType == Nimbus.DB.Enums.TopicType.Exam)
                        {
                            #region exam
                            //verificar se o usuario já fez o exame
                            int ChannelID = db.SelectParam<Channel>(ch => ch.Id == tpc.ChannelId).Select(ch => ch.OrganizationId).FirstOrDefault();
                         
                            UserExamAPI userExam = validateExam(tpc.Id, ChannelID);
                            bool isPrivate = db.SelectParam<Channel>(ch => ch.Id == tpc.ChannelId).Select(ch => ch.IsPrivate).FirstOrDefault();
                            
                            if (userExam == null || isPrivate == false)
                            {
                                //se nunca tiver feito o exame, pode fazer. Canal privado = pode limitar. Canal free = sempre aberto 
                                //caso seja um teste free, o 'bool' já permite refazer
                                List<QuestionTopicAPI> listQuestion = new List<QuestionTopicAPI>();

                                foreach (Nimbus.DB.Question item in tpc.Question)
                                {
                                    QuestionTopicAPI question = new QuestionTopicAPI();
                                    question.Question = item.TextQuestion;
                                    question.Options = item.ChoicesAnswer;
                                    listQuestion.Add(question);
                                }

                                //TODO topic.Exam.RelatedTopicList = ;
                                topic.Exam.ShortDescriptionTopic = tpc.Description;
                                topic.Exam.topic_ID = tpc.Id;
                                topic.Exam.Questions = listQuestion;
                                topic.Exam.TopicName = tpc.Title;
                                topic.Exam.TopicType = tpc.TopicType.ToString();
                                topic.Exam.UrlImgBanner = tpc.UrlCapa;
                                topic.Exam.UrlImgTopic = tpc.ImgUrl;
                                topic.Exam.Comments = listComments;
                            }
                            else
                            {
                                topic.Exam.examDone = userExam;
                            }
                            #endregion
                        }
                        else if ((tpc.TopicType != Nimbus.DB.Enums.TopicType.Exam) 
                                 && (tpc.TopicType != Nimbus.DB.Enums.TopicType.Add))
                        {
                            #region topic general
                            //TODO topic.generalTopic.RelatedTopicList = 
                            topic.generalTopic.ShortDescriptionTopic = tpc.Description;
                            topic.generalTopic.topic_ID = tpc.Id;
                            topic.generalTopic.TopicName = tpc.Title;
                            topic.generalTopic.TopicContent = tpc.Text;
                            topic.generalTopic.TopicType = tpc.TopicType.ToString();
                            topic.generalTopic.UrlImgTopic = tpc.ImgUrl;
                            topic.generalTopic.UrlImgBanner = tpc.UrlCapa;
                            topic.generalTopic.CountFavorites = db.SelectParam<UserTopicFavorite>(tp => tp.TopicId == topicID && tp.Visible == true).Count();
                            topic.Exam.Comments = listComments;
                            #endregion
                        }
                       
                    }
                }

            }
            catch (Exception ex)
            {                
                throw;
            }
            return topic;         
            
        }

        /// <summary>
        /// Função para pegar os ads por categoria específica ou genérica.
        /// </summary>
        /// <param name="idCategory"></param>
        /// <returns></returns>
        [Authorize]
        public List<showAdsAPIModel> showAds(int idCategory)
        {
            List<showAdsAPIModel> listAds = new List<showAdsAPIModel>();
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    List<Ad> ads = new List<Ad>();
                    if (idCategory == -1) //não tem categoria, anuncio generico
                    {
                        ads = db.SelectParam<Ad>(ad => ad.Visible == true);                       
                    }
                    else
                    {
                        ads = db.SelectParam<Ad>(ad => ad.Visible == true && ad.CategoryId == idCategory);
                    }
                    foreach (Ad item in ads)
                    {
                        showAdsAPIModel dado = new showAdsAPIModel
                        {
                            category_id = item.CategoryId,
                            idAds = item.Id,
                            ImgUrl = item.ImgUrl,
                            Url = item.Url

                        };
                        listAds.Add(dado);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
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
        public UserExamAPI validateExam(int topicID, int organizationID)
        {
            UserExamAPI userExam = new UserExamAPI();
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    UserExam exam = new UserExam();                    
                    exam = db.SelectParam<UserExam>(ex => ex.ExamId == topicID && ex.UserId == NimbusUser.UserId
                                                                        && ex.OrganizationId == organizationID).FirstOrDefault();
                    if (exam != null)
                    {
                        userExam.dateRealized = exam.RealizedOn;
                        userExam.ExamID = exam.ExamId;
                        userExam.Grade = exam.Grade;
                    }
                }
            }
            catch (Exception ex)
            {
                userExam = null;
                throw ex;
            }
            return userExam;
        }



    }
}
