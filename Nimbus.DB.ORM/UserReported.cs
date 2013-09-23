using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB.ORM
{
    public class UserReported  : Nimbus.DB.UserReported
    {
        [AutoIncrement]
        public override int Id { get; set; }

        [References(typeof(User))]
        public override int UserReportedId { get; set; }

        [References(typeof(User))]
        public override int UserReporterId { get; set; }

    }

}
