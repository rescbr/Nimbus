using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model
{
    public class UserTopicReadLater
    {
        public virtual int Id { get; set; }

        public virtual int UserId { get; set; }

        public virtual int TopicId { get; set; }


        public bool Visible { get; set; }
        public DateTime? ReadOn { get; set; }
    }
}
