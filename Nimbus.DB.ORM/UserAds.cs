using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model.ORM
{
    public class UserAds: Nimbus.Model.UserAds
    {
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(User))]
        public int UserId { get; set; }

        [References(typeof(Ad))]
        public int AdsId { get; set; }

        public int Credits { get; set; }
    }
}
