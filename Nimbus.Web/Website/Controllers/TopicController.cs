using Nimbus.Model.Bags;
using Nimbus.Web.Security;
using Nimbus.Web.Utils;
using Nimbus.Web.Website.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
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
                RolesCurrentUser = channelApi.ReturnRolesUser(aux.ChannelId), 
                Category = topicApi.CategoryTopic( aux.Id),
                NumFavorites = topicApi.CountFavorite(id),
                NumLikes = topicApi.CountLikes(id),
                NumUnLikes = topicApi.CountUnLikes(id),
                UserLike = topicApi.UserLiked(id),
                FavoriteTopic = topicApi.TopicIsFavorite(id)
            };
            return View("Topic", topic);
        }

        /// <summary>
        /// Retorna os comentários de um topico em HTML
        /// </summary>
        /// <param name="id">id do tópico</param>
        /// <returns></returns>
        public ActionResult Comments(int id) 
        {
            var commentApi = ClonedContextInstance<API.Controllers.CommentController>();
            //var comments = commentApi.ShowTopicComment(id);
            var comments = commentApi.AllTopicComments(id);

            return View("~/Website/Views/CommentPartials/PartialTopicComment.cshtml", comments);

        }

        /// <summary>
        /// Retorna apenas um comentario
        /// </summary>
        /// <param name="id">id do comentario</param>
        /// <returns></returns>
        public ActionResult Comment(int id)
        {
            var commentApi = ClonedContextInstance<API.Controllers.CommentController>();
            var bag = commentApi.GetComment(id);
            return View("~/Website/Views/CommentPartials/PartialComment.cshtml", bag);
        }

        public ActionResult ParentComment(int id)
        {
            var commentApi = ClonedContextInstance<API.Controllers.CommentController>();
            var bag = new List<CommentBag>();
            bag.Add(commentApi.GetParentComment(id));
            return View("~/Website/Views/CommentPartials/PartialTopicComment.cshtml", bag);
        }

    }
}