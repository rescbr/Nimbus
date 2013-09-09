using Nimbus.Web.API.Models.Topic;
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

        //ler o tópico mais tarde ou retirar da lista de leitura 
        //favoritar/desfavoritar o tópico
        //abrir o tópico
        //listar tópicos relacionados 



    }
}
