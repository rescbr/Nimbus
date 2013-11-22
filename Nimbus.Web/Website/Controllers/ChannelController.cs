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
        public ActionResult Index(int id)
        {
            var channelApi = ClonedContextInstance<API.Controllers.ChannelController>();
            var userApi = ClonedContextInstance<API.Controllers.UserController>();
            var msgApi = ClonedContextInstance<API.Controllers.MessageController>();
            var topicApi = ClonedContextInstance<API.Controllers.TopicController>();
            var commentApi = ClonedContextInstance<API.Controllers.CommentController>();
            var categoryApi = ClonedContextInstance<API.Controllers.CategoryController>();

            var channel = new ChannelModel()
            {
                Tags = channelApi.ShowTagChannel(id),
                Moderators = channelApi.ShowModerators(id),
                CurrentChannel = channelApi.ShowChannel(id),
                AllTopics = topicApi.AbstTopic( id,string.Empty, 0),
                Messages = msgApi.ChannelReceivedMessages(id),
                Comments = commentApi.ShowChannelComment(id),
                CurrentUser = NimbusUser,
                RolesCurrentUser = channelApi.ReturnRolesUser(id),
                CcMessageReceiver = channelApi.GetMessageModerators(id),
                NewTopic = null,
                Category = categoryApi.showAllCategory()
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