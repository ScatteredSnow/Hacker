using System.Xml.Serialization;

namespace Hacker
{
    [XmlRoot(ElementName = "display_value")]
    public class DisplayValue
    {
        [XmlAttribute(AttributeName = "name")]
        public string Name;

        [XmlAttribute(AttributeName = "value")]
        public int Value;
    }
}
