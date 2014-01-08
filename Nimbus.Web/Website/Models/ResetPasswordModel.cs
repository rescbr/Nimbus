using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Nimbus.Web.Website.Models
{
    public class ResetPasswordModel
    {
        public string NewPassord { get; set; }

        public string ConfirmPassword { get; set; }

        public string Token { get; set; }
    }
}