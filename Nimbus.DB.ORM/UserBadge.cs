using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB.ORM
{
    public class UserBadge : Nimbus.DB.UserBadge
    {
        [References(typeof(User))]
        public int UserId { get; set; }

        [References(typeof(Badge))]
        public int BadgeId { get; set; }

    }
}
