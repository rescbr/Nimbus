using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB.ORM
{
    public class ReceiverMessage : Nimbus.DB.ReceiverMessage
    {
        [References(typeof(User))]
        public override int UserId { get; set; }

        [References(typeof(Message))]
        public override int MessageId { get; set; }
    }
}
