using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model.Bags
{
    public class CommentBag:Comment
    {
        public string AvatarUrl { get; set; }
        public string UserName { get; set; }

        public string TopicName { get; set; }

        public bool IsParent { get; set; }

        public bool IsDeletable { get; set; }

        public bool IsRepotable { get; set; }

        public List<CommentBag> CommentChild { get; set; }
    }
}
