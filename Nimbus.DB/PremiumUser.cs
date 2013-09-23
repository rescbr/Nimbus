using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB
{
    public class PremiumUser
    {
        [References(typeof(Premium))]
        public int PremiumID { get; set; }

        [References(typeof(User))]
        public int UserID { get; set; }

        public bool Available { get; set; }
        public DateTime PaidOn { get; set; }
        public DateTime ExpireIn { get; set; }

    }
}
