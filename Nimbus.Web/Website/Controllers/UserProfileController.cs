using Nimbus.Web.API;
using Nimbus.Web.Website.Models;
using System.IO;
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
            var nomeFinal = "../Uploads/imagem" + Path.GetExtension(nomeOriginal);
            var pathFinal = Server.MapPath(nomeFinal);

            Request.Files[0].SaveAs(pathFinal);
            return Json(new { url = nomeFinal });
        }

    }
}