using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model.ORM
{
    public class Prices : Nimbus.Model.Prices
    {
        [References(typeof(Prices))]
        public int Id { get; set; }
    }
}
