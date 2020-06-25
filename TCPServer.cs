using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using TIPServer.TIPPacket;
using Newtonsoft.Json.Linq;
using TIPServer.Friends;
using System.Collections.Generic;
using TIPServer.Call_History;

namespace TIPServer
{
    public sealed class TCPServer
    {
        private readonly TcpListener TCPListener;
        public UsersDictionary usersDictionary;
        public FriendsDictionary friendsDictionary;
        public BlacklistDictionary blacklistDictionary;
        public CallhistoryDictionary callhistoryDictionary;

        public TCPServer(int portNumber)
        {
            TCPListener = new TcpListener(IPAddress.Any, portNumber);
            Thread usersLoader = new Thread(() =>
            {
                string json = File.ReadAllText(@"Data/users.json");
                usersDictionary = new UsersDictionary(json);
                Console.WriteLine("Dane uzytkownikow zaladowane.");
                string json2 = File.ReadAllText(@"Data/friends.json");
                friendsDictionary = new FriendsDictionary(json2);
                string json3 = File.ReadAllText(@"Data/blacklist.json");
                blacklistDictionary = new BlacklistDictionary(json3);
                string json4 = File.ReadAllText(@"Data/callhistory.json");
                callhistoryDictionary = new CallhistoryDictionary(json4);
            });

            usersLoader.Start();
            usersLoader.Join();
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            string hostIP = "";
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    hostIP += ip.ToString() + "\n";
                }

            }
            return hostIP;
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }

        public void AcceptClients()
        {
            TCPListener.Start();

            Console.WriteLine("Server wystartowal.\n IP =" + GetLocalIPAddress());
            try
            {
                for (; ; )
                {
                    TcpClient tcpClient = TCPListener.AcceptTcpClient();

                    Thread clientThread = new Thread(() => {
                        CommunicateWithClient(tcpClient);
                    });

                    clientThread.Start();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void CommunicateWithClient(TcpClient tcpClient)
        {
            bool endConnection = false;
            Packet message;

            Console.WriteLine("Client polaczyl sie.");

            NetworkStream networkStream = tcpClient.GetStream();
            string clientIP = tcpClient.Client.LocalEndPoint.ToString();
            try
            {
                for (; ; )
                {
                    if (endConnection)
                    {
                        break;
                    }

                    message = new Packet(networkStream);

                    AnalyzeMessage(ref endConnection, message, networkStream);
                }

                Console.WriteLine("Client zakonczyl polaczenie.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        private void AnalyzeMessage(ref bool endConnection, Packet message, NetworkStream networkStream)
        {
            switch (message.Command)
            {
                case Command.EndConnection:
                    endConnection = true;
                    ReplyMessage(Command.EndConnectionAck, message.Identifier, new byte[] { }, networkStream);
                    break;
                case Command.LogInRequest:
                    string Email = "", Password = "", clientIP = "";
                    int help = 0;
                    foreach (char character in message.Data)
                    {
                        if (character != '&' && help == 0)
                        {
                            Email += character;
                        }
                        else if (character == '&')
                        {
                            help++;
                        }
                        else if (character != '&' && help == 1)
                        {
                            Password += character;
                        }
                        else if (character != '&' && help == 2)
                        {
                            clientIP += character;
                        }
                    }

                    if (usersDictionary.FindUsers(Email, Password) == true)
                    {
                        if (usersDictionary.SetIP(Email, clientIP))
                        {
                            usersDictionary = new UsersDictionary(File.ReadAllText(@"Data/users.json"));
                        }
                        ReplyMessage(Command.LogInAccepted, message.Identifier, new byte[] { }, networkStream);
                    }
                    else
                    {
                        ReplyMessage(Command.LogInInvalidCredentials, message.Identifier, new byte[] { }, networkStream);

                    }
                    break;
                case Command.GetIPRequest:
                    string myMail = "";
                    string userMail = "";
                    help = 0;
                    foreach (char character in message.Data)
                    {
                        if (character != '&' && help == 0)
                        {
                            myMail += character;
                        }
                        else if (character == '&')
                        {
                            help++;
                        }
                        else if (character != '&' && help == 1)
                        {
                            userMail += character;
                        }
                    }
                    try
                    {
                        foreach (string mail in blacklistDictionary.getBlacklist(userMail))
                        {
                            if (mail == myMail)
                            {
                                ReplyMessage(Command.Blacklisted, message.Identifier, new byte[] { }, networkStream);
                            }
                        }
                    }
                    catch { }

                    string ip = usersDictionary.GetIP(userMail);
                    if (ip != "nope")
                    {
                        byte[] bytes = Encoding.ASCII.GetBytes(ip);
                        ReplyMessage(Command.IPSent, message.Identifier, bytes, networkStream);
                    }
                    else
                    {
                        ReplyMessage(Command.GetIPRequestDenied, message.Identifier, new byte[] { }, networkStream);
                    }
                    break;
                case Command.RegisterRequest:
                    Email = "";
                    Password = "";
                    clientIP = "";
                    help = 0;
                    foreach (char character in message.Data)
                    {
                        if (character != '&' && help == 0)
                        {
                            Email += character;
                        }
                        else if (character == '&')
                        {
                            help++;
                        }
                        else if (character != '&' && help == 1)
                        {
                            Password += character;
                        }
                        else if (character != '&' && help == 2)
                        {
                            clientIP += character;
                        }
                    }
                    if (usersDictionary.FindUsers(Email, Password) == true)
                    {
                        ReplyMessage(Command.RegisterRequestDenied, message.Identifier, new byte[] { }, networkStream);
                    }
                    else
                    {
                        usersDictionary.RegisterUser(Email, Password, clientIP);
                        usersDictionary = new UsersDictionary(File.ReadAllText(@"Data/users.json"));
                        ReplyMessage(Command.RegisterRequestAccepted, message.Identifier, new byte[] { }, networkStream);
                    }
                    break;
                case Command.GetFriendsListRequest:
                    Email = "";
                    help = 0;
                    foreach (char character in message.Data)
                    {
                        if (character != '&' && help == 0)
                        {
                            Email += character;
                        }
                    }
                    byte[] data;
                    try
                    {
                        data = Encoding.ASCII.GetBytes(friendsDictionary.getFriends(Email).Count.ToString());
                    }
                    catch
                    {
                        data = Encoding.ASCII.GetBytes("0");
                    }

                    ReplyMessage(Command.FriendsListCountSent, message.Identifier, data, networkStream);
                    break;
                case Command.GetFriendsListFirstPart:
                    Email = "";
                    string firstPart = "";
                    help = 0;
                    List<string> friends;
                    foreach (char character in message.Data)
                    {
                        if (character != '&' && help == 0)
                        {
                            Email += character;
                        }
                    }
                    friends = friendsDictionary.getFriends(Email);
                    for (int i = 0; i < 10; i++)
                    {
                        firstPart += friends[i] + "&";
                    }
                    data = Encoding.ASCII.GetBytes(firstPart);
                    ReplyMessage(Command.FriendsListFirstPartSent, message.Identifier, data, networkStream);
                    break;
                case Command.GetFriendsListSecondPart:
                    Email = "";
                    string secondPart = "";
                    help = 0;
                    foreach (char character in message.Data)
                    {
                        if (character != '&' && help == 0)
                        {
                            Email += character;
                        }
                    }
                    friends = friendsDictionary.getFriends(Email);
                    for (int i = 10; i < 20; i++)
                    {
                        secondPart += friends[i] + "&";
                    }
                    data = Encoding.ASCII.GetBytes(secondPart);
                    ReplyMessage(Command.FriendsListSecondPartSent, message.Identifier, data, networkStream);
                    break;
                case Command.GetFriendsListLastPart:
                    Email = "";
                    string friendsLeft = "";
                    string friendsStart = "";
                    string lastPart = "";
                    help = 0;
                    foreach (char character in message.Data)
                    {
                        if (character != '&' && help == 0)
                        {
                            Email += character;
                        }
                        else if (character == '&')
                        {
                            help++;
                        }
                        else if (character != '&' && help == 1)
                        {
                            friendsStart += character;
                        }
                        else if (character != '&' && help == 2)
                        {
                            friendsLeft += character;
                        }
                    }
                    friends = friendsDictionary.getFriends(Email);
                    for (int i = int.Parse(friendsStart); i < int.Parse(friendsLeft); i++)
                    {
                        lastPart += friends[i] + "&";
                    }
                    data = Encoding.ASCII.GetBytes(lastPart);
                    ReplyMessage(Command.FriendsListLastPartSent, message.Identifier, data, networkStream);
                    break;
                case Command.AddFriendRequest:
                    Email = "";
                    string friendEmail = "";
                    help = 0;
                    foreach (char character in message.Data)
                    {
                        if (character != '&' && help == 0)
                        {
                            Email += character;
                        }
                        else if (character == '&')
                        {
                            help++;
                        }
                        else if (character != '&' && help == 1)
                        {
                            friendEmail += character;
                        }
                    }
                    if (friendsDictionary.addFriend(Email, friendEmail))
                    {
                        ReplyMessage(Command.AddFriendAccepted, message.Identifier, new byte[] { }, networkStream);
                    }
                    else
                    {
                        ReplyMessage(Command.AddFriendDenied, message.Identifier, new byte[] { }, networkStream);
                    }
                    break;
                case Command.RemoveFriendRequest:
                    Email = "";
                    friendEmail = "";
                    help = 0;
                    foreach (char character in message.Data)
                    {
                        if (character != '&' && help == 0)
                        {
                            Email += character;
                        }
                        else if (character == '&')
                        {
                            help++;
                        }
                        else if (character != '&' && help == 1)
                        {
                            friendEmail += character;
                        }
                    }
                    if (friendsDictionary.removeFriend(Email, friendEmail))
                    {
                        ReplyMessage(Command.RemoveFriendAccepted, message.Identifier, new byte[] { }, networkStream);
                    }
                    else
                    {
                        ReplyMessage(Command.RemoveFriendDenied, message.Identifier, new byte[] { }, networkStream);
                    }
                    break;
                case Command.GetBlacklistListRequest:
                    Email = "";
                    help = 0;
                    foreach (char character in message.Data)
                    {
                        if (character != '&' && help == 0)
                        {
                            Email += character;
                        }
                    }
                    try
                    {
                        data = Encoding.ASCII.GetBytes(blacklistDictionary.getBlacklist(Email).Count.ToString());
                    }
                    catch
                    {
                        data = Encoding.ASCII.GetBytes("0");
                    }

                    ReplyMessage(Command.BlacklistListCountSent, message.Identifier, data, networkStream);
                    break;
                case Command.GetBlacklistListFirstPart:
                    Email = "";
                    firstPart = "";
                    help = 0;
                    List<string> blacklist;
                    foreach (char character in message.Data)
                    {
                        if (character != '&' && help == 0)
                        {
                            Email += character;
                        }
                    }
                    blacklist = blacklistDictionary.getBlacklist(Email);
                    for (int i = 0; i < 10; i++)
                    {
                        firstPart += blacklist[i] + "&";
                    }
                    data = Encoding.ASCII.GetBytes(firstPart);
                    ReplyMessage(Command.BlacklistListFirstPartSent, message.Identifier, data, networkStream);
                    break;
                case Command.GetBlacklistListSecondPart:
                    Email = "";
                    secondPart = "";
                    help = 0;
                    foreach (char character in message.Data)
                    {
                        if (character != '&' && help == 0)
                        {
                            Email += character;
                        }
                    }
                    blacklist = blacklistDictionary.getBlacklist(Email);
                    for (int i = 10; i < 20; i++)
                    {
                        secondPart += blacklist[i] + "&";
                    }
                    data = Encoding.ASCII.GetBytes(secondPart);
                    ReplyMessage(Command.BlacklistListSecondPartSent, message.Identifier, data, networkStream);
                    break;
                case Command.GetBlacklistListLastPart:
                    Email = "";
                    friendsLeft = "";
                    friendsStart = "";
                    lastPart = "";
                    help = 0;
                    foreach (char character in message.Data)
                    {
                        if (character != '&' && help == 0)
                        {
                            Email += character;
                        }
                        else if (character == '&')
                        {
                            help++;
                        }
                        else if (character != '&' && help == 1)
                        {
                            friendsStart += character;
                        }
                        else if (character != '&' && help == 2)
                        {
                            friendsLeft += character;
                        }
                    }
                    blacklist = blacklistDictionary.getBlacklist(Email);
                    for (int i = int.Parse(friendsStart); i < int.Parse(friendsLeft); i++)
                    {
                        lastPart += blacklist[i] + "&";
                    }
                    data = Encoding.ASCII.GetBytes(lastPart);
                    ReplyMessage(Command.BlacklistListLastPartSent, message.Identifier, data, networkStream);
                    break;
                case Command.AddBlacklistRequest:
                    Email = "";
                    friendEmail = "";
                    help = 0;
                    foreach (char character in message.Data)
                    {
                        if (character != '&' && help == 0)
                        {
                            Email += character;
                        }
                        else if (character == '&')
                        {
                            help++;
                        }
                        else if (character != '&' && help == 1)
                        {
                            friendEmail += character;
                        }
                    }
                    if (blacklistDictionary.addBlacklist(Email, friendEmail))
                    {
                        ReplyMessage(Command.AddBlacklistAccepted, message.Identifier, new byte[] { }, networkStream);
                    }
                    else
                    {
                        ReplyMessage(Command.AddBlacklistDenied, message.Identifier, new byte[] { }, networkStream);
                    }
                    break;
                case Command.RemoveBlacklistRequest:
                    Email = "";
                    friendEmail = "";
                    help = 0;
                    foreach (char character in message.Data)
                    {
                        if (character != '&' && help == 0)
                        {
                            Email += character;
                        }
                        else if (character == '&')
                        {
                            help++;
                        }
                        else if (character != '&' && help == 1)
                        {
                            friendEmail += character;
                        }
                    }
                    if (blacklistDictionary.removeBlacklist(Email, friendEmail))
                    {
                        ReplyMessage(Command.RemoveBlacklistAccepted, message.Identifier, new byte[] { }, networkStream);
                    }
                    else
                    {
                        ReplyMessage(Command.RemoveBlacklistDenied, message.Identifier, new byte[] { }, networkStream);
                    }
                    break;
                case Command.GetNumberOfCallsRequest:
                    Email = "";
                    help = 0;
                    foreach (char character in message.Data)
                    {
                        if (character != '&' && help == 0)
                        {
                            Email += character;
                        }
                    }
                    string numberofcalls = callhistoryDictionary.GetNumberOfCalls(Email);
                    if (numberofcalls != "0")
                    {
                        data = Encoding.ASCII.GetBytes(numberofcalls);
                        ReplyMessage(Command.GetNumberOfCallsAccepted, message.Identifier, data, networkStream);
                    }
                    else
                    {
                        data = Encoding.ASCII.GetBytes(numberofcalls);
                        if (!callhistoryDictionary.CallhistoryExist(Email))
                        {
                            callhistoryDictionary.CreateHistory(Email);
                        }
                        ReplyMessage(Command.GetNumberOfCallsAccepted, message.Identifier, data, networkStream);
                    }
                    break;
                case Command.GetCallInfoRequest:
                    Email = "";
                    string id = "";
                    help = 0;
                    foreach (char character in message.Data)
                    {
                        if (character != '&' && help == 0)
                        {
                            Email += character;
                        }
                        else if (character == '&')
                        {
                            help++;
                        }
                        else if (character != '&' && help == 1)
                        {
                            id += character;
                        }
                    }
                    string sendablestring = callhistoryDictionary.GetSendableString(Email, id);
                    data = Encoding.ASCII.GetBytes(sendablestring);
                    ReplyMessage(Command.GetCallInfoSent, message.Identifier, data, networkStream);
                    break;
                case Command.AddCallToHistoryRequest:
                    string EMAIL = "";
                    string ID = "";
                    string DATA = "";
                    string EMAIL2 = "";
                    string TIME = "";
                    int HELP = 0;
                    foreach (char character in message.Data)
                    {
                        if (HELP == 0 && character != '&')
                        {
                            EMAIL += character;
                        }
                        else if (HELP == 1 && character != '&')
                        {
                            ID += character;
                        }
                        else if (character == '&')
                        {
                            HELP++;
                        }
                        else if (HELP == 2 && character != '&')
                        {
                            DATA += character;
                        }
                        else if (HELP == 3 && character != '&')
                        {
                            EMAIL2 += character;
                        }
                        else if (HELP == 4 && character != '&')
                        {
                            TIME += character;
                        }
                    }
                    callhistoryDictionary.AddCall(EMAIL, ID, DATA, EMAIL2, TIME);
                    ReplyMessage(Command.AddCallToHistoryAccepted, message.Identifier, new byte[] { }, networkStream);
                    break;
                case Command.ClearCallHistory:
                    Email = "";
                    foreach (char character in message.Data)
                    {
                        if (character != '&')
                        {
                            Email += character;
                        }
                    }
                    callhistoryDictionary.RemoveCallhistory(Email);
                    ReplyMessage(Command.ClearCallHistoryAccepted, message.Identifier, new byte[] { }, networkStream);
                    break;
                case Command.GetEmailFromIPRequest:
                    userMail = "";
                    foreach (char character in message.Data)
                    {
                        if (character != '&')
                        {
                            userMail += character;
                        }
                    }

                    string email = usersDictionary.GetEmail(userMail);
                    if (email != "nope")
                    {
                        byte[] bytes = Encoding.ASCII.GetBytes(email);
                        ReplyMessage(Command.GetEmailFromIPAccepted, message.Identifier, bytes, networkStream);
                    }
                    else
                    {
                        ReplyMessage(Command.GetEmailFromIPDenied, message.Identifier, new byte[] { }, networkStream);
                    }
                    break;

            }
        }

        private void ReplyMessage(Command command, int identifier, byte[] data, NetworkStream networkStream)
        {
            Packet message = new Packet(command, identifier, data);

            byte[] serializedMessage = message.Serialize();

            networkStream.Write(serializedMessage, 0, serializedMessage.Length);
        }
    }
}