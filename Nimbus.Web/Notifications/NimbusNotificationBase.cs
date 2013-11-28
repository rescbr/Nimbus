using Microsoft.AspNet.SignalR;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nimbus.Web.Notifications
{
    public class NimbusNotificationBase
    {
        private readonly static IHubContext _nimbusHubCtx = GlobalHost.ConnectionManager.GetHubContext<NimbusHub>();
        public IHubContext NimbusHubContext
        {
            get { return NimbusNotificationBase._nimbusHubCtx; }
        }

        /// <summary>
        /// For test purposes only!
        /// </summary>
        private IDbConnectionFactory _databaseFactory = null;
        /// <summary>
        /// Obtém a DatabaseFactory a partir das configurações.
        /// </summary>
        public IDbConnectionFactory DatabaseFactory
        {
            get
            {
                if (_databaseFactory != null)
                    return _databaseFactory;
                else
                    return new OrmLiteConnectionFactory
                    (NimbusConfig.DatabaseConnection,
                    SqlServerDialect.Provider);
            }
            set
            {
                _databaseFactory = value;
            }
        }
    }
}