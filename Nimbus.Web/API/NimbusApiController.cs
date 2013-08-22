using Nimbus.Plumbing.Interface;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace Nimbus.Web.API
{
    public class NimbusApiController : ApiController
    {
        public INimbusAppBus NimbusAppBus
        {
            get
            {
                return base.Configuration.Properties["NimbusAppBus"] as INimbusAppBus;
            }
        }

        public NimbusUser NimbusUser
        {
            get
            {
                if (User.Identity.AuthenticationType == "NimbusUser")
                {
                    return ((User.Identity) as NimbusUser);
                }
                else throw new Exception("User.Identity.AuthenticationType is not NimbusUser.");
            }
        }

        public IDbConnectionFactory DatabaseFactory
        {
            get
            {
                return new OrmLiteConnectionFactory
                    (NimbusAppBus.Settings.DatabaseConnectionString,
                    SqlServerDialect.Provider);
            }
        }
        
    }
}