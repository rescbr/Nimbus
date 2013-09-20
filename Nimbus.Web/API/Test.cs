using Nimbus.Plumbing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using ServiceStack.OrmLite;
using Nimbus.Web.Middleware;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Nimbus.Web.Security;


namespace Nimbus.Web.API
{
    public class CookieController : NimbusApiController
    {
        [AllowAnonymous]
        public async Task<HttpResponseMessage> Get()
        {
            Guid token;
            string authToken = Token.GenerateToken(
                new NSCInfo()
                {
                    TokenGenerationDate = DateTime.Now.ToUniversalTime(),
                    UserId = 1
                }, 
                out token);

            var loginCookie = new CookieHeaderValue("nsc-session", authToken)
            {
                //params
            };
            
            var response = Request.CreateResponse<string>("Cookie!");
            response.Headers.AddCookies(new CookieHeaderValue[] {
                loginCookie
            });
            return response;
        }
    }
    public class TestController : NimbusApiController
    {
        [Authorize]
        public string Get()
        {
            //using (var db = DatabaseFactory.OpenDbConnection())
            //{   db.DropAndCreateTables(new Type[]{
            //        typeof(Nimbus.DB.Ad), 
            //        typeof(Nimbus.DB.Category),
            //        typeof(Nimbus.DB.User),
            //        typeof(Nimbus.DB.UserReported),
            //        typeof(Nimbus.DB.Organization),
            //        typeof(Nimbus.DB.Channel),
            //        typeof(Nimbus.DB.ChannelReported),
            //        typeof(Nimbus.DB.ChannelUser),
            //        typeof(Nimbus.DB.OrganizationUser),
            //        typeof(Nimbus.DB.Topic),
            //        typeof(Nimbus.DB.TopicReported),
            //        typeof(Nimbus.DB.Comment),
            //        typeof(Nimbus.DB.CommentReported),
            //        typeof(Nimbus.DB.Message),
            //        typeof(Nimbus.DB.UserTopicFavorite)});
            //  }
                return "AvatarURL: ";
        }
    }
}