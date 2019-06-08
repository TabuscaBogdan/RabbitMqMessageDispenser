using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Utils
{
    public static class ProtoSerialization
    {
        public static byte[] SerializeAndGetBytes<T>(T message)
        {
            var stream = new MemoryStream();
            Serializer.Serialize(stream, message);
            return stream.ToArray();
        }

        public static T Deserialize<T>(byte[] message)
        {
            var stream = new MemoryStream(message);
            return Serializer.Deserialize<T>(stream);
        }
    }
}
