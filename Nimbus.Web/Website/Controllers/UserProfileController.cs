using Nimbus.Web.API;
using Nimbus.Web.Website.Models;
using System.Web.Http;
using WebApiContrib.Formatting.Html;

namespace Nimbus.Web.Website.Controllers
{
    public class UserProfileController : NimbusApiController
    {
        [Authorize]
        public View Get()
        {
            var channelApi = ClonedContextInstance<API.Controllers.ChannelController>(); 
            var userApi = ClonedContextInstance<API.Controllers.UserController>();
            var userprofile = new UserProfileModel()
            {
                CurrentUser = NimbusUser,
                Channel = channelApi.UserChannelPaid(NimbusUser.UserId),
                User = userApi.showProfile()
            };
            return new View("Website.UserProfile", userprofile);
        }


    }
}