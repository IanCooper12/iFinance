using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Group13iFinanceFix.Models
{
    public class ForgotPasswordViewModel
    {
        public string Email { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
