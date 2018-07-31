using UnityEngine;
using System.Collections;
using System.IO;
public class LocalFileIO
{
        public static string SaveDataPath
        {
                get
                {
                        string path = null;
                        if (Application.platform == RuntimePlatform.IPhonePlayer)
                        {
                                path = Application.dataPath.Substring(0, Application.dataPath.Length - 5);
                                path = path.Substring(0, path.LastIndexOf('/')) + "/Documents/";
                        }
                        else if (Application.platform == RuntimePlatform.Android)
                        {
                                path = Application.persistentDataPath + "/";
                        }
                        else
                        {
                                path = Application.dataPath + "/../GameLog/";
                        }
                        return path;
                }
        }
        public static void LoadDataWithFile(string txtName)
        {
                string path = SaveDataPath + txtName;

                if (File.Exists(path))
                {
                        FileStream fs = new FileStream(path, FileMode.Open);
                        StreamReader sr = new StreamReader(fs);
                        sr.Close();
                        fs.Close();
                }
                else
                {
                        Debuger.Log("No json ");
                }
        }
        public static void WriteDataWithFile(string txtName, string content)
        {
                if (!Directory.Exists(SaveDataPath))
                {
                        Directory.CreateDirectory(SaveDataPath);
                }
                string path = SaveDataPath + txtName;
                FileStream fs;
                if (!File.Exists(path))
                {
                        fs = new FileStream(path, FileMode.Create);
                }
                else
                {
                        fs = new FileStream(path, FileMode.Truncate);//注意对存在文件内容替换
                }
               
                StreamWriter sw = new StreamWriter(fs);
                sw.Write(content);
                sw.Close();
                fs.Close();
                Debug.Log(path);
        }

        public static void WriteBytesToFile(string fileName, byte[] bytes)
        {
                string filePath = SaveDataPath + fileName;
                FileStream fs = File.Open(filePath, FileMode.Create);
                BinaryWriter bw = new BinaryWriter(fs);
                bw.Write(bytes);
                bw.Flush();
                bw.Close();
                fs.Close();
        }
}
