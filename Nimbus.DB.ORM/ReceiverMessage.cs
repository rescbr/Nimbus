using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model.ORM
{
    public class ReceiverMessage : Nimbus.Model.ReceiverMessage
    {
        //GAMBIARRA POR CAUSA DO SERVICESTACK NOJENTO https://groups.google.com/forum/#!msg/servicestack/u81hFKRyFLw/htYx6BDW9ZgJ
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(User))]
        public override int UserId { get; set; }
        
        [References(typeof(Message))]
        public override int MessageId { get; set; }
    }
}
