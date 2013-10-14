using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB.ORM
{
    public class Log_User: Nimbus.DB.Log_User
    {
        [AutoIncrement]
        public override int Id { get; set; }

        [References(typeof(User))]
        public override int UserId { get; set; }


    }
}
