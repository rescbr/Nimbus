using Nimbus.Web.Website.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Nimbus.Web.Website.Controllers
{
    public class ResetPasswordController : NimbusWebController
    {
        public ActionResult Index(string reset)
        {
            return View("ResetPassword", new ResetPasswordModel() { Token = reset });
        }
    }
}