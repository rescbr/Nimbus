using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model.ORM
{
    public class TagChannel : Nimbus.Model.TagChannel
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        
        [References(typeof(Tag))]
        public override int TagId { get; set; }
        
        [References(typeof(Channel))]
        public override int ChannelId { get; set; }

    }
}
