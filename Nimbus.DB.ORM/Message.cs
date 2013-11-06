using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model.ORM
{
    public class Message : Nimbus.Model.Message
    {
        [AutoIncrement]
        public override int Id { get; set; }

        [References(typeof(User))]
        public override int SenderId { get; set; }
        
        [References(typeof(Channel))]
        public override int ChannelId { get; set; }

    }
}
