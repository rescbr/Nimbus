using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB
{
    public class Channel
    {
        [AutoIncrement]
        public int Id { get; set; }
        
        [References(typeof(User))]
        public int OwnerId { get; set; }

        [References(typeof(Category))]
        public int CategoryId { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool OpenToComments { get; set; }
        public bool Visible { get; set; }
        public bool IsCourse { get; set; }
        public int Ranking { get; set; }
        public int Followers { get; set; }


    }
}
