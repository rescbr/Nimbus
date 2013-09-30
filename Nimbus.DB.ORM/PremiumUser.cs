using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB.ORM
{
    public class PremiumUser : Nimbus.DB.PremiumUser
    {
        [References(typeof(Premium))]
        public override int PremiumId { get; set; }

        [References(typeof(User))]
        public override int UserId { get; set; }
        
    }
}
