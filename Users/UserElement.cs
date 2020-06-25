using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TIPServer
{
    public sealed class UserElement
    {
        public string Email { get; }
        public string Password { get; }
        public string IPAdress { get; set; }

        public UserElement(string email, string password, string ipadress)
        {
          //init commit
            Email = email;
            Password = password;
            IPAdress = ipadress;
        }
    }
}
