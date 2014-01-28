using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model.Bags
{
    public class UserBag : User
    {
        public int  Age {get;set;}
        public int Interaction { get; set; }

        public string RoleInChannel { get; set; }

        public int PointsForComment { get; set; }

        public int PointsForChannel { get; set; }

        public int PontsForTopic { get; set; }

        public bool IsUserFacebook { get; set; }
    }
}
