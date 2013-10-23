using Nimbus.Web.Website.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Nimbus.Web.Website.Controllers
{
    public class TopicController : NimbusWebController
    {
        [Authorize]
        public ActionResult Index(int id)
        {
            var channelApi = ClonedContextInstance<API.Controllers.ChannelController>();
            var userApi = ClonedContextInstance<API.Controllers.UserController>();
            var msgApi = ClonedContextInstance<API.Controllers.MessageController>();
            var topicApi = ClonedContextInstance<API.Controllers.TopicController>();
            var commentApi = ClonedContextInstance<API.Controllers.CommentController>();

            var aux = topicApi.ShowTopic(id);

            if (aux == null) throw new HttpException(404, "Topic not found");

            var topic = new TopicModel()
            {
                CurrentTopic = aux,
                CurrentChannel = channelApi.ShowChannel(aux.ChannelId),
                CurrentUser = NimbusUser,
                Comments = commentApi.ShowTopicComment(id),
                RolesCurrentUser = channelApi.ReturnRolesUser(aux.ChannelId),
                Category = topicApi.CategoryTopic(aux.ChannelId),
                NumFavorites = topicApi.CountFavorite(id)
            };
            return View("Topic", topic);
        }
    }
}