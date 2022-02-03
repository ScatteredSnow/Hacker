using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Hacker
{
    public static class LocalExtensions
    {
        public static string ToFixedString(this string value, int length, char appendChar = ' ')
        {
            int length2 = value.Length;
            int num = (length == length2) ? 0 : (length - length2);
            if (num == 0)
            {
                return value;
            }
            if (num <= 0)
            {
                return new string(new string(value.ToCharArray().Reverse<char>().ToArray<char>()).Substring(num * -1, value.Length - num * -1).ToCharArray().Reverse<char>().ToArray<char>());
            }
            return value + new string(' ', num);
        }

        public static bool HasComponent<T>(this GameObject flag) where T : Component
        {
            return !(flag == null) && flag.GetComponent<T>() != null;
        }

        public static void DumpAllText(this GameObject obj)
        {
            string text = "";
            Text[] componentsInChildren = obj.GetComponentsInChildren<Text>();
            text = text + "Texts CNT: " + componentsInChildren.Length.ToString() + "\r\n";
            foreach (Text text2 in componentsInChildren)
            {
                text = string.Concat(new string[]
                {
                    text,
                    "GameObj: ",
                    text2.transform.parent.gameObject.name,
                    "> ",
                    text2.text.Replace("\n", " "),
                    "\r\n"
                });
            }
            File.WriteAllText(string.Concat(new string[]
            {
                TrainerMenu.baseDirectory,
                "\\",
                obj.name,
                "-",
                obj.GetHashCode().ToString(),
                "-TextsDump.txt"
            }), text);
            Debug.Log(string.Concat(new string[]
            {
                "Dumped All Texts For ",
                obj.name,
                " To: ",
                TrainerMenu.baseDirectory,
                "\\",
                obj.name,
                "-",
                obj.GetHashCode().ToString(),
                "-TextsDump.txt"
            }));
        }

        public static void DumpGameObjectXML(this GameObject obj)
        {
            if (obj == null)
            {
                return;
            }
            List<GameObjectDetails> list = new List<GameObjectDetails>();
            list.Add(new GameObjectDetails(obj));
            if (!Directory.Exists(TrainerMenu.baseDirectory + "\\OBJECT_DUMPS\\xml\\"))
            {
                Directory.CreateDirectory(TrainerMenu.baseDirectory + "\\OBJECT_DUMPS\\xml\\");
            }
            File.WriteAllText(TrainerMenu.baseDirectory + "\\OBJECT_DUMPS\\xml\\" + obj.name + ".xml", GameObjectDetails.XMLSerialize(list));
        }

        public static string Dump(this object obj, int indentsize = 4, bool writetofile = true, string fileindex = "")
        {
            string text = "Object Dump:\r\n";
            string text2 = " ";
            if (indentsize > 0)
            {
                for (int i = 1; i <= indentsize; i++)
                {
                    text2 += " ";
                }
                if (obj.Equals(typeof(Nullable)))
                {
                    return "Object is null";
                }
            }
            try
            {
                text += "Properties:\r\n";
                foreach (KeyValuePair<string, string> keyValuePair in LocalExtensions.GetObjectProperties(obj))
                {
                    if (keyValuePair.Key != null)
                    {
                        text = string.Concat(new string[]
                        {
                            text,
                            text2,
                            keyValuePair.Key,
                            ": ",
                            keyValuePair.Value,
                            "\r\n"
                        });
                    }
                }
            }
            catch
            {
            }
            try
            {
                text += "Fields:\r\n";
                foreach (KeyValuePair<string, string> keyValuePair2 in LocalExtensions.GetObjectFields(obj))
                {
                    if (keyValuePair2.Key != null)
                    {
                        text = string.Concat(new string[]
                        {
                            text,
                            text2,
                            keyValuePair2.Key,
                            ": ",
                            keyValuePair2.Value,
                            "\r\n"
                        });
                    }
                }
            }
            catch
            {
            }
            try
            {
                text += "Methods:\r\n";
                foreach (string text3 in LocalExtensions.GetObjectMethods(obj))
                {
                    if (text3 != null)
                    {
                        text = text + text2 + text3 + "\r\n";
                    }
                }
            }
            catch
            {
            }
            if (writetofile)
            {
                try
                {
                    File.WriteAllText(string.Concat(new string[]
                    {
                        TrainerMenu.baseDirectory,
                        "\\ObjectDump-",
                        obj.GetType().FullName,
                        fileindex,
                        ".txt"
                    }), text, Encoding.ASCII);
                    Debug.Log(string.Concat(new string[]
                    {
                        "Dump File Written To: ",
                        TrainerMenu.baseDirectory,
                        "\\ObjectDump-",
                        obj.GetType().FullName,
                        fileindex,
                        ".txt"
                    }));
                }
                catch (Exception ex)
                {
                    Debug.LogError("Dump Object Error: " + ex.Message);
                }
            }
            return text;
        }

        private static Dictionary<string, string> GetObjectProperties(object obj)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            if (obj.Equals(typeof(Nullable)))
            {
                return dictionary;
            }
            foreach (PropertyInfo propertyInfo in obj.GetType().GetProperties(TrainerMenu.propertyBindingFlags))
            {
                try
                {
                    object value = propertyInfo.GetValue(obj, new object[0]);
                    string value2 = "";
                    if (!value.Equals(typeof(Nullable)))
                    {
                        value2 = value.ToString();
                    }
                    dictionary.Add(propertyInfo.Accessmodifier().ToString() + " " + propertyInfo.Name, value2);
                }
                catch (Exception ex)
                {
                    Debug.LogError("ERROR DUMPIMG PROPERTY: (" + propertyInfo.Name + ") - " + ex.Message);
                }
            }
            return dictionary;
        }

        private static Dictionary<string, string> GetObjectFields(object obj)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            if (obj.Equals(typeof(Nullable)))
            {
                return dictionary;
            }
            foreach (FieldInfo fieldInfo in obj.GetType().GetFields(TrainerMenu.fieldBindingFlags))
            {
                try
                {
                    object value = fieldInfo.GetValue(obj);
                    string value2 = (value == null) ? " " : value.ToString();
                    dictionary.Add(fieldInfo.Name, value2);
                }
                catch (Exception ex)
                {
                    Debug.LogError("ERROR DUMPIMG FIELD: (" + fieldInfo.Name + ") - " + ex.Message);
                }
            }
            return dictionary;
        }

        private static List<string> GetObjectMethods(object obj)
        {
            List<string> list = new List<string>();
            if (obj.Equals(typeof(Nullable)))
            {
                return list;
            }
            foreach (MethodInfo methodInfo in obj.GetType().GetMethods(TrainerMenu.defaultBindingFlags))
            {
                try
                {
                    Type returnType = methodInfo.ReturnType;
                    string text = string.Concat(new string[]
                    {
                        methodInfo.Accessmodifier().ToString(),
                        " ",
                        returnType.Name.ToString(),
                        " ",
                        methodInfo.Name,
                        " ("
                    });
                    foreach (ParameterInfo parameterInfo in from obj2 in methodInfo.GetParameters()
                                                            orderby obj2.Position
                                                            select obj2)
                    {
                        text = string.Concat(new string[]
                        {
                            text,
                            parameterInfo.ParameterType.Name,
                            " ",
                            parameterInfo.Name,
                            ", "
                        });
                    }
                    if (text.EndsWith(", "))
                    {
                        text = text.Remove(text.Length - 2);
                    }
                    text += ")";
                    list.Add(text);
                }
                catch (Exception ex)
                {
                    Debug.LogError("ERROR DUMPIMG METHOD: (" + methodInfo.Name + ") - " + ex.Message);
                }
            }
            return list;
        }

        public static string Dump(this Type type, int indentsize = 4, bool writetofile = true)
        {
            string text = "Static Type Dump:\r\n";
            string text2 = " ";
            if (indentsize > 0)
            {
                for (int i = 1; i <= indentsize; i++)
                {
                    text2 += " ";
                }
                if (type == null)
                {
                    return "Type is null";
                }
            }
            try
            {
                text += "Properties:\r\n";
                foreach (KeyValuePair<string, string> keyValuePair in LocalExtensions.GetTypeProperties(type))
                {
                    if (keyValuePair.Key != null)
                    {
                        text = string.Concat(new string[]
                        {
                            text,
                            text2,
                            keyValuePair.Key,
                            ": ",
                            keyValuePair.Value,
                            "\r\n"
                        });
                    }
                }
            }
            catch
            {
            }
            try
            {
                text += "Fields:\r\n";
                foreach (KeyValuePair<string, string> keyValuePair2 in LocalExtensions.GetTypeFields(type))
                {
                    if (keyValuePair2.Key != null)
                    {
                        text = string.Concat(new string[]
                        {
                            text,
                            text2,
                            keyValuePair2.Key,
                            ": ",
                            keyValuePair2.Value,
                            "\r\n"
                        });
                    }
                }
            }
            catch
            {
            }
            try
            {
                text += "Methods:\r\n";
                foreach (string text3 in LocalExtensions.GetTypeMethods(type))
                {
                    if (text3 != null)
                    {
                        text = text + text2 + text3 + "\r\n";
                    }
                }
            }
            catch
            {
            }
            if (writetofile)
            {
                try
                {
                    File.WriteAllText(TrainerMenu.baseDirectory + "\\TypeDump-" + type.FullName + ".txt", text, Encoding.ASCII);
                    Debug.Log(string.Concat(new string[]
                    {
                        "Dump File Written To: ",
                        TrainerMenu.baseDirectory,
                        "\\TypeDump-",
                        type.FullName,
                        ".txt"
                    }));
                }
                catch (Exception ex)
                {
                    Debug.LogError("Dump Type Error: " + ex.Message);
                }
            }
            return text;
        }

        private static Dictionary<string, string> GetTypeProperties(Type type)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            if (type.Equals(typeof(Nullable)))
            {
                return dictionary;
            }
            foreach (PropertyInfo propertyInfo in type.GetProperties(TrainerMenu.propertyBindingFlags))
            {
                try
                {
                    object value = propertyInfo.GetValue(type, new object[0]);
                    string value2 = (value == null) ? " " : value.ToString();
                    dictionary.Add(propertyInfo.Accessmodifier().ToString() + " " + propertyInfo.Name, value2);
                }
                catch (Exception ex)
                {
                    Debug.LogError("ERROR DUMPIMG PROPERTY: (" + propertyInfo.Name + ") - " + ex.Message);
                }
            }
            return dictionary;
        }

        private static Dictionary<string, string> GetTypeFields(Type type)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            if (type.Equals(typeof(Nullable)))
            {
                return dictionary;
            }
            foreach (FieldInfo fieldInfo in type.GetFields(TrainerMenu.bindflags))
            {
                try
                {
                    object value = fieldInfo.GetValue(type);
                    string value2 = (value == null) ? " " : value.ToString();
                    dictionary.Add(fieldInfo.Name, value2);
                }
                catch (Exception ex)
                {
                    Debug.LogError("ERROR DUMPIMG FIELD: (" + fieldInfo.Name + ") - " + ex.Message);
                }
            }
            return dictionary;
        }

        private static List<string> GetTypeMethods(Type type)
        {
            List<string> list = new List<string>();
            if (type.Equals(typeof(Nullable)))
            {
                return list;
            }
            foreach (MethodInfo methodInfo in type.GetMethods(TrainerMenu.bindflags))
            {
                try
                {
                    Type returnType = methodInfo.ReturnType;
                    string text = string.Concat(new string[]
                    {
                        methodInfo.Accessmodifier().ToString(),
                        " ",
                        returnType.Name.ToString(),
                        " ",
                        methodInfo.Name,
                        " ("
                    });
                    foreach (ParameterInfo parameterInfo in from obj2 in methodInfo.GetParameters()
                                                            orderby obj2.Position
                                                            select obj2)
                    {
                        text = string.Concat(new string[]
                        {
                            text,
                            parameterInfo.ParameterType.Name,
                            " ",
                            parameterInfo.Name,
                            ", "
                        });
                    }
                    if (text.EndsWith(", "))
                    {
                        text = text.Remove(text.Length - 2);
                    }
                    text += ")";
                    list.Add(text);
                }
                catch (Exception ex)
                {
                    Debug.LogError("ERROR DUMPIMG METHOD: (" + methodInfo.Name + ") - " + ex.Message);
                }
            }
            return list;
        }

        public static LocalExtensions.AccessModifier Accessmodifier(this PropertyInfo propertyInfo)
        {
            if (propertyInfo.SetMethod == null)
            {
                return propertyInfo.GetMethod.Accessmodifier();
            }
            if (propertyInfo.GetMethod == null)
            {
                return propertyInfo.SetMethod.Accessmodifier();
            }
            int index = Math.Max(LocalExtensions.AccessModifiers.IndexOf(propertyInfo.GetMethod.Accessmodifier()), LocalExtensions.AccessModifiers.IndexOf(propertyInfo.SetMethod.Accessmodifier()));
            return LocalExtensions.AccessModifiers[index];
        }

        public static LocalExtensions.AccessModifier Accessmodifier(this MethodInfo methodInfo)
        {
            if (methodInfo.IsPrivate)
            {
                return LocalExtensions.AccessModifier.Private;
            }
            if (methodInfo.IsFamily)
            {
                return LocalExtensions.AccessModifier.Protected;
            }
            if (methodInfo.IsFamilyOrAssembly)
            {
                return LocalExtensions.AccessModifier.ProtectedInternal;
            }
            if (methodInfo.IsAssembly)
            {
                return LocalExtensions.AccessModifier.Internal;
            }
            if (methodInfo.IsPublic)
            {
                return LocalExtensions.AccessModifier.Public;
            }
            throw new ArgumentException("Did not find access modifier", "methodInfo");
        }

        public static void Move<T>(this List<T> list, int oldIndex, int newIndex)
        {
            T item = list[oldIndex];
            list.RemoveAt(oldIndex);
            if (newIndex > oldIndex)
            {
                newIndex--;
            }
            list.Insert(newIndex, item);
        }

        public static void Move<T>(this List<T> list, T item, int newIndex)
        {
            if (item != null)
            {
                int num = list.IndexOf(item);
                if (num > -1)
                {
                    list.RemoveAt(num);
                    if (newIndex > num)
                    {
                        newIndex--;
                    }
                    list.Insert(newIndex, item);
                }
            }
        }

        public static ParameterInfo[] GetCachedParemeters(this MethodInfo mo)
        {
            ParameterInfo[] parameters;
            if (!LocalExtensions.ParametersOfMethods.TryGetValue(mo, out parameters))
            {
                parameters = mo.GetParameters();
                LocalExtensions.ParametersOfMethods[mo] = parameters;
            }
            return parameters;
        }

        public static readonly List<LocalExtensions.AccessModifier> AccessModifiers = new List<LocalExtensions.AccessModifier>
        {
            LocalExtensions.AccessModifier.Private,
            LocalExtensions.AccessModifier.Protected,
            LocalExtensions.AccessModifier.ProtectedInternal,
            LocalExtensions.AccessModifier.Internal,
            LocalExtensions.AccessModifier.Public
        };

        public static Dictionary<MethodInfo, ParameterInfo[]> ParametersOfMethods = new Dictionary<MethodInfo, ParameterInfo[]>();

        public enum AccessModifier
        {
            Private,
            Protected,
            ProtectedInternal,
            Internal,
            Public
        }
    }
}
