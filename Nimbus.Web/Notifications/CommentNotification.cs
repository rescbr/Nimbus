using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nimbus.Web.Notifications
{
    public class CommentNotification : NimbusNotificationBase
    {
        public void NewComment(string msg, List<int> userIds)
        {
            foreach (var uid in userIds)
            {
                NimbusHubContext.Clients.Group(NimbusHub.GetGroupName(uid)).newMessageNotification(msg);
            }
        }
    }
}