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

            //TODO colocar p chamar a funçao CERTA
            var topics = topicApi.TopTopics();//inicial é trazer todos os Tts sem ser filtrado por categoria
            var lstCat = new List<string>();
            lstCat.Add("asafs");
            ViewBag.lstCat = lstCat;
            return View("TopTopics", topics);
        }
    }
}