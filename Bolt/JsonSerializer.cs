using System.IO;

namespace Bolt
{
    public class JsonSerializer : ISerializer
    {
        public void Write<T>(Stream stream, T data)
        {
            using (StreamWriter writer = new StreamWriter(stream))
            {
                writer.Write(Newtonsoft.Json.JsonConvert.SerializeObject(data));
            }
        }

        public T Read<T>(Stream data)
        {
            using (StreamReader reader = new StreamReader(data))
            {
                string rawString = reader.ReadToEnd();
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(rawString);
            }
        }

        public string ContentType
        {
            get { return "application/json"; }
        }
    }
}