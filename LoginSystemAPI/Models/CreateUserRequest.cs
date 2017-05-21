using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CustomLoginSystem.Models
{
    public class CreateUserRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}