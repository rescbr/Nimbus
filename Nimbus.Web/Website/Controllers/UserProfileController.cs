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
using System;
using System.Net;


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
            var categoryApi = ClonedContextInstance<API.Controllers.CategoryController>();
            var userprofile = new UserProfileModel()
            {
                CurrentUser = NimbusUser,
                ChannelPaid = channelApi.UserChannelPaid(NimbusUser.UserId),
                User = userApi.showProfile(),
                ChannelFollow = channelApi.FollowsChannel(NimbusOrganization.Id),
                MyChannels = channelApi.MyChannel(),
                ReadLater = channelApi.showReadLaterChannel(NimbusOrganization.Id),
                Messages = msgApi.ReceivedMessages(),
                Categories = categoryApi.showAllCategory()
            };
            return View("UserProfile", userprofile);
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> Upload()
        {
            //se alterar aqui nao esqueça de mudar no metodo Crop()
            const int dimensaoMax = 200;

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

            var blob = new AzureBlob(Const.Azure.AvatarContainer, nomeFinal);
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

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> Crop()
        {
            //se alterar aqui nao esqueça de mudar no metodo Crop()
            const int dimensaoMax = 200;

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
            if (largura < dimensaoMax || altura < dimensaoMax)
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                return Json(new { error = "Image should be at least 130x130" });
            }

            if (largura != altura)
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.BadRequest;
                return Json(new { error = "Image should be square" });
            }

            //pega a imagem do azure gerando um nome previsivel
            var nomeImagemOriginal = "fullsize-" + NimbusUser.UserId;

            HMACMD5 md5 = new HMACMD5(NimbusConfig.GeneralHMACKey);
            md5.ComputeHash(Encoding.Unicode.GetBytes(nomeImagemOriginal));
            nomeImagemOriginal = Base32.ToString(md5.Hash).ToLower() + ".jpg";

            Stream imgResize = null;
            var origBlob = new AzureBlob(Const.Azure.AvatarContainer, nomeImagemOriginal);

            using (var streamImgOrig = origBlob.DownloadToMemoryStream())
            {
                var img = new ImageManipulation(streamImgOrig);
                img.Crop(x1, y1, x2, y2);
                img.Resize(dimensaoMax, dimensaoMax);
                imgResize = img.SaveToJpeg();
            }

            origBlob.Delete();

            var nomeImgAvatar = DateTime.Now.ToFileTime().ToString() + "avatar-" + NimbusUser.UserId;
            md5.ComputeHash(Encoding.Unicode.GetBytes(nomeImgAvatar));
            nomeImgAvatar = Base32.ToString(md5.Hash).ToLower() + ".jpg";

            var blob = new AzureBlob(Const.Azure.AvatarContainer, nomeImgAvatar);
            blob.UploadStreamToAzure(imgResize);

            var pathFinal = blob.BlockBlob.Uri.AbsoluteUri.Replace("https://", "http://");

            using (var db = DatabaseFactory.OpenDbConnection())
            {
                var user = db.Where<User>(u => u.Id == NimbusUser.UserId).FirstOrDefault();

                //apaga imagem antiga
                var uriAntigo = new Uri(user.AvatarUrl).Segments;
                var blobAntigo = new AzureBlob(Const.Azure.AvatarContainer, uriAntigo[uriAntigo.Length - 1]);
                try { blobAntigo.Delete(); }
                catch { }

                user.AvatarUrl = pathFinal;
                db.Save(user);

                //ATENÇÃO! Ao alterar informações presentes no NimbusUser, 
                //lembre-se de atualizar no cache também!
                Session[Const.UserSession] = DatabaseLogin.GetNimbusPrincipal(user);
            }

            //depois que salvar no azure retorna por json p mostrar na tela a imagem final

            //adiciona query string para atrapalhar o cache =)
            pathFinal += "?x=" + DateTime.Now.ToFileTime().ToString();
            return Json(new { url = pathFinal });
        }

        [HttpPost]
        public ActionResult SaveNewChannel()
        {
                var channelAPI = ClonedContextInstance<API.Controllers.ChannelController>();
                var categoryAPI = ClonedContextInstance<API.Controllers.CategoryController>(); 
                Channel channel = new Channel();
                int idCateg = Convert.ToInt32(Request.Form["slcCategory"]);

                channel.CategoryId = idCateg;
                channel.CreatedOn = DateTime.Now;
                channel.Description = Request.Form["txtaDescNewChannel"];
                channel.Followers = 0;
                channel.ImgUrl = categoryAPI.GetImgTopChannel(idCateg); 
                channel.IsCourse = Convert.ToBoolean(Request.Form["isCourse"]);
                channel.IsPrivate = false;
                channel.LastModification = DateTime.Now;
                channel.Name = Request.Form["txtNameNewChannel"];
                channel.OpenToComments = Convert.ToBoolean(Request.Form["openComment"]); ;
                channel.OrganizationId = NimbusOrganization.Id;
                channel.OwnerId = NimbusUser.UserId;
                channel.Price = 0;
                channel.Visible = true;
            try
            {

                channel = channelAPI.NewChannel(channel);                
                return RedirectToRoute(new { controller = "channel", action = "index", id = channel.Id});
                
            }
            catch (Exception ex)
            {
                throw ex; //TODO
            }
        }

    }
}