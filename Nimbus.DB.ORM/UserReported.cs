using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model.ORM
{
    public class UserReported  : Nimbus.Model.UserReported
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(User))]
        public int UserReportedId { get; set; }

        [References(typeof(User))]
        public int UserReporterId { get; set; }

    }

}
