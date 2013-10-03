using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB.ORM
{
    public class Tag: Nimbus.DB.Tag
    {
        [AutoIncrement]
        public  int Id { get; set; }


    }
}
