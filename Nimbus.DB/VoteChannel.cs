using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB
{
    public class VoteChannel
    {
        [References(typeof(Channel))]
        public int Channel_ID { get; set; }

        public int Score { get; set; }
    }
}
