using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TIPServer.Call_History
{
    public sealed class CallhistoryElement
    {
        public string Email { get; }
        public List<CallParameters> Calls { get; }

        public CallhistoryElement(string email, List<CallParameters> calls)
        {
            Email = email;
            Calls = calls;
        }
    }
}
