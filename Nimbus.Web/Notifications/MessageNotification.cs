using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace Nimbus.Web.Notifications
{
    public class MessageNotification : NimbusNotificationBase
    {
        public void NewMessage(Model.ORM.Message msg, int senderId)
        {
            //var rz = new RazorTemplate();
            //string s = rz.ParseRazorTemplate<HomeModel>("~/Website/Views/Home.cshtml", hm);

            var userIds = msg.Receivers.Where(l => l.UserId != senderId).Select(l => l.UserId);
            foreach (var uid in userIds)
            {
                NimbusHubContext.Clients.Group(NimbusHub.GetGroupName(uid)).newMessageNotification(msg);
            }
        }
    }
}