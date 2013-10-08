using Nimbus.Web.API;
using Nimbus.Web.Security;
using Nimbus.Web.Website.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using WebApiContrib.Formatting.Html;

namespace Nimbus.Web.Website.Controllers
{
    public class UserProfileController : NimbusApiController
    {
        public View Get(string redirect = null)
        {
            var channelApi = ClonedContextInstance<API.Controllers.ChannelController>(); 
            var userApi = ClonedContextInstance<API.Controllers.UserController>();
            var userprofile = new UserProfileModel()
            {
                CurrentUser = NimbusUser,
                Channel = channelApi.UserChannelPaid(NimbusUser.UserId),
                user = userApi.showProfile()
            };
            return new View("Website.UserProfile", userprofile);
        }

        //fazer a parte de post: enviar mensagem e editar perfil
    }
}