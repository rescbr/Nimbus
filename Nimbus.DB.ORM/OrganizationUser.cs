using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB.ORM
{
    public class OrganizationUser  : Nimbus.DB.OrganizationUser
    {
        [References(typeof(User))]
        public override int UserId { get; set; }

        [References(typeof(Organization))]
        public override int OrganizationId { get; set; }
    }
}
