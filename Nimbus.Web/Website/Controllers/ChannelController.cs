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
        public ActionResult Index(int channelId)
        {
            var channelApi = ClonedContextInstance<API.Controllers.ChannelController>();
            var userApi = ClonedContextInstance<API.Controllers.UserController>();
            var msgApi = ClonedContextInstance<API.Controllers.MessageController>();
            var topicApi = ClonedContextInstance<API.Controllers.TopicController>();
            var commentApi = ClonedContextInstance<API.Controllers.CommentController>();
                                    
            var channel = new ChannelModel()
            {
                Tags = channelApi.ShowTagChannel(channelId),
                Moderators = channelApi.ShowModerators(channelId),
                CurrentChannel = channelApi.ShowChannel(channelId),
                AllTopics = topicApi.AbstTopic(string.Empty,channelId, 0),            
                Messages = msgApi.ChannelReceivedMessages(channelId),
                Comments = commentApi.ShowChannelComment(channelId),
                CurrentUser = NimbusUser,
                RolesCurrentUser = channelApi.ReturnRolesUser(channelId),
                CcMessageReceiver = channelApi.GetMessageModerators(channelId)
            };
            return View("Channels", channel);
        }
    }
}