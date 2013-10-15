using Nimbus.Web.API;
using Nimbus.Web.Utils;
using Nimbus.Web.Website.Models;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;


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
        public ActionResult Upload()
        {
            var nomeOriginal = Request.Files[0].FileName;
            var nomeFinal = "../uploads/imagem" + Path.GetExtension(nomeOriginal);
            var pathFinal = Server.MapPath(nomeFinal);

            Request.Files[0].SaveAs(pathFinal);
            return Json(new { url = nomeFinal });            
        }

        [Authorize]
        [HttpPost]
        public ActionResult Crop()
        {
            var imagem_url = Request.Form["url"];
            var x1 = int.Parse(Request.Form["x1"]);
            var x2 = int.Parse(Request.Form["x2"]);
            var y1 = int.Parse(Request.Form["y1"]);
            var y2 = int.Parse(Request.Form["y2"]);

            var nomeFinal = "../uploads/imagem_crop" + Path.GetExtension(imagem_url);
            
            using (var response = new StreamReader(Server.MapPath(imagem_url)))
            {
                Bitmap imagem = new Bitmap(response.BaseStream);

                int largura = x2 - x1;
                int altura = y2 - y1;

                Bitmap target = new Bitmap(largura, altura);
                Rectangle cropRect = new Rectangle(x1, y1, largura, altura);

                using (Graphics g = Graphics.FromImage(target))
                {
                    g.DrawImage(imagem, new Rectangle(0, 0, largura, altura), cropRect, GraphicsUnit.Pixel);
                    using (var fileStream = new FileStream(Server.MapPath(nomeFinal), FileMode.OpenOrCreate))
                    {
                        target.Save(fileStream, imagem.RawFormat);
                        fileStream.Flush();
                    }

                }
            }
            return Json(new { imagem_recortada = nomeFinal });
        }



    }
}