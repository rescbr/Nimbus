using Nimbus.Web.Website.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Nimbus.Web.Website.Controllers
{
    public class TopicDemoController : NimbusWebController
    {
        public ActionResult Index(int id)
        {
            var channelApi = ClonedContextInstance<API.Controllers.ChannelController>();
            var userApi = ClonedContextInstance<API.Controllers.UserController>();
            var msgApi = ClonedContextInstance<API.Controllers.MessageController>();
            var topicApi = ClonedContextInstance<API.Controllers.TopicController>();

            var aux = topicApi.ShowTopicToLoginPage(id);

            if (aux == null) throw new HttpException(404, "Topic not found");

            var topic = new TopicModel()
            {
                CurrentTopic = aux,
                CurrentChannel = channelApi.InfoChannelToLoginPage(aux.ChannelId), 
                Category = topicApi.CategoryTopic(aux.Id),
                NumFavorites = topicApi.CountFavorite(id),
                NumLikes = topicApi.CountLikes(id),
                NumUnLikes = topicApi.CountUnLikes(id)
            };
            return View("TopicDemo", topic);
        }
    }
}