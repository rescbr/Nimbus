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
    public class ModeratorNotification : NimbusNotificationBase
    {
        public void CreateModeratorInvite(Channel channel, User inviter, User invited)
        {
            var now = DateTime.UtcNow;
            var moderatorNotification = new ModeratorNotificationModel
            {
                SenderName = inviter.FirstName,
                SenderAvatarUrl = inviter.AvatarUrl,
                ChannelName = channel.Name,
                ChannelId = channel.Id,
                Date = now.ToShortDateString(),
                Time = now.ToShortTimeString(),
                Timestamp = now.ToFileTimeUtc(),
                Guid = Guid.NewGuid() //gera novo GUID
            };


            var rz = new RazorTemplate();
            string htmlNotif = rz.ParseRazorTemplate<ModeratorNotificationModel>
                ("~/Website/Views/NotificationPartials/Accept.cshtml", moderatorNotification);

            var wrapper = new ModeratorNotificationWrapper()
            {
                ChannelId = moderatorNotification.ChannelId,
                Html = htmlNotif
            };

            NimbusHubContext.Clients.Group(NimbusHub.GetMessageGroupName(invited.Id)).newModeratorNotification(wrapper);

            StoreNotification(moderatorNotification, invited.Id);

        }

        public void StoreNotification(ModeratorNotificationModel mod, int userid)
        {
            using (var db = DatabaseFactory.OpenDbConnection())
            {

                var dbNotif = new Model.ORM.Notification<ModeratorNotificationModel>()
                {
                    Id = mod.Guid,
                    UserId = userid,
                    IsRead = false,
                    NotificationObject = mod,
                    Timestamp = mod.Timestamp,
                    Type = Model.NotificationTypeEnum.moderatorinvite
                };
                db.Insert<Model.ORM.Notification<ModeratorNotificationModel>>(dbNotif);

            }
        }
    }

    public class ModeratorNotificationWrapper
    {
        public int ChannelId { get; set; }
        public string Html { get; set; }
    }


    public class ModeratorNotificationModel
    {
        public int ChannelId { get; set; }
        public string SenderName { get; set; }
        public string SenderAvatarUrl { get; set; }
        public string ChannelName { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        //Datetime.ToFileTimeUtc()
        public long Timestamp { get; set; }
        public Guid Guid { get; set; }

        /// <summary>
        /// Cria nova instância de MessageNotificationModel
        /// </summary>
        public ModeratorNotificationModel() { }

        /// <summary>
        /// Cria cópia de uma instância de MessageNotificationModel *com outro Guid*
        /// </summary>
        /// <param name="other">instância de MessageNotificationModel</param>
        public ModeratorNotificationModel(MessageNotificationModel other)
        {
            this.ChannelId = other.MessageId;
            this.SenderName = other.SenderName;
            this.SenderAvatarUrl = other.SenderAvatarUrl;
            this.ChannelName = other.Subject;
            this.Date = other.Date;
            this.Time = other.Time;
            this.Timestamp = other.Timestamp;
            this.Guid = Guid.NewGuid();
        }
    }
}