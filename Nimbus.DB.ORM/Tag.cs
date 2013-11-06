using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model.ORM
{
    public class Tag: Nimbus.Model.Tag
    {
        [AutoIncrement]
        public  int Id { get; set; }


    }
}
