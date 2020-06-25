using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace TIPServer.Friends
{
    public sealed class FriendsDictionary
    {
        private List<FriendsElement> Friends { get; }

        public FriendsDictionary(string json)
        {
            Friends = JsonConvert.DeserializeObject<List<FriendsElement>>(json);
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(Friends);
        }

        public bool Exist(string email)
        {
            foreach (var a in Friends)
            {
                if (a.Email == email)
                {
                    return true;
                }
            }
            Friends.Add(new FriendsElement(email, new List<string>()));
            File.WriteAllText(@"Data/friends.json", Serialize());
            return false;
        }

        public List<string> getFriends(string email)
        {
            if (Exist(email))
            {
                foreach (var a in Friends)
                {
                    if (a.Email == email)
                    {
                        return a.FriendList;
                    }
                }
                return new List<string>();
            }
            else
            {
                return new List<string>();
            }
        }

        public bool addFriend(string email, string friend)
        {
            foreach (var a in Friends)
            {
                if (a.Email == email)
                {
                    try
                    {
                        foreach (string b in a.FriendList)
                        {
                            if (b == friend)
                            {
                                return false;
                            }
                        }
                    }
                    catch
                    {
                        a.FriendList.Add(friend);
                        File.WriteAllText(@"Data/friends.json", Serialize());
                        return true;
                    }
                    a.FriendList.Add(friend);
                    File.WriteAllText(@"Data/friends.json", Serialize());
                    return true;
                }
            }
            return false;
        }

        public bool removeFriend(string email, string friend)
        {
            foreach (var a in Friends)
            {
                if (a.Email == email)
                {
                    try
                    {
                        foreach (string b in a.FriendList)
                        {
                            if (b == friend)
                            {
                                a.FriendList.Remove(b);
                                File.WriteAllText(@"Data/friends.json", Serialize());
                                return true;
                            }
                        }
                    }
                    catch
                    {
                        return false;
                    }
                    return false;
                }
            }
            return false;
        }

    }
}
