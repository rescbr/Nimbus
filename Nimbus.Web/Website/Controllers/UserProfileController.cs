using Nimbus.Web.Security;
using Nimbus.Web.Utils;
using Nimbus.Web.Website.Models;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using ServiceStack.OrmLite;
using Nimbus.DB.ORM;
using System.Collections.Generic;
using System.Linq;

namespace Nimbus.Web.Website.Controllers
{
    public class UserProfileController : NimbusWebController
    {
        [Authorize]
        public ActionResult Index()
        {
            var channelApi = ClonedContextInstance<API.Controllers.ChannelController>();
            var userApi = ClonedContextInstance<API.Controllers.UserController>();
            var msgApi = ClonedContextInstance<API.Controllers.MessageController>();
            var userprofile = new UserProfileModel()
            {
                CurrentUser = NimbusUser,
                ChannelPaid = channelApi.UserChannelPaid(NimbusUser.UserId),
                User = userApi.showProfile(),
                ChannelFollow = channelApi.FollowsChannel(NimbusOrganization.Id),
                MyChannels = channelApi.MyChannel(),
                ReadLater = channelApi.showReadLaterChannel(NimbusOrganization.Id),
                Messages = msgApi.ReceivedMessages()
            };
            return View("UserProfile", userprofile);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> Upload()
        {
            if (Request.Files.Count != 1)
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                return Json(new { error = "Bad Request" });
            }

            if (Request.Files[0].ContentLength > 10 * 1024 * 1024) //10 MB
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.RequestEntityTooLarge;
                return Json(new { error = "Too Large" });
            }

            var nomeFinal = "-fullsize-" + NimbusUser.UserId;

            HMACMD5 md5 = new HMACMD5(NimbusConfig.GeneralHMACKey);
            md5.ComputeHash(Encoding.Unicode.GetBytes(nomeFinal));
            nomeFinal = Base32.ToString(md5.Hash).ToLower() + nomeFinal + ".jpg";

            //3x o tamanho máximo
            var image = new ImageManipulation(Request.Files[0].InputStream);
            if (image.Width <= 262 && image.Height <= 262)
            {
                //aumenta a imagem, mesmo perdendo definição
                image.Resize(262, 262);
            }
            else if (image.Width <= 600 && image.Height <= 450)
            {
                //mantém a imagem no tamanho original
            }
            else { 
                //reduz a imagem
                image.Resize(600, 450);
            }

            var imageStream = image.SaveToJpeg();

            var blob = new AzureBlob("avatarupload", nomeFinal);
            blob.UploadStreamToAzure(imageStream);

            var pathFinal = blob.BlockBlob.Uri.AbsoluteUri.Replace("https://", "http://");

            //pega o nome do arquivo
            //nome final = onde vai ser armazendo
            //pega o caminho da pasta que vai ser gravado o arquivo e sava
            //retorna a img JA salva  para o json colocar na tela 
            return Json(new { url = pathFinal });
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> Crop()
        { //262 x 100
            int x1, x2, y1, y2 = 0;

            try
            {
                x1 = int.Parse(Request.Form["x1"]);
                x2 = int.Parse(Request.Form["x2"]);
                y1 = int.Parse(Request.Form["y1"]);
                y2 = int.Parse(Request.Form["y2"]);
            }
            catch
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                return Json(new { error = "Invalid size" });
            }

            int largura = x2 - x1;
            int altura = y2 - y1;

            //de acordo com o estilo no css
            if (largura < 262 || altura < 262)
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                return Json(new { error = "Image should be at least 262x262" });
            }

            if (largura != altura)
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                return Json(new { error = "Image should be square" });
            }

            //pega a imagem do azure
            var nomeImagemOriginal = "-fullsize-" + NimbusUser.UserId;

            HMACMD5 md5 = new HMACMD5(NimbusConfig.GeneralHMACKey);
            md5.ComputeHash(Encoding.Unicode.GetBytes(nomeImagemOriginal));
            nomeImagemOriginal = Base32.ToString(md5.Hash).ToLower() + nomeImagemOriginal + ".jpg";

            Stream imgResize = null;
            var origBlob = new AzureBlob("avatarupload", nomeImagemOriginal);

            using (var streamImgOrig = origBlob.DownloadToMemoryStream())
            {
                var img = new ImageManipulation(streamImgOrig);
                img.Crop(x1, y1, x2, y2);
                img.Resize(262, 262);
                imgResize = img.SaveToJpeg();
            }

            origBlob.Delete();

            var nomeImgAvatar = "-avatar-" + NimbusUser.UserId;
            md5.ComputeHash(Encoding.Unicode.GetBytes(nomeImagemOriginal));
            nomeImgAvatar = Base32.ToString(md5.Hash).ToLower() + nomeImgAvatar + ".jpg";

            var blob = new AzureBlob("avatarupload", nomeImgAvatar);
            blob.UploadStreamToAzure(imgResize);

            var pathFinal = blob.BlockBlob.Uri.AbsoluteUri.Replace("https://", "http://");

            using (var db = DatabaseFactory.OpenDbConnection())
            {
                var user = db.Where<User>(u => u.Id == NimbusUser.UserId).FirstOrDefault();
                user.AvatarUrl = pathFinal;
                db.Save(user);
            }


            //depois que salvar no azure retorna por json p mostrar na tela a imagem final
            return Json(new { url = pathFinal });
        }
    }
}