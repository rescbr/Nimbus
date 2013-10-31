using Microsoft.AspNet.SignalR;
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
    }
}