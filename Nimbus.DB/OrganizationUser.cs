using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model
{
    public class OrganizationUser
    {
        public virtual int UserId { get; set; }

        public virtual int OrganizationId { get; set; }
    }
}
