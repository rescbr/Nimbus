using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB.ORM
{
    public class Channel : Nimbus.DB.Channel
    {
        [AutoIncrement]
        public int Id { get; set; }
        
        [References(typeof(User))]
        public int OwnerId { get; set; }

        [References(typeof(Category))]
        public int CategoryId { get; set; }

        [References(typeof(Organization))]
        public int OrganizationId { get; set; }

    }
}
