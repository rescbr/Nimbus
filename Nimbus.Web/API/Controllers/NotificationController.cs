using Nimbus.Web.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Nimbus.Web.API.Controllers
{
    [NimbusAuthorize]
    public class NotificationController : NimbusApiController
    {
        [HttpGet]
        public List<MessageNotificationModel> GetMessages()
        {
            return null;
        }
    }
}