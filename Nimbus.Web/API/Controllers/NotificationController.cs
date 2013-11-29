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
                    .Take(15).ToList();
            }
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