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
        public async Task<ActionResult> Upload()
        {
            //se alterar aqui nao esqueça de mudar no metodo Crop()
            const int dimensaoMax = 150;

            if (Request.Files.Count != 1)
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                return Json(new { error = "Bad Request" });
            }

            if (Request.Files[0].ContentLength > 5 * 1024 * 1024) // 5MB
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.RequestEntityTooLarge;
                return Json(new { error = "Too Large" });
            }

            //gera um nome obfuscado mas previsivel
            var nomeFinal = "fullsize-" + NimbusUser.UserId;

            HMACMD5 md5 = new HMACMD5(NimbusConfig.GeneralHMACKey);
            md5.ComputeHash(Encoding.Unicode.GetBytes(nomeFinal));
            nomeFinal = Base32.ToString(md5.Hash).ToLower() + ".jpg";

            //3x o tamanho máximo
            var image = new ImageManipulation(Request.Files[0].InputStream);
            if (image.Width <= dimensaoMax && image.Height <= dimensaoMax)
            {
                //aumenta a imagem, mesmo perdendo definição
                image.Resize(dimensaoMax, dimensaoMax);
            }
            else if (image.Width <= 600 && image.Height <= 450)
            {
                //mantém a imagem no tamanho original
            }
            else
            {
                //reduz a imagem
                image.Resize(600, 450);
            }

            var imageStream = image.SaveToJpeg();

            var blob = new AzureBlob(Const.Azure.TopicContainer, nomeFinal);
            blob.UploadStreamToAzure(imageStream);

            var pathFinal = blob.BlockBlob.Uri.AbsoluteUri.Replace("https://", "http://");
            //adiciona query string para atrapalhar o cache =)
            pathFinal += "?x=" + DateTime.Now.ToFileTime().ToString();

            //pega o nome do arquivo
            //nome final = onde vai ser armazendo
            //pega o caminho da pasta que vai ser gravado o arquivo e sava
            //retorna a img JA salva  para o json colocar na tela 

            return Json(new { url = pathFinal });
        }

       
    }             
}