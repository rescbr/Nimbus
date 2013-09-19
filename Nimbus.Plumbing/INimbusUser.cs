using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Plumbing
{
    public class NimbusUser : IIdentity
    {
        //IIdentity
        public string AuthenticationType { get { return "NimbusUser"; } }

        public bool IsAuthenticated { get; set; }
        public string Name { get; set; }
        public int UserId { get; set; }
        public string AvatarUrl { get; set; }
    }

    public class NimbusPrincipal : IPrincipal
    {
        public IIdentity Identity { get; set; }

        public bool IsInRole(string role)
        {
            return false;
        }

        public NimbusPrincipal(NimbusUser identity)
        {
            Identity = identity;
        }
    }
}
