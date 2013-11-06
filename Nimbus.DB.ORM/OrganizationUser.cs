using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model.ORM
{
    public class OrganizationUser  : Nimbus.Model.OrganizationUser
    {
        [References(typeof(User))]
        public override int UserId { get; set; }

        [References(typeof(Organization))]
        public override int OrganizationId { get; set; }
    }
}
