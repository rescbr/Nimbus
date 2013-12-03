using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Nimbus.Web.Utils;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using System.Threading.Tasks;
using ServiceStack.OrmLite;
using Nimbus.Model.Bags;
using Nimbus.Model.ORM;

namespace Nimbus.Web.Notifications
{
    public class MessageNotification : NimbusNotificationBase
    {
        public void NewMessage(Model.ORM.Message msg)
        {
            var sender = msg.Receivers.Where(r => r.UserId == msg.SenderId).FirstOrDefault();
            List<int> receivers = msg.Receivers.Where(r => r.UserId != msg.SenderId).Select(s => s.UserId).ToList();

            if (receivers.Count() == 0)
            {
                receivers = new List<int>();
                receivers.Add(sender.UserId);
            }


            var messageNotification = new MessageNotificationModel
            {
                SenderName = sender.Name,
                SenderAvatarUrl = sender.AvatarUrl,
                Subject = msg.Title,
                MessageId = msg.Id,
                Date = msg.Date.ToShortDateString(),
                Time = msg.Date.ToShortTimeString(),
                Timestamp = msg.Date.ToFileTimeUtc()
            };

            Parallel.ForEach(receivers, (receiver) =>
            {
                var msgCopy = new MessageNotificationModel(messageNotification);

                var rz = new RazorTemplate();
                string htmlNotif = rz.ParseRazorTemplate<MessageNotificationModel>
                    ("~/Website/Views/NotificationPartials/Message.cshtml", msgCopy);

                var wrapper = new MessageNotificationWrapper()
                {
                    MessageId = msgCopy.MessageId,
                    Html = htmlNotif
                };

                NimbusHubContext.Clients.Group(NimbusHub.GetMessageGroupName(receiver)).newMessageNotification(wrapper);

                StoreNotification(msgCopy, receiver);
            });
        }

        public void StoreNotification(MessageNotificationModel msg, int userid)
        {
            using (var db = DatabaseFactory.OpenDbConnection())
            {
   
                var dbNotif = new Model.ORM.Notification<MessageNotificationModel>()
                {
                    Id = msg.Guid,
                    UserId = userid,
                    IsRead = false,
                    NotificationObject = msg,
                    Timestamp = msg.Timestamp,
                    Type = Model.NotificationTypeEnum.message
                };
                db.Insert<Model.ORM.Notification<MessageNotificationModel>>(dbNotif);

            }
        }
    }

    public class MessageNotificationWrapper
    {
        public int MessageId { get; set; }
        public string Html { get; set; }
    }


    public class MessageNotificationModel
    {
        public int MessageId { get; set; }
        public string SenderName { get; set; }
        public string SenderAvatarUrl { get; set; }
        public string Subject { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        //Datetime.ToFileTimeUtc()
        public long Timestamp { get; set; }
        public Guid Guid { get; set; }

        /// <summary>
        /// Cria nova instância de MessageNotificationModel
        /// </summary>
        public MessageNotificationModel() { }

        /// <summary>
        /// Cria cópia de uma instância de MessageNotificationModel *com outro Guid*
        /// </summary>
        /// <param name="other">instância de MessageNotificationModel</param>
        public MessageNotificationModel(MessageNotificationModel other)
        {
            this.MessageId = other.MessageId;
            this.SenderName = other.SenderName;
            this.SenderAvatarUrl = other.SenderAvatarUrl;
            this.Subject = other.Subject;
            this.Date = other.Date;
            this.Time = other.Time;
            this.Timestamp = other.Timestamp;
            this.Guid = Guid.NewGuid();
        }
    }
}