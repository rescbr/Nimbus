using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.Model
{
    public class UserInfoPayment
    {
        public int UserId { get; set; }
        public string CPF { get; set; }
        public string CNPJ { get; set; }
        public DateTime RegisterDate { get; set; }
        public bool Visible { get; set; }
    }
}
