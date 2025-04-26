using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Group13iFinanceFix.Models
{
    //ViewModel for Register form to collect user info and create user(s)
    public class RegisterViewModel
    {
        [Required]
        public string ID { get; set; }

        public string UsersName { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public bool IsAdmin { get; set; }

        public string StreetAddress { get; set; }
        public string Email { get; set; }
    }
}
