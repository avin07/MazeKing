using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Xml;
using System;

public class XML2CS
{
        const string STRING_TABLE = "        ";

        [@MenuItem("Assets/XML_TO_CS")]
        public static void XML_TO_CS()
        {
                string file_name = Selection.activeObject.name;

                string path = AssetDatabase.GetAssetPath(Selection.activeObject);
                if (path.EndsWith(".xml"))
                {
                        string filePath = EditorUtility.SaveFilePanel("title", "Assets/Script/ConfigHold/", MachiningFileName(file_name), "cs");
                        string fileName = Path.GetFileNameWithoutExtension(filePath);

                        if (String.IsNullOrEmpty(filePath))
                        {
                                return;
                        }

                        FileStream fs = File.Create(filePath);
                        StreamWriter sw = new StreamWriter(fs);
                        sw.WriteLine("using UnityEngine;");
                        sw.WriteLine("using System.Collections;");
                        sw.WriteLine("using System.Collections.Generic;");
                        sw.WriteLine("using System.Xml;");
                        sw.WriteLine("");
                        sw.WriteLine("public class " + fileName +" : ConfigBase");
                        sw.WriteLine("{");

                        XmlDocument xml = new XmlDocument();
                        XmlReaderSettings settings = new XmlReaderSettings();
                        settings.IgnoreComments = true;
                        XmlReader reader = XmlReader.Create(path, settings);
                        xml.Load(reader);
                        XmlNode root = xml.SelectSingleNode("dataroot");
                        Debuger.Log(root.Name);

                        XmlNode firstNode = root.FirstChild;
                        if (firstNode != null)
                        {
                                foreach (XmlNode node in firstNode.ChildNodes)
                                {
                                        if (node.Attributes.GetNamedItem("type") != null)
                                        {
                                                if (node.Name.Equals("id"))
                                                {
                                                        continue;
                                                }
                                                if (node.Name.Equals("mark"))
                                                {
                                                        continue;
                                                }
                                                if (node.Attributes.GetNamedItem("type").Value == "1")
                                                {
                                                        sw.WriteLine(STRING_TABLE + "public int " + node.Name + ";");
                                                }
                                                else if (node.Attributes.GetNamedItem("type").Value == "3")
                                                {
                                                        sw.WriteLine(STRING_TABLE + "public string " + node.Name + ";");
                                                }
                                        }
                                }
                        }
                        sw.WriteLine("");

                        sw.WriteLine(STRING_TABLE + "public " + fileName + "(XmlNode child) : base(child)");
                        sw.WriteLine(STRING_TABLE + "{");
                        sw.WriteLine(STRING_TABLE + "}");
                        sw.WriteLine("");

                        sw.WriteLine(STRING_TABLE + "public override void InitSelf(XmlNode child)");
                        sw.WriteLine(STRING_TABLE + "{");
                        sw.WriteLine(STRING_TABLE + STRING_TABLE  + "SetupFields(child);");
                        sw.WriteLine(STRING_TABLE + "}");
                        sw.WriteLine("");

                        sw.WriteLine("}");
                        sw.Close();
                        fs.Close();

                        //XMLPARSE_METHOD.GetNodeInnerInt(root, "ManualMode", ref mode, 0);
                }
        }


        static string MachiningFileName(string name)
        {
                string file_name = "";
                string[] temp = name.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < temp.Length; i++)
                {
                        temp[i] = temp[i].Substring(0, 1).ToUpper() + temp[i].Substring(1, temp[i].Length - 1);
                        file_name += temp[i];
                }
                file_name += "Config";
                return file_name;

        }
}
