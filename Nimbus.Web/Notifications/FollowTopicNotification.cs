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
using Nimbus.Model.ORM;

namespace Nimbus.Web.Notifications
{
    public class FollowTopicNotification : NimbusNotificationBase
    {
        /// <summary>
        /// Envia notificações de tópico novo para os usuários seguidores do canal.
        /// Utiliza mesmo canal de notificações de mensagem.
        /// </summary>
        /// <param name="topic"></param>
        public void NewTopic(Topic topic)
        {
            using (var db = DatabaseFactory.OpenDbConnection())
            {
                var channelFollowers = db.Where<ChannelUser>(chu => chu.ChannelId == topic.ChannelId
                                                                 && chu.Follow == true
                                                                 && chu.Accepted == true
                                                                 && chu.Visible == true);

                var channel = db.Where<Channel>(ch => ch.Id == topic.ChannelId).FirstOrDefault();

                var now = DateTime.UtcNow;

                var nt = new NewTopicNotificationModel()
                {
                    TopicId = topic.Id,
                    TopicName = topic.Title,
                    ChannelId = topic.ChannelId,
                    ChannelName = channel.Name,
                    TopicImage = topic.ImgUrl,

                    Date = now.ToShortDateString(),
                    Time = now.ToShortTimeString(),
                    Timestamp = now.ToFileTimeUtc(),
                };

                var rz = new RazorTemplate();
                string htmlNotif = rz.ParseRazorTemplate<NewTopicNotificationModel>
                    ("~/Website/Views/NotificationPartials/NewTopic.cshtml", nt);

                foreach (var follower in channelFollowers)
                {
                    var ntClone = new NewTopicNotificationModel(nt);
                    NimbusHubContext.Clients.Group(NimbusHub.GetFollowerGroupName(follower.UserId)).newMessageNotification(htmlNotif);

                    StoreNotification(ntClone, follower.UserId);

                }
            }
        }

        public void StoreNotification(NewTopicNotificationModel nt, int userid)
        {
            using (var db = DatabaseFactory.OpenDbConnection())
            {

                var dbNotif = new Model.ORM.Notification<NewTopicNotificationModel>()
                {
                    Id = nt.Guid,
                    UserId = userid,
                    IsRead = false,
                    NotificationObject = nt,
                    Timestamp = nt.Timestamp,
                    Type = Model.NotificationTypeEnum.newtopic
                };
                db.Insert<Model.ORM.Notification<NewTopicNotificationModel>>(dbNotif);

            }
        }
    }

    public class NewTopicNotificationModel
    {
        public int TopicId { get; set; }
        public string TopicName { get; set; }
        public int ChannelId { get; set; }
        public string ChannelName { get; set; }
        public string TopicImage { get; set; }

        public string Date { get; set; }
        public string Time { get; set; }
        //Datetime.ToFileTimeUtc()
        public long Timestamp { get; set; }
        public Guid Guid { get; set; }

        /// <summary>
        /// Cria nova instância de NewTopicNotificationModel
        /// </summary>
        public NewTopicNotificationModel() { }

        /// <summary>
        /// Cria cópia de uma instância de NewTopicNotificationModel *com outro Guid*
        /// </summary>
        /// <param name="other">instância de NewTopicNotificationModel</param>
        public NewTopicNotificationModel(NewTopicNotificationModel other)
        {
            this.TopicId = other.TopicId;
            this.TopicName = other.TopicName;
            this.ChannelId = other.ChannelId;
            this.ChannelName = other.ChannelName;
            this.TopicImage = other.TopicImage;
            this.Date = other.Date;
            this.Time = other.Time;
            this.Timestamp = other.Timestamp;
            this.Guid = Guid.NewGuid();
        }
    }
}