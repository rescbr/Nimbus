using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nimbus.Plumbing.Cache;

namespace Nimbus.Plumbing
{
    public class NimbusCache 
    {
        private INimbusCacheProvider _organizationHosts;
        public INimbusCacheProvider OrganizationHosts
        {
            get { return _organizationHosts; }
        }

        private INimbusCacheProvider _sessionPrincipal;
        public INimbusCacheProvider SessionPrincipal
        {
            get { return _sessionPrincipal; }
        }

        public void Initialize<T>()
            where T : INimbusCacheProvider, new()
        {
            _organizationHosts = new T();
            _sessionPrincipal = new T();
        }



    }
}
