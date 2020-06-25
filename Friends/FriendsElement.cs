using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TIPServer
{
    public sealed class FriendsElement
    {
        public string Email { get; }
        public List<string> FriendList { get; }

        public FriendsElement(string email, List<string> friendList)
        {
            Email = email;
            FriendList = friendList;
        }
    }
}
