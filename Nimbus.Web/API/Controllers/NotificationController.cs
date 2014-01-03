using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Nimbus.Web.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Nimbus.Model.ORM;
using ServiceStack.OrmLite;
using Nimbus.Web.Utils;
using System.Text;
using ServiceStack.Text;
using System.Threading.Tasks;

namespace Nimbus.Web.API.Controllers
{
    public class NotificationWrapper
    {
        public int Count { get; set; }
        public string Html { get; set; }
        public Guid LastNotificationGuid { get;set; }
    }

    [NimbusAuthorize]
    public class NotificationController : NimbusApiController
    {
        [HttpGet]
        [ActionName("DefaultAction")]
        public NotificationWrapper Get()
        {
            List<Notification<string>> allNotifications;
            using (var db = DatabaseFactory.OpenDbConnection())
            {
                allNotifications = db.Where<Notification<string>>
                    (n => n.UserId == NimbusUser.UserId)
                    .OrderByDescending(n => n.Timestamp)
                    .Take(6).ToList();
            }


            return GenerateUserNotificationHtml(allNotifications);

        }

        [HttpGet]
        [ActionName("DefaultAction")]
        public NotificationWrapper Get(Guid after)
        {
            List<Notification<string>> allNotifications;
            using (var db = DatabaseFactory.OpenDbConnection())
            {
                allNotifications = db.Where<Notification<string>>
                    (n => n.UserId == NimbusUser.UserId 
                        && n.Timestamp <= db.Where<Notification<string>>
                                            (nt => nt.UserId == NimbusUser.UserId && nt.Id == after).Single().Timestamp
                        && n.Id != after)
                    .OrderByDescending(n => n.Timestamp)
                    .Take(6).ToList();
            }

            return GenerateUserNotificationHtml(allNotifications);
        }

        [HttpGet]
        public NotificationWrapper Channel(int id)
        {
            List<Notification<string>> allNotifications;
            using (var db = DatabaseFactory.OpenDbConnection())
            {
                allNotifications = db.Where<Notification<string>>
                    (n => n.ChannelId == id)
                    .OrderByDescending(n => n.Timestamp)
                    .Take(6).ToList();
            }

            return GenerateChannelNotificationHtml(allNotifications);

        }

        [HttpGet]
        public NotificationWrapper Channel(int id, Guid after)
        {
            List<Notification<string>> allNotifications;
            using (var db = DatabaseFactory.OpenDbConnection())
            {
                allNotifications = db.Where<Notification<string>>
                    (n => n.ChannelId == id
                        && n.Timestamp <= db.Where<Notification<string>>
                                            (nt => nt.ChannelId == id && nt.Id == after).Single().Timestamp
                        && n.Id != after)
                    .OrderByDescending(n => n.Timestamp)
                    .Take(6).ToList();
            }

            return GenerateChannelNotificationHtml(allNotifications);
        }

        [NonAction]
        private NotificationWrapper GenerateUserNotificationHtml(List<Notification<string>> allNotifications)
        {
            if (allNotifications.Count == 0) return new NotificationWrapper() { Count = 0 };

            var razor = new RazorTemplate();
            var sbuilder = new StringBuilder();
            //Parallel.ForEach(allNotifications, (notification) =>
            foreach (var notification in allNotifications)
            {
                if (notification.Type == Model.NotificationTypeEnum.message)
                {
                    MessageNotificationModel model = TypeSerializer.DeserializeFromString
                        <MessageNotificationModel>(notification.NotificationObject);

                    sbuilder.Append(
                        razor.ParseRazorTemplate<MessageNotificationModel>
                            ("~/Website/Views/NotificationPartials/Message.cshtml", model));

                }
                else if (notification.Type == Model.NotificationTypeEnum.newtopic)
                {
                    TopicNotificationModel model = TypeSerializer.DeserializeFromString
                        <TopicNotificationModel>(notification.NotificationObject);

                    sbuilder.Append(
                        razor.ParseRazorTemplate<TopicNotificationModel>
                            ("~/Website/Views/NotificationPartials/NewTopic.cshtml", model));
                }
                else if (notification.Type == Model.NotificationTypeEnum.moderatorinvite)
                {
                    ModeratorNotificationModel model = TypeSerializer.DeserializeFromString
                        <ModeratorNotificationModel>(notification.NotificationObject);

                    sbuilder.Append(
                        razor.ParseRazorTemplate<ModeratorNotificationModel>
                            ("~/Website/Views/NotificationPartials/Accept.cshtml", model));
                }
            }//);

            return new NotificationWrapper()
            {
                Count = allNotifications.Count,
                Html = sbuilder.ToString(),
                LastNotificationGuid = allNotifications.Last().Id
            };
        }

        [NonAction]
        private NotificationWrapper GenerateChannelNotificationHtml(List<Notification<string>> allNotifications)
        {
            if (allNotifications.Count == 0) return new NotificationWrapper() { Count = 0 };

            var razor = new RazorTemplate();
            var sbuilder = new StringBuilder();
            //Parallel.ForEach(allNotifications, (notification) =>
            foreach (var notification in allNotifications)
            {
                if(notification.Type == Model.NotificationTypeEnum.newtopic || 
                    notification.Type == Model.NotificationTypeEnum.edittopic ||
                    notification.Type == Model.NotificationTypeEnum.deletetopic)
                {
                    TopicNotificationModel model = TypeSerializer.DeserializeFromString
                        <TopicNotificationModel>(notification.NotificationObject);

                    sbuilder.Append(
                        razor.ParseRazorTemplate<TopicNotificationModel>
                            ("~/Website/Views/NotificationPartials/ChannelTopic.cshtml", model));
                }
            }//);

            return new NotificationWrapper()
            {
                Count = allNotifications.Count,
                Html = sbuilder.ToString(),
                LastNotificationGuid = allNotifications.Last().Id
            };
        }
        
        
        [HttpGet]
        public List<MessageNotificationModel> Messages()
        {
            using (var db = DatabaseFactory.OpenDbConnection())
            {
                var msgNotifications = db.Where<Notification<MessageNotificationModel>>
                    (n => n.UserId == NimbusUser.UserId && n.Type == Model.NotificationTypeEnum.message)
                    .OrderByDescending(n => n.Timestamp).Take(15).Select(n => n.NotificationObject);

                return msgNotifications.ToList();
            }
        }

        [HttpGet]
        public List<MessageNotificationModel> Messages(Guid after)
        {
            using (var db = DatabaseFactory.OpenDbConnection())
            {
                var afterNotifications = db.Where<Notification<MessageNotificationModel>>
                    (n => n.UserId == NimbusUser.UserId
                    && n.Timestamp <= 
                        db.Where<Notification<MessageNotificationModel>>
                            (nt => nt.UserId == NimbusUser.UserId && nt.Id == after).Single().Timestamp
                    && n.Id != after).OrderByDescending(n => n.Timestamp).Take(15).Select(n=>n.NotificationObject);

                return afterNotifications.ToList();
            }
        }
    }
}