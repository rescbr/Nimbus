using Nimbus.Web.Security;
using Nimbus.Web.Utils;
using Nimbus.Web.Website.Models;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using ServiceStack.OrmLite;
using Nimbus.Model.ORM;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Net;


namespace Nimbus.Web.Website.Controllers
{
    public class UserProfileController : NimbusWebController
    {
        [Authorize]
        public async Task<ActionResult> Index(int? id)
        {
            var channelApi = ClonedContextInstance<API.Controllers.ChannelController>();
            var userApi = ClonedContextInstance<API.Controllers.UserController>();
            var msgApi = ClonedContextInstance<API.Controllers.MessageController>();
            var categoryApi = ClonedContextInstance<API.Controllers.CategoryController>();
            var topicApi = ClonedContextInstance<API.Controllers.TopicController>();

            var taskChannelPaid = Task.Run(() => channelApi.UserChannelPaid(NimbusUser.UserId));
            var taskUser = Task.Run(() => userApi.showProfile(id));
            var taskChannelFollow = Task.Run(() => channelApi.FollowsChannel(NimbusOrganization.Id, 0));
            var taskMyChannels = Task.Run(() => channelApi.MyChannel(id , 0));
            var taskReadLater = Task.Run(() => topicApi.showReadLaterTopic(NimbusOrganization.Id, 0));
            var taskTopicFavorite = Task.Run(() => topicApi.TopicsFavoriteUsers(NimbusOrganization.Id, 0));
            var taskMessages = Task.Run(() => msgApi.ReceivedMessages(0));
            var taskCountMsgSend = Task.Run(() => msgApi.CountSentMessages(0));
            var taskCategories = Task.Run(() => categoryApi.showAllCategory());
            var taskChannelManager = Task.Run(()=>channelApi.ModeratorChannel(id, 0));

            await Task.WhenAll(taskChannelPaid, taskUser, taskChannelFollow, taskMyChannels, taskReadLater, taskMessages, taskCategories, taskChannelManager);

            var userprofile = new UserProfileModel()
            {
                CurrentUser = NimbusUser,
                //ChannelPaid = taskChannelPaid.Result,
                User =  taskUser.Result,
                ChannelFollow = taskChannelFollow.Result,
                MyChannels =  taskMyChannels.Result,
                ReadLater = taskReadLater.Result,
                Messages = taskMessages.Result,
                CountMessageSend = taskCountMsgSend.Result,
                Categories = taskCategories.Result,
                TopicsFavorite = taskTopicFavorite.Result,
                ChannelMannager = taskChannelManager.Result
            };
            return View("UserProfile", userprofile);
        }

        [Authorize]
        [HttpPost]
        public ActionResult Upload()
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

            var pathFinal = blob.BlockBlob.Uri.AbsoluteUri.Replace("https://", "http://").Replace("***REMOVED***", "storage.portalnimbus.com.br");
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
        public ActionResult Crop()
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

            var origBlob = new AzureBlob(Const.Azure.AvatarContainer, nomeImagemOriginal);

            var streamImgOrig = origBlob.DownloadToMemoryStream();

            var img = new ImageManipulation(streamImgOrig);
            img.Crop(x1, y1, x2, y2);
            img.Resize(dimensaoMax, dimensaoMax);
            
            string pathFinal = UploadAvatar(img, NimbusUser.UserId.ToString());
            
            using (var db = DatabaseFactory.OpenDbConnection())
            {
                var user = db.Where<User>(u => u.Id == NimbusUser.UserId).FirstOrDefault();

                //apaga imagem antiga
                //if (user.AvatarUrl != null && user.AvatarUrl != "/images/Utils/person_icon.png")
                //{
                //    try
                //    {
                //        var uriAntigo = new Uri(user.AvatarUrl).Segments;
                //        var blobAntigo = new AzureBlob(Const.Azure.AvatarContainer, uriAntigo[uriAntigo.Length - 1]);
                //        blobAntigo.Delete();
                //    }
                //    catch { }
                //}
                user.AvatarUrl = pathFinal;
                db.Save(user);
                
                //ATENÇÃO! Ao alterar informações presentes no NimbusUser, 
                //lembre-se de atualizar no cache também!
                //Session[Const.UserSession] = DatabaseLogin.GetNimbusPrincipal(user);
            }

