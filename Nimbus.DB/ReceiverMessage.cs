using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB
{
    public class ReceiverMessage
    {
        [References(typeof(User))]
        public int UserID { get; set; }

        [References(typeof(Message))]
        public int MessageID { get; set; }

        public bool  IsOwner { get; set; }
    }
}
