using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hacker
{
    [Serializable]
    public class GameObjectDetails
    {
        public GameObjectDetails()
        {
        }

        public GameObjectDetails(GameObject rootObject)
        {
            name = rootObject.name;
            if (rootObject.transform.parent != null)
            {
                parent = rootObject.transform.parent.gameObject.name;
            }
            else
            {
                parent = "null";
            }
            enabled = rootObject.activeSelf;
            layer = rootObject.layer;
            position = rootObject.transform.position.ToString();
            localPosition = rootObject.transform.localPosition.ToString();
            foreach (Component component in rootObject.GetComponents<Component>())
            {
                if (component != null)
                {
                    components.Add(component.GetType().FullName);
                }
            }
            int childCount = rootObject.transform.childCount;
            for (int j = 0; j < childCount; j++)
            {
                children.Add(new GameObjectDetails(rootObject.transform.GetChild(j).gameObject));
            }
        }

        public static string XMLSerialize(List<GameObjectDetails> objectTree)
        {
            string text = "<?xml version=\"1.0\"?>\r\n";
            text += "<GameObjects>\r\n";
            text = text + "<Count value=\"" + objectTree.Count.ToString() + "\" />\r\n";
            foreach (GameObjectDetails gameObjectDetails in objectTree)
            {
                text = text + "<GameObject name=\"" + gameObjectDetails.name + "\">\r\n";
                text += GameObjectDetails.CreateXMLGameObject(gameObjectDetails);
                text += "</GameObject>\r\n";
            }
            text += "</GameObjects>";
            return text;
        }

        private static string CreateXMLGameObject(GameObjectDetails obj)
        {
            string text = "";
            text = text + "<parent name=\"" + obj.parent + "\" />\r\n";
            text = text + "<enabled value=\"" + obj.enabled.ToString() + "\" />\r\n";
            text = text + "<layer value=\"" + obj.layer.ToString() + "\" />\r\n";
            text += "<components>\r\n";
            foreach (string str in obj.components)
            {
                text = text + "<component name=\"" + str + "\" />\r\n";
            }
            text += "</components>\r\n";
            if (obj.children.Count > 0)
            {
                text += "<children>\r\n";
                foreach (GameObjectDetails gameObjectDetails in obj.children)
                {
                    text = text + "<child name=\"" + gameObjectDetails.name + "\">\r\n";
                    text += GameObjectDetails.CreateXMLGameObject(gameObjectDetails);
                    text += "</child>\r\n";
                }
                text += "</children>\r\n";
            }
            return text;
        }

        public string name = "";

        public string parent = "";

        public bool enabled;

        public int layer = -1;

        public string position = "";

        public string localPosition = "";

        public List<string> components = new List<string>();

        public List<GameObjectDetails> children = new List<GameObjectDetails>();
    }
}
