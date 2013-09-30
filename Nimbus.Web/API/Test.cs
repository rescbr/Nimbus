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
        //[Authorize]
        public string Get()
        {
            using (var db = DatabaseFactory.OpenDbConnection())
            {
                db.DropAndCreateTables(new Type[]{
                    typeof(Nimbus.DB.ORM.Category),
                    typeof(Nimbus.DB.ORM.Ad), 
                    typeof(Nimbus.DB.ORM.Organization),
                    typeof(Nimbus.DB.ORM.User),
                    typeof(Nimbus.DB.ORM.UserReported),
                    typeof(Nimbus.DB.ORM.Channel),
                    typeof(Nimbus.DB.ORM.ChannelReported),
                    typeof(Nimbus.DB.ORM.ChannelUser),
                    typeof(Nimbus.DB.ORM.OrganizationUser),
                    typeof(Nimbus.DB.ORM.Topic),
                    typeof(Nimbus.DB.ORM.TopicReported),
                    typeof(Nimbus.DB.ORM.Comment),
                    typeof(Nimbus.DB.ORM.CommentReported),
                    typeof(Nimbus.DB.ORM.Message),
                    typeof(Nimbus.DB.ORM.UserTopicFavorite)});
            }
                return "AvatarURL: ";
        }
    }
}