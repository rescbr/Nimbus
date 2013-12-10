using Nimbus.Model.Bags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Nimbus.Web.Website.Controllers
{
    public class TrendingController:NimbusWebController
    {
        [Authorize]
        public ActionResult Index()
        {
            var topicApi = ClonedContextInstance<API.Controllers.TopicController>();

            //TODO colocar p chamar a funçao CERTA
            var topics = topicApi.TrendingTopics();//inicial é trazer todos os Tts sem ser filtrado por categoria

            return View("TrendingTopics", topics);
        }

    }
}