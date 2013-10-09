using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB.ORM
{
    public class UserAds: Nimbus.DB.UserAds
    {
        [References(typeof(User))]
        public int UserId { get; set; }

        [References(typeof(Ad))]
        public int AdsId { get; set; }

        public int Credits { get; set; }
    }
}
