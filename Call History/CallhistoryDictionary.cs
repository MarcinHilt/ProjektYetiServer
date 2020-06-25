using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace TIPServer.Call_History
{
    public sealed class CallhistoryDictionary
    {
        private List<CallhistoryElement> AllCalls { get; }

        public CallhistoryDictionary(string json)
        {
            AllCalls = JsonConvert.DeserializeObject<List<CallhistoryElement>>(json);
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(AllCalls);
        }


        public string GetSendableString(string email, string id)
        {
            string sendablestring = "";
            foreach (var call in AllCalls)
            {
                if (call.Email == email)
                {
                    foreach (var a in call.Calls)
                    {
                        if (a.Id == id)
                        {
                            return sendablestring = a.Id + "&" + a.Data + "&" + a.Email + "&" + a.Time;
                        }
                    }
                    return "";
                }
            }
            return "";
        }

        public string GetNumberOfCalls(string email)
        {
            try
            {
                foreach (var call in AllCalls)
                {
                    if (call.Email == email)
                    {
                        return call.Calls.Count.ToString();
                    }
                }
            }
            catch
            { return "0"; }


            return "0";
        }

        public void AddCall(string email, string id, string data, string email2, string time)
        {
            CallParameters callParameters = new CallParameters(id, data, email2, time);
            foreach (var call in AllCalls)
            {
                if (call.Email == email)
                {
                    call.Calls.Add(callParameters);
                    File.WriteAllText(@"Data/callhistory.json", Serialize());
                }
            }
        }
        public void RemoveCallhistory(string email)
        {
            foreach (var call in AllCalls)
            {
                if (call.Email == email)
                {
                    call.Calls.Clear();
                    File.WriteAllText(@"Data/callhistory.json", Serialize());
                }
            }
        }
        public bool CallhistoryExist(string mail)
        {
            foreach (var call in AllCalls)
            {
                if (call.Email == mail)
                    return true;
            }
            return false;
        }
        public void CreateHistory(string email)
        {
            try
            {
                CallhistoryElement callhistoryElement = new CallhistoryElement(email, new List<CallParameters>());
                AllCalls.Add(callhistoryElement);
                File.WriteAllText(@"Data/callhistory.json", Serialize());
            }
            catch
            {

            }
        }
    }
}
