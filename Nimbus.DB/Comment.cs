using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB
{
    public class Comment
    {
        public int Id { get; set; }

        public int ParentId { get; set; }

        public int UserId { get; set; }

        public int ChannelId { get; set; }

        public int TopicId { get; set; }

        public DateTime PostedOn { get; set; }
        public bool Visible { get; set; }
        public string Text { get; set; }

       
    }
}
