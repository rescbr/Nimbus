using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model.ORM
{
    public class UserInfoPayment: Nimbus.Model.UserInfoPayment
    {
        [References(typeof(User))]
        public int UserId { get; set; }
    }
}
