using System;
using System.Runtime.Serialization;

namespace TestService.Contracts
{
    [DataContract]
    public class ServerInfo
    {
        public static ServerInfo Create()
        {
            return new ServerInfo() { Machine = Environment.MachineName, Ticks = Environment.TickCount, Time = DateTime.UtcNow };
        }

        [DataMember(Order = 1)]
        public string Machine { get; set; }

        [DataMember(Order = 2)]
        public long Ticks { get; set; }

        [DataMember(Order = 3)]
        public DateTime Time { get; set; }
    }
}