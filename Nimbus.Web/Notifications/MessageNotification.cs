using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Nimbus.Web.Utils;

namespace Nimbus.Web.Notifications
{
    public class MessageNotification : NimbusNotificationBase
    {
        public void NewMessage(Model.ORM.Message msg)
        {
            var sender = msg.Receivers.Where(r => r.UserId == msg.SenderId).FirstOrDefault();
            var receivers = msg.Receivers.Where(r => r.UserId != msg.SenderId);

            var messageNotification = new MessageNotificationModel
            {
                SenderName = sender.Name,
                SenderAvatarUrl = sender.AvatarUrl,
                Subject = msg.Title,
                MessageId = msg.Id,
                Date = msg.Date.ToShortDateString(),
                Time = msg.Date.ToShortTimeString(),
            };

            var rz = new RazorTemplate();
            string htmlNotif = rz.ParseRazorTemplate<MessageNotificationModel>
                ("~/Website/Views/NotificationPartials/Message.cshtml", messageNotification);

            foreach (var receiver in receivers)
            {
                NimbusHubContext.Clients.Group(NimbusHub.GetMessageGroupName(receiver.UserId)).newMessageNotification(htmlNotif);
            }
        }
    }

    public class MessageNotificationModel
    {
        public int MessageId { get; set; }
        public string SenderName { get; set; }
        public string SenderAvatarUrl { get; set; }
        public string Subject { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }

    }
}