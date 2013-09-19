using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB
{
    public class Premium
    {
        [AutoIncrement]
        public int Id { get; set; }
        
        public string PremiunName { get; set; }
    }
}
