using Nimbus.Web.Website.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ServiceStack.OrmLite;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using Nimbus.Web.Security;
using Nimbus.Web.Utils;
using Nimbus.Model.ORM;
using Nimbus.Model.Bags;
using System.IO;

namespace Nimbus.Web.Website.Controllers
{
    public class ChannelController : NimbusWebController
    {
        [Authorize]
        public async Task<ActionResult> Index(int id)
        {
            var channelApi = ClonedContextInstance<API.Controllers.ChannelController>();
            var userApi = ClonedContextInstance<API.Controllers.UserController>();
            var msgApi = ClonedContextInstance<API.Controllers.MessageController>();
            var topicApi = ClonedContextInstance<API.Controllers.TopicController>();
            var commentApi = ClonedContextInstance<API.Controllers.CommentController>();
            var categoryApi = ClonedContextInstance<API.Controllers.CategoryController>();
            var notificationApi = ClonedContextInstance<API.Controllers.NotificationController>();
            var datamindingApi = ClonedContextInstance<API.Controllers.DataMiningController>();

            var taskTags = Task.Run(() => channelApi.ShowTagChannel(id));
            var taskModerators = Task.Run(() => channelApi.ShowModerators(id));
            var taskCurrentChannel = Task.Run(() => channelApi.ShowChannel(id));
            var taskAllTopics = Task.Run(() => topicApi.AbstTopic(id, string.Empty, 0));
            var taskMessages = Task.Run(() => msgApi.ChannelReceivedMessages(id));
            var taskComments = Task.Run(() => commentApi.ShowChannelComment(id, 0));
            var taskRolesCurrentUser = Task.Run(() => channelApi.ReturnRolesUser(id));
            var taskCcMessageReceiver = Task.Run(() => channelApi.GetMessageModerators(id));
            var taskCategory = Task.Run(() => categoryApi.showAllCategory());
            var taskNotifications = Task.Run(() => notificationApi.Channel(id));
            var taskRelatedChannels = Task.Run(() => datamindingApi.RelatedChannels(id));

            await Task.WhenAll(taskTags, taskModerators, taskCurrentChannel, taskAllTopics, taskMessages, 
                taskComments, taskRolesCurrentUser, taskCcMessageReceiver, taskCategory, taskNotifications, taskRelatedChannels);
            var channel = new ChannelModel()
            {
                Tags              = taskTags.Result,
                Moderators        = taskModerators.Result,
                CurrentChannel    = taskCurrentChannel.Result,
                AllTopics         = taskAllTopics.Result,
                Messages          = taskMessages.Result,
                Comments          = taskComments.Result,
                CurrentUser       = NimbusUser,
                RolesCurrentUser  = taskRolesCurrentUser.Result,
                CcMessageReceiver = taskCcMessageReceiver.Result,
                NewTopic          = null,
                Category          = taskCategory.Result,
                RelatedChannels   = taskRelatedChannels.Result,
                ChannelNotifications = taskNotifications.Result,
            };
            return View("Channels", channel);
        }

        [Authorize]
        [HttpPost]
        public ActionResult Upload()
        {
            //tamanho da imagem: 150x100, olhe a chamada para o metodo image.FitSize().

            if (Request.Files.Count != 1)
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                return Json(new { error = "Bad Request" });
            }

            var file = Request.Files[0];
            if (file.ContentLength > 5 * 1024 * 1024) // 5MB
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.RequestEntityTooLarge;
                return Json(new { error = "Too Large" });
            }

            //nome do arquivo: /container/tp150x100/md5(timestamp + nome arquivo).jpg
            var filename = Path.GetFileNameWithoutExtension(file.FileName);
            var timeFileName = DateTime.UtcNow.ToFileTimeUtc().ToString() + filename;

            HMACMD5 md5 = new HMACMD5(NimbusConfig.GeneralHMACKey);
            md5.ComputeHash(Encoding.Unicode.GetBytes(timeFileName));
            var fileHash = Base32.ToString(md5.Hash).ToLower() + ".jpg";
            var uploadFileName150x100 = "tp150x100/" + fileHash;
            var uploadFileName60x60 = "tp60x60/" + fileHash;

            var image = new ImageManipulation(Request.Files[0].InputStream);
            
            //faz upload da imagem 150x100
            image.FitSize(150, 100);
            var imageStream = image.SaveToJpeg();
            var blob = new AzureBlob(Const.Azure.TopicContainer, uploadFileName150x100);
            blob.UploadStreamToAzure(imageStream);

            image.FitSize(60, 60);
            var image60x60Stream = image.SaveToJpeg();
            var blob60x60 = new AzureBlob(Const.Azure.TopicContainer, uploadFileName60x60);
            blob60x60.UploadStreamToAzure(image60x60Stream);


            var pathFinal = blob.BlockBlob.Uri.AbsoluteUri.Replace("https://", "http://").Replace("***REMOVED***", "storage.portalnimbus.com.br");

            //retorna apenas a 150x100.
            return Json(new { url = pathFinal });
        }

       
    }             
}