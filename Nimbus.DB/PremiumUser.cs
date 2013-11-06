using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model
{
    public class PremiumUser
    {
        public virtual int PremiumId { get; set; }

        public virtual int UserId { get; set; }


        public bool Available { get; set; }
        public DateTime PaidOn { get; set; }
        public DateTime ExpireIn { get; set; }

    }
}
