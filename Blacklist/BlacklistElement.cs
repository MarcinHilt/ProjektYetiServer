using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TIPServer
{
    public sealed class BlacklistElement
    {
        public string Email { get; }
        public List<string> BlacklistList { get; }

        public BlacklistElement(string email, List<string> blacklistList)
        {
            Email = email;
            BlacklistList = blacklistList;
        }
    }
}