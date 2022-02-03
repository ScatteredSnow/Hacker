using System.Collections.Generic;
using System.Xml.Serialization;

namespace Hacker
{
    [XmlRoot(ElementName = "property")]
    public class Property
    {
        [XmlElement(ElementName = "property")]
        public List<Property> property;

        [XmlAttribute(AttributeName = "class")]
        public string Class;

        [XmlAttribute(AttributeName = "name")]
        public string Name;

        [XmlAttribute(AttributeName = "value")]
        public string Value;

        [XmlAttribute(AttributeName = "param1")]
        public string Param1;
    }
}
