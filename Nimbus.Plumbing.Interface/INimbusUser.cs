using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Plumbing.Interface
{
    public class NimbusUser : IIdentity
    {
        //IIdentity
        public string AuthenticationType { get { return "NimbusUser"; } }

        private bool _isAuthenticated;
        public bool IsAuthenticated
        {
            get { return _isAuthenticated; }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
        }


        private int _userId;
        public int UserId
        {
            get { return _userId; }
        }

        private string _avatarUrl;
        public string AvatarUrl
        {
            get { return _avatarUrl; }
        }


    }
}
