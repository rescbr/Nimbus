using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB.ORM
{
    public class Premium : Nimbus.DB.Premium
    {
        [AutoIncrement]
        public override int Id { get; set; }        
    }
}
