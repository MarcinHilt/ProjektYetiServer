using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TIPServer
{
    public sealed class UsersDictionary
    {
        private List<UserElement> Users { get; set; }

        public UserElement Find(string email, string password)
        {
            //test
            UserElement user = new UserElement("", "", "");
            foreach (var e in Users)
            {
                if (email == e.Email && password == e.Password)
                {
                    return e;
                }
            }
            return user;
        }

        public bool FindUsers(string email, string password)
        {

            string sHashedPassword = AuthenticationMethods.HashPassword(password);
            foreach (var e in Users)
            {
                if (email == e.Email && sHashedPassword == e.Password)
                {
                    return true;
                }
            }
            return false;
        }

        public string GetIP(string email)
        {
            foreach (var e in Users)
            {
                if (e.Email == email)
                {
                    return e.IPAdress;
                }
            }
            return "nope";
        }

        public string GetEmail(string ip)
        {
            foreach (var e in Users)
            {
                if (e.IPAdress == ip)
                {
                    return e.Email;
                }
            }
            return "nope";
        }
        public void RegisterUser(string email, string password, string clientIP)
        {
            string sHashedPassword = AuthenticationMethods.HashPassword(password);
            Users.Add(new UserElement(email, sHashedPassword, clientIP));
            File.WriteAllText(@"Data/users.json", Serialize());
        }

        public bool SetIP(string email, string ipAddress)
        {
            for (int i = 0; i < Users.Count(); i++)
            {
                if (Users[i].Email == email && Users[i].IPAdress != ipAddress)
                {
                    Users[i].IPAdress = ipAddress;
                    File.WriteAllText(@"Data/users.json", Serialize());
                    return true;
                }
            }
            return false;
        }

        public UsersDictionary(string json)
        {
            Users = JsonConvert.DeserializeObject<List<UserElement>>(json);
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(Users);
        }
    }
}