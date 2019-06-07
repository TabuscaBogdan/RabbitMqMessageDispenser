using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Utils
{
    public static class Serialization
    {
        public static byte[] SerializeAndGetBytes<T>(T message)
        {
            string json = JsonConvert.SerializeObject(message);
            return Encoding.UTF8.GetBytes(json);
        }

        public static T Deserialize<T>(byte[] message)
        {
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(message));
        }
    }
}
