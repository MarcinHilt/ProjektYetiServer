using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace TIPServer.Friends
{
    public sealed class BlacklistDictionary
    {
        private List<BlacklistElement> Blacklist { get; }

        public BlacklistDictionary(string json)
        {
            Blacklist = JsonConvert.DeserializeObject<List<BlacklistElement>>(json);
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(Blacklist);
        }

        public bool Exist(string email)
        {
            foreach (var a in Blacklist)
            {
                if (a.Email == email)
                {
                    return true;
                }
            }
            Blacklist.Add(new BlacklistElement(email, new List<string>()));
            File.WriteAllText(@"Data/blacklist.json", Serialize());
            return false;
        }

        public List<string> getBlacklist(string email)
        {
            if (Exist(email))
            {
                foreach (var a in Blacklist)
                {
                    if (a.Email == email)
                    {
                        return a.BlacklistList;
                    }
                }
                return new List<string>();
            }
            else
            {
                return new List<string>();
            }
        }

        public bool addBlacklist(string email, string blacklist)
        {
            foreach (var a in Blacklist)
            {
                try
                {
                    if (a.Email == email)
                    {
                        foreach (string b in a.BlacklistList)
                        {
                            if (b == blacklist)
                            {
                                return false;
                            }
                        }
                        a.BlacklistList.Add(blacklist);
                        File.WriteAllText(@"Data/blacklist.json", Serialize());
                        return true;
                    }
                }
                catch
                {
                    a.BlacklistList.Add(blacklist);
                    File.WriteAllText(@"Data/blacklist.json", Serialize());
                    return true;
                }
            }
            return false;
        }

        public bool removeBlacklist(string email, string blacklist)
        {
            foreach (var a in Blacklist)
            {
                try
                {
                    if (a.Email == email)
                    {
                        foreach (string b in a.BlacklistList)
                        {
                            if (b == blacklist)
                            {
                                a.BlacklistList.Remove(b);
                                File.WriteAllText(@"Data/blacklist.json", Serialize());
                                return true;
                            }
                        }
                        return false;
                    }
                }
                catch
                {
                    return false;
                }

            }
            return false;
        }

    }
}
