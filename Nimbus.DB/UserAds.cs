using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB
{
    public class UserAds
    {
        public int UserId { get; set; }
        public int AdsId { get; set; }
        public bool IsPaid { get; set; }
        public DateTime? DatePayment { get; set; }
        public int Credits { get; set; }
        public int CountLeft { get; set; }
        public int CountDay { get; set; }
        public int CountClick { get; set; }

    }
}
