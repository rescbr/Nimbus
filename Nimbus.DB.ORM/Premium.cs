using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model.ORM
{
    public class Premium : Nimbus.Model.Premium
    {
        [AutoIncrement]
        public override int Id { get; set; }        
    }
}
