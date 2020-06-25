using System;
using System.Net.Sockets;
using System.Linq;

namespace TIPServer.TIPPacket
{
    public sealed class Packet
    {
        public Command Command { get; set; }
        public int Identifier { get; }
        public int DataLength { get; }
        public byte[] Data { get; set; }

        public Packet(Command command, int identifier, byte[] data)
        {
            Command = command;
            Identifier = identifier;
            DataLength = data.Length;
            Data = data;
        }

        public Packet(NetworkStream networkStream)
        {
            byte[] header = new byte[6];

            networkStream.Read(header, 0, 6);

            Command = (Command)(header[0] << 8 | header[1]);
            Identifier = header[2] << 8 | header[3];
            DataLength = header[4] << 8 | header[5];

            if (DataLength > 0)
            {
                byte[] data = new byte[DataLength];

                networkStream.Read(data, 0, DataLength);

                Data = data;
            }
        }

        public byte[] Serialize()
        {
            return new byte[] {
                Convert.ToByte((int)Command >> 8 & 255), Convert.ToByte((int)Command & 255),
                Convert.ToByte(Identifier >> 8 & 255), Convert.ToByte(Identifier & 255),
                Convert.ToByte(DataLength >> 8 & 255), Convert.ToByte(DataLength & 255),
            }.Concat(Data).ToArray();
        }
    }
}