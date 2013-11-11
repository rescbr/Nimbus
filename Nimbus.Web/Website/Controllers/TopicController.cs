using Nimbus.Web.Website.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Nimbus.Web.Website.Controllers
{
    [Authorize]
    public class TopicController : NimbusWebController
    {
        public ActionResult Index(int id)
        {
            var channelApi = ClonedContextInstance<API.Controllers.ChannelController>();
            var userApi = ClonedContextInstance<API.Controllers.UserController>();
            var msgApi = ClonedContextInstance<API.Controllers.MessageController>();
            var topicApi = ClonedContextInstance<API.Controllers.TopicController>();
            var commentApi = ClonedContextInstance<API.Controllers.CommentController>();

            var aux = topicApi.ShowTopic(id);

            if (aux == null) throw new HttpException(404, "Topic not found");

            var topic = new TopicModel()
            {
                CurrentTopic = aux,
                CurrentChannel = channelApi.ShowChannel(aux.ChannelId),
                CurrentUser = NimbusUser,
                Comments = null, // commentApi.ShowTopicComment(id), //Renato: usar a Action Comments
                RolesCurrentUser = null, //channelApi.ReturnRolesUser(aux.ChannelId), //Renato: migrado para a API de Comentário
                Category = topicApi.CategoryTopic( aux.Id),
                NumFavorites = topicApi.CountFavorite(id)
            };
            return View("Topic", topic);
        }

        /// <summary>
        /// Retorna os comentários já parseados
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Comments(int id) 
        {
            var commentApi = ClonedContextInstance<API.Controllers.CommentController>();
            var comments = commentApi.ShowTopicComment(id);

            return View("~/Website/Views/CommentPartials/PartialTopicComment.cshtml", comments);

        }

    }
}