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

namespace Nimbus.Web.Notifications
{
    public class MessageNotification : NimbusNotificationBase
    {
        public void NewMessage(Model.ORM.Message msg)
        {
            Task.Run(() => NewMessageTask(msg));
        }
        public void NewMessageTask(Model.ORM.Message msg)
        {
            var sender = msg.Receivers.Where(r => r.UserId == msg.SenderId).FirstOrDefault();
            var receivers = msg.Receivers.Where(r => r.UserId != msg.SenderId).Select(s => s.UserId);

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

            Task.Run(() =>
            {
                StoreNotifications(messageNotification, receivers.ToArray());
            });

            Parallel.ForEach(receivers, (receiver) =>
            {
                NimbusHubContext.Clients.Group(NimbusHub.GetMessageGroupName(receiver)).newMessageNotification(htmlNotif);
            });
        }

        public void StoreNotifications(MessageNotificationModel msg, int[] userids)
        {
            CloudStorageAccount _storageAccount = CloudStorageAccount.Parse(NimbusConfig.StorageAccount);
            CloudTableClient tableClient = _storageAccount.CreateCloudTableClient();
            CloudTable table = tableClient.GetTableReference(Const.Azure.MessageNotificationsTable);

            TableBatchOperation tbo = new TableBatchOperation();
            foreach (var userid in userids)
            {
                var entity = new NotificationTableEntity<MessageNotificationModel>(msg, userid);
                tbo.Insert(entity);    
            }

            table.ExecuteBatch(tbo);
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