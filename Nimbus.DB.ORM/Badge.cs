using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB.ORM
{
    public class Badge : Nimbus.DB.Badge
    {
        [AutoIncrement]
        public int Id { get; set; }
    }
}
