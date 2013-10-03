using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB.ORM
{
    public class TagChannel : Nimbus.DB.TagChannel
    {
       [References(typeof(TagChannel))]
        public int TagId { get; set; }

        [References(typeof(Channel))]
        public int ChannelId { get; set; }

    }
}
