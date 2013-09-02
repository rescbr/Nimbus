using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB
{
    public class UserChannelReadLater
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(User))]
        public int UserId { get; set; }

        [References(typeof(Channel))]
        public int ChannelId { get; set; }

        public DateTime Date { get; set; }
        public DateTime? ReadOn { get; set; }
        public bool Visible { get; set; }
    }
}
