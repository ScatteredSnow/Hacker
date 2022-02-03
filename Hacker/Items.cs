using System.Collections.Generic;
using System.Xml.Serialization;

namespace Hacker
{
    [XmlRoot(ElementName = "items")]
    public class Items
    {
        [XmlElement(ElementName = "item")]
        public List<Item> Item;
    }
}
