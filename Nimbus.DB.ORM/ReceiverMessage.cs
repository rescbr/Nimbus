using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model.ORM
{
    public class ReceiverMessage : Nimbus.Model.ReceiverMessage
    {
        [PrimaryKey]
        [References(typeof(User))]
        public override int UserId { get; set; }
        
        [PrimaryKey]
        [References(typeof(Message))]
        public override int MessageId { get; set; }
    }
}
