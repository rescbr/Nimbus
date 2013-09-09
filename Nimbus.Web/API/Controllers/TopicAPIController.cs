using Nimbus.Web.API.Models.Topic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ServiceStack.OrmLite;
using Nimbus.DB;

namespace Nimbus.Web.API.Controllers
{
    /// <summary>
    /// Controle sobre todas as funções realizadas para os Tópicos
    /// </summary>
    public class TopicAPIController : NimbusApiController
    {                
        /// <summary>
        /// método de exibir tópicos em resumo
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public List<AbstractTopicAPI> abstTopic(int channelID, TopicList viewBy)
        {
            List<AbstractTopicAPI> list = new List<AbstractTopicAPI>();
            try
            {
                using (var db = DatabaseFactory.OpenDbConnection())
                {
                    List<AbstractTopicAPI> tpcList = db.Query<AbstractTopicAPI>("SELECT Topic.Id, Topic.Description, Topic.Title, Topic.TopicType, Topic.ImgUrl, " +
                                                                                "Topic.LastModified, ViewByTopic.CountView " +
                                                                                "INNER JOIN ViewByTopic ON ViewByTopic.TopicID = Topic.Id"+
                                                                                "FROM Topic WHERE Topic.Visible = true && Topic.ChannelId = {0}", channelID);
                   
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
                    Nimbus.DB.UserTopicFavorite user = new DB.UserTopicFavorite();
                    user = db.SelectParam<Nimbus.DB.UserTopicFavorite>(us => us.UserId == NimbusUser.UserId && us.TopicId == idTopic).FirstOrDefault();
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
                    UserTopicReadLater user = db.SelectParam<Nimbus.DB.UserTopicReadLater>(rl => rl.UserId == NimbusUser.UserId && rl.TopicId == topicID).FirstOrDefault();
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
        
        //abrir o tópico
        /// <summary>
        /// carregar informações gerais de um tópico
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public ShowTopicAPI showTopic(int topicID, TopicType type)
        {
            ShowTopicAPI topic = new ShowTopicAPI();
            try
            {
                //ver permissao p vizualizar => se é pago = ter pagado, se é privado = ser aceito

                using(var db = DatabaseFactory.OpenDbConnection())
                {

                }

            }
            catch (Exception ex)
            {                
                throw;
            }
            return topic;         
            
        }
               
        
        //TODO: listar tópicos relacionados 



    }
}
