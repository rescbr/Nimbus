using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model.ORM
{
    public class UserBadge : Nimbus.Model.UserBadge
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        [References(typeof(User))]
        public int UserId { get; set; }

        [References(typeof(Badge))]
        public int BadgeId { get; set; }

    }
}
