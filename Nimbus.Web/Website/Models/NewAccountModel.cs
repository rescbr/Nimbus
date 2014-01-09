using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nimbus.Web.Website.Models
{
    public class NewAccountModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}