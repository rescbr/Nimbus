using Nimbus.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.OrmLite;
using Nimbus.Web.API.Models;
using Nimbus.Web.API.Models.Channel;
using Nimbus.Web.API.Models.Topic;

namespace Nimbus.Web.API.Controllers
{
    public class SearchAPIController:NimbusApiController
    {
        /// <summary>
        /// método para pegar o nome da organização pelo host e saber se o usuário esta no portal 'nimbus' ou da org
        /// </summary>
        /// <returns></returns>
        public int CurrentOrgID()
        {
            int idOrg = 0;
            using (var db = DatabaseFactory.OpenDbConnection())
            {
                try                    
                {
                    string name = Request.Headers.Host;
                    idOrg = db.SelectParam<Organization>(org => org.Cname == name).Select(o => o.Id).FirstOrDefault();
                }
                catch (Exception)
                {
                    idOrg = 1; //nimbus
                }
            }
            return idOrg;
        }

        public SearchAPIModel Search(SearchType typeSearch, string text)
        {
            SearchAPIModel search = new SearchAPIModel ();

            if (!string.IsNullOrEmpty(text))
            {
                int idOrg = CurrentOrgID();

                try
                {                    
                    using (var db = DatabaseFactory.OpenDbConnection())
                    {
                        if (typeSearch == SearchType.all)
                        {
                            search.abstractChannel = db.Query<AbstractChannelAPI>("SELECT Channel.OrganizationId as Organization_ID, Channel.Id as channel_ID, Channel.Name as ChannelName, Channel.ImgUrl as UrlImgChannel "+
                                                                                  "FROM Channel" +
                                                                                  "INNER JOIN Topic ON Topic.ChannelId = Channel.Id" +
                                                                                  "INNER JOIN Category ON Category.Id = Channel.CategoryId " +
                                                                                  "WHERE Channel.Organization = @orgID AND Channel.Visible = true AND Topic.Visible = true" +
                                                                                  "AND (Channel.Name LIKE @text OR Channel.Description LIKE @text OR " +
                                                                                        "Topic.Title LIKE @text OR Topic.Text LIKE @text OR Category.Name LIKE @text)",
                                                                                  new {text = text, orgID = idOrg });

                            search.abstractTopic = db.Query<AbstractTopicAPI>("SELECT Topic.Id as topic_ID, Topic.ImgUrl as UrlImgTopic, Topic.Description as shortTexTopic, Topic.Title, "+
                                                                                       "Topic.TopicType as Type, Topic.LastModified as ModifiedOn, COUNT(Favorite.UserId) as Count " +
                                                                                  "FROM Topic" +
                                                                                  "INNER JOIN Channel ON Channel.Id = Topic.ChannelId " +
                                                                                  "INNER JOIN Category ON Category.Id = Channel.CategoryId " +
                                                                                  "INNER JOIN UserTopicFavorite as Favorite ON Favorite.TopicId = Topic.Id " +
                                                                                  "WHERE Channel.Organization = @orgID AND Channel.Visible = true AND Topic.Visible = true" +
                                                                                  "AND (Channel.Name LIKE @text OR Channel.Description LIKE @text OR " +
                                                                                        "Topic.Title LIKE @text OR Topic.Text LIKE @text OR Category.Name LIKE @text)",
                                                                                  new { text = text, orgID = idOrg });
                        }
                        else if (typeSearch == SearchType.category)
                        {
                            search.abstractChannel = db.Query<AbstractChannelAPI>("SELECT Channel.OrganizationId as Organization_ID, Channel.Id as channel_ID, Channel.Name as ChannelName, Channel.ImgUrl as UrlImgChannel " +
                                                                                  "FROM Channel" +                                                                               
                                                                                  "INNER JOIN Category ON Category.Id = Channel.CategoryId " +
                                                                                  "WHERE Channel.Organization = @orgID AND Channel.Visible = true AND Category.Name LIKE @text",
                                                                                  new { text = text, orgID = idOrg });

                            search.abstractTopic = db.Query<AbstractTopicAPI>("SELECT Topic.Id as topic_ID, Topic.ImgUrl as UrlImgTopic, Topic.Description as shortTexTopic, Topic.Title, " +
                                                                                       "Topic.TopicType as Type, Topic.LastModified as ModifiedOn, COUNT(Favorite.UserId) as Count " +
                                                                                  "FROM Topic" +
                                                                                  "INNER JOIN Channel ON Channel.Id = Topic.ChannelId" +
                                                                                  "INNER JOIN Category ON Category.Id = Channel.CategoryId " +
                                                                                  "INNER JOIN UserTopicFavorite as Favorite ON Favorite.TopicId = Topic.Id " +
                                                                                  "WHERE Channel.Organization = @orgID AND Channel.Visible = true AND Topic.Visible = true" +
                                                                                  "AND  Category.Name LIKE @text",
                                                                                  new { text = text, orgID = idOrg });
                        }
                        else if (typeSearch == SearchType.channel)
                        {
                            search.abstractChannel = db.Query<AbstractChannelAPI>("SELECT Channel.OrganizationId as Organization_ID, Channel.Id as channel_ID, Channel.Name as ChannelName, Channel.ImgUrl as UrlImgChannel " +
                                                          "FROM Channel" +
                                                          "WHERE Channel.Organization = @orgID AND Channel.Visible = true AND (Channel.Name LIKE @text OR Channel.Description LIKE @text)",
                                                          new { text = text, orgID = idOrg });

                        }
                        else if (typeSearch == SearchType.tag)
                        {
                        }
                        else if (typeSearch == SearchType.topic)
                        {
                            search.abstractTopic = db.Query<AbstractTopicAPI>("SELECT Topic.Id as topic_ID, Topic.ImgUrl as UrlImgTopic, Topic.Description as shortTexTopic, Topic.Title, " +
                                                                                       "Topic.TopicType as Type, Topic.LastModified as ModifiedOn, COUNT(Favorite.UserId) as Count " +
                                                                                  "FROM Topic" +
                                                                                  "INNER JOIN Channel ON Channel.Id = Topic.ChannelId "+
                                                                                  "INNER JOIN UserTopicFavorite as Favorite ON Favorite.TopicId = Topic.Id " +
                                                                                  "WHERE Channel.Organization = @orgID AND Channel.Visible = true AND Topic.Visible = true" +
                                                                                  "AND (Topic.Title LIKE @text OR Topic.Text LIKE @text)",
                                                                                  new { text = text, orgID = idOrg });
                        }
                    }
                }
                catch (Exception ex)
                {
                    search = null;
                }
            }


            return search;
        }



    }
}