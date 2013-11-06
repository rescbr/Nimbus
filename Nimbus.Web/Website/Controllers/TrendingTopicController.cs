using Nimbus.Model.Bags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Nimbus.Web.Website.Controllers
{
    public class TrendingTopicController:NimbusWebController
    {
        [Authorize]
        public ActionResult Index(int id)
        {
            var topicApi = ClonedContextInstance<API.Controllers.TopicController>();

            List<TopicBag> topics = new List<TopicBag>();
            //TODO colocar p chamar a funçao CERTA
            topics = topicApi.ShowTTopic(0);//inicial é trazer todos os Tts sem ser filtrado por categoria

            return View("TrendingTopic", topics);
        }

    }
}