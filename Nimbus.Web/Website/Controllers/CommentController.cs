using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Nimbus.Web.Website.Controllers
{
    [Authorize]
    public class CommentController : NimbusWebController
    {
        [HttpGet]
        public ActionResult Index(int id)
        {
            //TODO: Terminar
            var commentApi = ClonedContextInstance<API.Controllers.CommentController>();
            return View("~/Website/Views/CommentPartials/PartialTopicComment.cshtml", null);
        }
    }
}