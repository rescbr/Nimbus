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

namespace Nimbus.Web.API.Controllers
{
    [NimbusAuthorize]
    public class NotificationController : NimbusApiController
    {
        [HttpGet]
        public List<MessageNotificationModel> GetMessages(int skip = 0)
        {
            using (var db = DatabaseFactory.OpenDbConnection())
            {
                var msgNotifications = db.Where<Notification<MessageNotificationModel>>
                    (n => n.UserId == NimbusUser.UserId && n.Type == Model.NotificationTypeEnum.message)
                    .OrderByDescending(n => n.Timestamp)
                    .Skip(15 * skip).Take(15).Select(n => n.NotificationObject);

                return msgNotifications.ToList();
            }
        }

        public List<MessageNotificationModel> GetMessages(Guid after)
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