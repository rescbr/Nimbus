using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB
{
    public class Badge
    {
        public Badge()
        {
            LocalizedDescription = new LocalizedString();
        }

        [AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public LocalizedString LocalizedDescription { get; set; }
        public string Url { get; set; }


    }
}
