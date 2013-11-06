using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model
{
    public class Channel
    {
        public int Id { get; set; }
        
        public int OwnerId { get; set; }

        public int CategoryId { get; set; }

        public int OrganizationId { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string ImgUrl { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime LastModification { get; set; }
        public bool OpenToComments { get; set; }
        public bool IsPrivate { get; set; }
        public bool Visible { get; set; }
        public bool IsCourse { get; set; }
        public decimal Price { get; set; }
        public int Followers { get; set; }


    }
}
