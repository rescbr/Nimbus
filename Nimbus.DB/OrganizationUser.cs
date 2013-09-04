using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB
{
    public class OrganizationUser
    {
        [References(typeof(User))]
        public int UserId { get; set; }

        [References(typeof(Organization))]
        public int OrganizationId { get; set; }

        //public Role Role { get; set; }
    }
}
