using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model
{
    public class UserChannelReadLater
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int ChannelId { get; set; }

        public DateTime Date { get; set; }
        public DateTime? ReadOn { get; set; }
        public bool Visible { get; set; }
    }
}
