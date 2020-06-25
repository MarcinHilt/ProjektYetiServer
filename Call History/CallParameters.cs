using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TIPServer.Call_History
{
    public sealed class CallParameters
    {
        public string Id { get; }
        public string Data { get; }
        public string Email { get; }
        public string Time { get; }

        public CallParameters(string id, string data, string email, string time)
        {
            Id = id;
            Data = data;
            Email = email;
            Time = time;
        }
    }
}
