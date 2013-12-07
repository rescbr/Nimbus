using Nimbus.Web.Website.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Nimbus.Web.Website.Controllers
{
    [Authorize]
    public class SearchController:NimbusWebController
    {

        public ActionResult Index(string text, int filter)
        {
            var channelApi = ClonedContextInstance<API.Controllers.ChannelController>();
            var topicApi = ClonedContextInstance<API.Controllers.TopicController>();
            var userApi = ClonedContextInstance<API.Controllers.UserController>();

             var search = new SearchModel();
             search.Text = text;
             if (filter == 0) //todos
             {
                 
             }
             else if (filter == 1)//channel
             {
                 search.ItensFound = channelApi.SearchChannel(text);
                 search.FieldType = 1;
             }
             else if (filter == 2)//topic
             {
                 search.ItensFound = topicApi.SearchTopic(text);
                 search.FieldType = 2;
             }
             else if (filter == 3)//user
             {
                 search.ItensFound = userApi.SearchUser(text);
                 search.FieldType = 3;
             }

            return View("SearchContent", search);
        }

    }
}