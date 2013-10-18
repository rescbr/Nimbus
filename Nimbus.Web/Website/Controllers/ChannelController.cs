using Nimbus.Web.Website.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ServiceStack.OrmLite;

namespace Nimbus.Web.Website.Controllers
{
    public class ChannelController: NimbusWebController
    {
        [Authorize]
        public ActionResult Index(int id)
        {
            var channelApi = ClonedContextInstance<API.Controllers.ChannelController>();
            var userApi = ClonedContextInstance<API.Controllers.UserController>();
            var msgApi = ClonedContextInstance<API.Controllers.MessageController>();
            var topicApi = ClonedContextInstance<API.Controllers.TopicController>();
            var commentApi = ClonedContextInstance<API.Controllers.CommentController>();

            var channel = new ChannelModel()
            {
                Tags = channelApi.ShowTagChannel(id),
                Moderators = channelApi.ShowModerators(id),
                CurrentChannel = channelApi.ShowChannel(id),
                AllTopics = topicApi.AbstTopic(string.Empty, id, 0),
                Messages = msgApi.ChannelReceivedMessages(id),
                Comments = commentApi.ShowChannelComment(id),
                CurrentUser = NimbusUser,
                RolesCurrentUser = channelApi.ReturnRolesUser(id),
                CcMessageReceiver = channelApi.GetMessageModerators(id),
                NewTopic = null
            };
            return View("Channels", channel);
        }
    }
}