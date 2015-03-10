﻿using System;
using System.IO;
using System.Text;

using Newtonsoft.Json;

namespace Bolt
{
    public class JsonSerializer : ISerializer
    {
        public JsonSerializer()
        {
            Serializer = new Newtonsoft.Json.JsonSerializer()
                             {
                                 NullValueHandling = NullValueHandling.Ignore,
                                 TypeNameHandling = TypeNameHandling.Auto,
                                 Formatting = Formatting.None,
                                 ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
                             };
        }

        public string ContentType
        {
            get { return "application/json"; }
        }

        public Newtonsoft.Json.JsonSerializer Serializer { get; private set; }

        public virtual void Write<T>(Stream stream, T data)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            using (TextWriter writer = new StreamWriter(stream, Encoding.UTF8, 4096, true))
            {
                Serializer.Serialize(writer, data);
            }
        }

        public virtual T Read<T>(Stream stream)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            using (TextReader reader = new StreamReader(stream, Encoding.UTF8, true, 4096, true))
            {
                using (JsonReader jsonReader = new JsonTextReader(reader))
                {
                    return Serializer.Deserialize<T>(jsonReader);
                }
            }
        }
    }
}