using Nimbus.Plumbing.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using ServiceStack.OrmLite;

namespace Nimbus.Web.API
{
    public class TestController : NimbusApiController
    {
        public string Get()
        {
            /*using (var db = DatabaseFactory.OpenDbConnection())
            {
                db.DropAndCreateTables(new Type[]{
                    typeof(Nimbus.DB.Ad), 
                    typeof(Nimbus.DB.Category),
                    typeof(Nimbus.DB.User),
                    typeof(Nimbus.DB.Organization),
                    typeof(Nimbus.DB.Channel),
                    typeof(Nimbus.DB.ChannelUser),
                    typeof(Nimbus.DB.OrganizationUser),
                    typeof(Nimbus.DB.Topic),
                    typeof(Nimbus.DB.Comment),
                    typeof(Nimbus.DB.UserTopicFavorite)});

            }
            */
            byte x = NimbusAppBus.Settings.Cryptography.RSAParams.Exponent[0];
            return x.ToString() + "Nimbus: " + NimbusAppBus.Settings.DatabaseConnectionString;
        }
    }
}