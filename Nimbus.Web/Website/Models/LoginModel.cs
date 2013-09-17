using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nimbus.Web.Website.Models
{
    public class LoginModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string RedirectURL { get; set; }
        public string ErrorMessage { get; set; }
    }
}