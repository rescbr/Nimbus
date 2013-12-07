using Nimbus.Model.ORM;
using Nimbus.Web.Utils;
using ServiceStack.OrmLite;
using System;
using System.Linq;

namespace Nimbus.Web.Notifications
{
    public class TopicNotification : NimbusNotificationBase
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

                var nt = new TopicNotificationModel()
                {
                    TopicId = topic.Id,
                    TopicName = topic.Title,
                    ChannelId = topic.ChannelId,
                    ChannelName = channel.Name,
                    TopicImage = topic.ImgUrl,
                    NotificationType = Model.NotificationTypeEnum.newtopic,
                    
                    Date = now.ToShortDateString(),
                    Time = now.ToShortTimeString(),
                    Timestamp = now.ToFileTimeUtc(),
                };

                var rz = new RazorTemplate();
                string htmlNotif = rz.ParseRazorTemplate<TopicNotificationModel>
                    ("~/Website/Views/NotificationPartials/NewTopic.cshtml", nt);

                foreach (var follower in channelFollowers)
                {
                    var ntClone = new TopicNotificationModel(nt);
                    NimbusHubContext.Clients.Group(NimbusHub.GetFollowerGroupName(follower.UserId)).newMessageNotification(htmlNotif);

                    StoreNotification(ntClone, follower.UserId);

                }

                var ntChClone = new TopicNotificationModel(nt);
                StoreNotificationChannel(ntChClone);

            }
        }

        public void EditTopic(Topic topic)
        {
            var now = DateTime.UtcNow;
            var nt = new TopicNotificationModel()
            {
                TopicId = topic.Id,
                TopicName = topic.Title,
                ChannelId = topic.ChannelId,
                TopicImage = topic.ImgUrl,
                NotificationType = Model.NotificationTypeEnum.edittopic,

                Date = now.ToShortDateString(),
                Time = now.ToShortTimeString(),
                Timestamp = now.ToFileTimeUtc(),
                Guid = Guid.NewGuid()
            };

            StoreNotificationChannel(nt);
        }

        public void DeleteTopic(Topic topic)
        {
            var now = DateTime.UtcNow;
            var nt = new TopicNotificationModel()
            {
                TopicId = topic.Id,
                TopicName = topic.Title,
                ChannelId = topic.ChannelId,
                TopicImage = topic.ImgUrl,
                NotificationType = Model.NotificationTypeEnum.deletetopic,

                Date = now.ToShortDateString(),
                Time = now.ToShortTimeString(),
                Timestamp = now.ToFileTimeUtc(),
                Guid = Guid.NewGuid()
            };

            StoreNotificationChannel(nt);
        }

        private void StoreNotification(TopicNotificationModel nt, int userid)
        {
            using (var db = DatabaseFactory.OpenDbConnection())
            {

                var dbNotif = new Model.ORM.Notification<TopicNotificationModel>()
                {
                    Id = nt.Guid,
                    UserId = userid,
                    IsRead = false,
                    NotificationObject = nt,
                    Timestamp = nt.Timestamp,
                    Type = Model.NotificationTypeEnum.newtopic
                };
                db.Insert<Model.ORM.Notification<TopicNotificationModel>>(dbNotif);

            }
        }

        private void StoreNotificationChannel(TopicNotificationModel nt)
        {
            using (var db = DatabaseFactory.OpenDbConnection())
            {

                var dbNotif = new Model.ORM.Notification<TopicNotificationModel>()
                {
                    Id = nt.Guid,
                    ChannelId = nt.ChannelId,
                    IsRead = false,
                    NotificationObject = nt,
                    Timestamp = nt.Timestamp,
                    Type = Model.NotificationTypeEnum.newtopic
                };
                db.Insert<Model.ORM.Notification<TopicNotificationModel>>(dbNotif);

            }
        }
    }

    public class TopicNotificationModel
    {
        public int TopicId { get; set; }
        public string TopicName { get; set; }
        public int ChannelId { get; set; }
        public string ChannelName { get; set; }
        public string TopicImage { get; set; }
        public Model.NotificationTypeEnum NotificationType { get; set; }

        public string Date { get; set; }
        public string Time { get; set; }
        //Datetime.ToFileTimeUtc()
        public long Timestamp { get; set; }
        public Guid Guid { get; set; }

        /// <summary>
        /// Cria nova instância de TopicNotificationModel
        /// </summary>
        public TopicNotificationModel() { }

        /// <summary>
        /// Cria cópia de uma instância de TopicNotificationModel *com outro Guid*
        /// </summary>
        /// <param name="other">instância de TopicNotificationModel</param>
        public TopicNotificationModel(TopicNotificationModel other)
        {
            this.TopicId = other.TopicId;
            this.TopicName = other.TopicName;
            this.ChannelId = other.ChannelId;
            this.ChannelName = other.ChannelName;
            this.TopicImage = other.TopicImage;
            this.Date = other.Date;
            this.Time = other.Time;
            this.Timestamp = other.Timestamp;
            this.NotificationType = other.NotificationType;
            this.Guid = Guid.NewGuid();
        }
    }
}