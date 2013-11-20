using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Nimbus.Web.Website.Controllers
{
    [Authorize]
    public class NotificationController : NimbusWebController
    {
        [HttpGet]
        public ActionResult UserNotifications()
        {
            
            return null;
        }
    }
}