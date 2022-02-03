using System.IO;
using System.Xml.Serialization;

namespace Hacker
{
    public class Xml2Object
    {
        public static Items Deserialize(string filepath)
        {
            string s = File.ReadAllText(filepath);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Items));
            Items result;
            using (StringReader stringReader = new StringReader(s))
            {
                result = (Items)xmlSerializer.Deserialize(stringReader);
            }
            return result;
        }

        public static T Deserialize<T>(string filepath)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            StreamReader streamReader = new StreamReader(filepath);
            T result = (T)((object)xmlSerializer.Deserialize(streamReader.BaseStream));
            streamReader.Close();
            return result;
        }
    }
}
