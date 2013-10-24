using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace Nimbus.Web.Notifications
{
    public class MessageNotification : NimbusNotificationBase
    {
        public void NewMessage(string msg, List<int> userIds)
        {
            foreach (var uid in userIds)
            {
                NimbusHubContext.Clients.Group(NimbusHub.GetGroupName(uid)).newMessageNotification(msg);
            }
        }
    }
}