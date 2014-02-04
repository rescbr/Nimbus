using Nimbus.Model.ORM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Nimbus.Web.Website.Controllers
{
    public class TopController : NimbusWebController
    {
        public ActionResult Index()
        {
            var topicApi = ClonedContextInstance<API.Controllers.TopicController>();
            var categoryApi = ClonedContextInstance<API.Controllers.CategoryController>();

            //TODO colocar p chamar a funçao CERTA
            var topics = topicApi.TopTopics();//inicial é trazer todos os Tts sem ser filtrado por categoria
            var lstCat = new List<Category>();
            lstCat = categoryApi.showAllCategory();

            ViewBag.lstCat = lstCat.OrderBy(c => c.Name).ToList();
            return View("TopTopics", topics);
        }
    }
}