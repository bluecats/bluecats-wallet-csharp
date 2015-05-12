using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace BlueCats.Wallet.Serialization
{
    public static class DictionarySerializer
    {
        public static byte[] SerializeToByteArray(Dictionary<string, object> dictionary)
        {
            var ascii = JsonConvert.SerializeObject(dictionary);
            byte[] data = Encoding.ASCII.GetBytes(ascii);
            return data;
        }

        public static Dictionary<string, object> DeserializeFromByteArray(byte[] data)
        {
            string ascii = Encoding.ASCII.GetString(data);
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>(ascii);
            return dictionary;
        }
    }
}
