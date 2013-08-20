using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB
{
    //TODO: Terminar
    public class Ad
    {
        [AutoIncrement]
        public int Id { get; set; }
        public string Url { get; set; }
    }
}