            //depois que salvar no azure retorna por json p mostrar na tela a imagem final
            origBlob.Delete();
            //adiciona query string para atrapalhar o cache =)
            pathFinal += "?x=" + DateTime.Now.ToFileTime().ToString();
            return Json(new { url = pathFinal });
        }

        [Authorize]
        [HttpPost]
        public ActionResult SaveNewChannel()
        {
                var channelAPI = ClonedContextInstance<API.Controllers.ChannelController>();
                var categoryAPI = ClonedContextInstance<API.Controllers.CategoryController>(); 
                Channel channel = new Channel();
                int idCateg = Convert.ToInt32(Request.Form["slcCategory"]);

                channel.CategoryId = idCateg;
                channel.Description = Request.Form["txtaDescNewChannel"];
                channel.ImgUrl = categoryAPI.GetImgTopChannel(idCateg).ToLower(); 
                channel.IsCourse = Convert.ToBoolean(Request.Form["isCourse"]);
                channel.IsPrivate = false;
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

        public static string UploadAvatar(ImageManipulation img, string id)
        {
            string pathAvatar;
            HMACMD5 md5 = new HMACMD5(NimbusConfig.GeneralHMACKey);
            var nomeImgAvatar = "avatar-" + id;
            md5.ComputeHash(Encoding.Unicode.GetBytes(nomeImgAvatar));
            var nomeHashExt = Base32.ToString(md5.Hash).ToLower() + ".jpg";
            nomeImgAvatar = "av130x130/" + nomeHashExt;
            var nomeImgAvatar180x100 = "av180x100/" + nomeHashExt;
            var nomeImgAvatar35x35 = "av35x35/" + nomeHashExt;
            var nomeImgAvatar60x60 = "av60x60/" + nomeHashExt;


            //envia as imagens redimensionadas
            Task<string> img1 = Task.Run(() =>
            {
                var clone = new ImageManipulation(img);
                clone.FitSize(200, 200); //muito embora a url é av130x130, o tamanho do avatar é 200x200.
                var blob = new AzureBlob(Const.Azure.AvatarContainer, nomeImgAvatar);
                blob.UploadStreamToAzure(clone.SaveToJpeg());
                return blob.BlockBlob.Uri.AbsoluteUri.Replace("https://", "http://").Replace("***REMOVED***", "storage.portalnimbus.com.br");
            });

            Task<string> img2 = Task.Run(() =>
            {
                var clone = new ImageManipulation(img);
                clone.FitSize(180, 100);
                var blob180x100 = new AzureBlob(Const.Azure.AvatarContainer, nomeImgAvatar180x100);
                blob180x100.UploadStreamToAzure(clone.SaveToJpeg());
                return string.Empty;
            });

            Task<string> img3 = Task.Run(() =>
            {
                var clone = new ImageManipulation(img);
                clone.FitSize(60, 60);
                var blob60x60 = new AzureBlob(Const.Azure.AvatarContainer, nomeImgAvatar60x60);
                blob60x60.UploadStreamToAzure(clone.SaveToJpeg());
                return string.Empty;
            });

            Task<string> img4 = Task.Run(() =>
            {
                var clone = new ImageManipulation(img);
                clone.FitSize(35, 35);
                var blob35x35 = new AzureBlob(Const.Azure.AvatarContainer, nomeImgAvatar35x35);
                blob35x35.UploadStreamToAzure(clone.SaveToJpeg());
                return string.Empty;
            });

            Task.WaitAll(img1, img2, img3, img4);
            pathAvatar = img1.Result;
            return pathAvatar;
        }
    }
}