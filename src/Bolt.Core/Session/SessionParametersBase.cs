using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Bolt.Session
{
    [DataContract]
    public class SessionParametersBase
    {
        public SessionParametersBase()
        {
            UserData = new Dictionary<string, string>();
        }

        [DataMember]
        public Dictionary<string, string> UserData { get; set; }
    }
}