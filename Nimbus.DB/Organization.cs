using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB
{
    public class Organization
    {
        [AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string LogoUrl { get; set; }
        public string Cname { get; set; }

    }
}
