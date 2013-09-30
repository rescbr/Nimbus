using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nimbus.DB
{
    public class User
    {
        public virtual int Id { get; set; }


        public string Email { get; set; }
        public string Password { get; set; }
        public string TOTPKey { get; set; }
        
        public DateTime BirthDate { get; set; }
        public string Occupation { get; set; }
        public string Interest { get; set; }
        public string Experience { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string AvatarUrl { get; set; }
        public string About { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
    }
}
