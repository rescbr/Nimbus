using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model
{
    public class ReceiverMessage
    {
        public virtual int UserId { get; set; }

        public virtual int MessageId { get; set; }


        public bool  IsOwner { get; set; }

        public string NameUser { get; set; }

        public bool UserReadStatus { get; set; }

        public Enums.MessageType Status { get; set; }
    }
}
