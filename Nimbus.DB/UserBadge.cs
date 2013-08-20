using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB
{
    public class UserBadge
    {
        [References(typeof(User))]
        public int UserId { get; set; }

        [References(typeof(Badge))]
        public int BadgeId { get; set; }

        public DateTime ReceivedOn { get; set; }


    }
}
