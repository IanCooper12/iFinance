using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Group13iFinanceFix.Models
{
    //ViewModel for Register form to collect user info and create user(s)
    public class RegisterViewModel
    {
        public string ID { get; set; }
        public string UsersName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool IsAdmin { get; set; }
        public string StreetAddress { get; set; }
        public string Email { get; set; }
    }
}
