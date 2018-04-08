using UnityEditor;
using UnityEngine;
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;

public	class UIPerfabHelp
{

        static string s_buildAssetPath;
        [MenuItem("Window/UI界面导出")]
        static void UIExport()
        {
                // 打开保存面板，获得用户选择的路径  
                if (string.IsNullOrEmpty(s_buildAssetPath))
                {
                        s_buildAssetPath = EditorUtility.SaveFilePanel("Save Resource", "", "MyUI", "unitypackage");
                }
                else
                {
                        s_buildAssetPath = EditorUtility.SaveFilePanel("Save Resource", s_buildAssetPath, "MyUI", "unitypackage");
                }

                List<string> m_paths = new List<string>();
                string UIpath = "Assets/Resources/UI";
                string[] filepaths = System.IO.Directory.GetFiles(UIpath, "*.prefab", SearchOption.AllDirectories);
                for (int i = 0; i < filepaths.Length; i++)
                {
                        string[] paths = AssetDatabase.GetDependencies(new string[] { filepaths[i] });
                        foreach (string path in paths)
                        {
                                if (!m_paths.Contains(path))
                                {
                                        m_paths.Add(path);
                                }
                        }

                }

                AssetDatabase.ExportPackage(m_paths.ToArray(), s_buildAssetPath, ExportPackageOptions.IncludeDependencies);
                AssetDatabase.Refresh();
                Debuger.Log("Build all Done!");
        }
}



