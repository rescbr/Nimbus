using Nimbus.Plumbing;
using Nimbus.Web.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace Nimbus.Web.Website.Controllers
{

    public class HomeController : NimbusWebController
    {
        [Authorize]
        public ActionResult Index()
        {
            var hm = new HomeModel()
            {
                CurrentUser = NimbusUser,
                CurrentOrganization = NimbusOrganization.Name

            };
            return View("Home", hm);
        }
    }

    public class HomeModel
    {
        public NimbusUser CurrentUser { get; set; }
        public string CurrentOrganization { get; set; }
    }
}