using System.Collections.Generic;
using System.Xml.Serialization;

namespace Hacker
{
    [XmlRoot(ElementName = "item")]
    public class Item
    {
        [XmlElement(ElementName = "property")]
        public List<Property> Property;

        [XmlAttribute(AttributeName = "name")]
        public string Name;
    }
}
