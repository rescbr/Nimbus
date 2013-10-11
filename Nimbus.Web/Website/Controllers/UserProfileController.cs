using Nimbus.Web.API;
using Nimbus.Web.Website.Models;
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
            var userprofile = new UserProfileModel()
            {
                CurrentUser = NimbusUser,
                ChannelPaid = channelApi.UserChannelPaid(NimbusUser.UserId),
                User = userApi.showProfile(),
                ChannelFollow = channelApi.FollowsChannel(NimbusOrganization.Id),
                MyChannels = channelApi.MyChannel()
                
            };
            return View("UserProfile", userprofile);
        }


    }
}