using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Reflection;

public class BuildScriptConfig
{
#if UNITY_STANDALONE
        static string AssetbundlePath = "E:/Workspace/MainVersion/assetbundles/Config/";

        [MenuItem("Assets/【PC】打包副本配置")]
        public static void BuildAllConfigs_PC()
        {
                BuildAllConfigs(AssetbundlePath, BuildTarget.StandaloneWindows);
        }
#endif

#if UNITY_ANDROID
        static string AssetbundlePath = "E:/Workspace/MainVersion/Android/assetbundles/Config/";

        [MenuItem("Assets/【ANDROID】打包副本配置")]
        public static void BuildAllConfigs_Android()
        {
                BuildAllConfigs(AssetbundlePath, BuildTarget.Android);
        }
#endif

#if UNITY_IPHONE
        static string AssetbundlePath = "E:/Workspace/MainVersion/IOS/assetbundles/Config/";
        [MenuItem("Assets/【IOS】打包副本配置")]
        public static void BuildAllConfigs_IOS()
        {
                BuildAllConfigs(AssetbundlePath, BuildTarget.iPhone);
        }
#endif

        public static void BuildAllConfigs(string outputPath, BuildTarget buildTarget)
        {
                if (System.IO.Directory.Exists(outputPath) == false)
                {
                        System.IO.Directory.CreateDirectory(outputPath);
                }

                string path = "Assets/Data/Config";// AssetDatabase.GetAssetPath(Selection.activeObject);
                string[] filepaths = System.IO.Directory.GetFiles(path);
                foreach (string filePath in filepaths)
                {
                        if (filePath.Contains(".meta"))
                                continue;

                        string fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
                        string outputAB = outputPath + fileName + ".unity3d";
                        System.DateTime assetDateTime = System.IO.Directory.GetLastWriteTime(outputAB);
                        if (System.IO.File.Exists(filePath))
                        {
                                System.DateTime dt = System.IO.Directory.GetLastWriteTime(filePath);
                                if (dt.CompareTo(assetDateTime) != 1)
                                {
/*                                        Debuger.Log(filePath);*/
                                        continue;
                                }
                        }
                        BuildConfig(outputPath, fileName, buildTarget);
                }
        }
        public static void BuildConfig(string outputPath, string configName, BuildTarget buildTarget) 
        {
                string dstPath = outputPath + configName + ".unity3d";
                if (/*(buildTarget == BuildTarget.Android || buildTarget == BuildTarget.iPhone) &&*/
                        System.IO.File.Exists("Assets/Script/ConfigHold/" + configName + ".cs"))
                {
                        CommonScriptableObject config = ScriptableObject.CreateInstance(configName) as CommonScriptableObject;
                        if (config != null)
                        {
                                config.LoadAll(configName);
                                string outputAssetPath = "Assets/ConfigParseOutput/" + configName + ".asset";
                                if (!System.IO.Directory.Exists("Assets/ConfigParseOutput/"))
                                        System.IO.Directory.CreateDirectory("Assets/ConfigParseOutput/");

                                EditorUtility.SetDirty((config));
                                AssetDatabase.CreateAsset(config, outputAssetPath);

                                if (System.IO.Directory.Exists(outputPath))
                                {
                                        config.name = configName;
                                        BuildPipeline.BuildAssetBundle((UnityEngine.Object)config, null, dstPath,
                                                BuildAssetBundleOptions.CollectDependencies |
                                                BuildAssetBundleOptions.CompleteAssets |
                                                BuildAssetBundleOptions.UncompressedAssetBundle,
                                                buildTarget);
                                        Debuger.LogWarning(configName + " PreLoad Success");
                                }
                        }
                }
                else
                {
                        string srcPath = "Assets/Data/Config/" + configName + ".xml";
                        BuildPipeline.BuildAssetBundle(AssetDatabase.LoadAssetAtPath<TextAsset>(srcPath), null, dstPath, 
                                BuildAssetBundleOptions.CollectDependencies | 
                                BuildAssetBundleOptions.UncompressedAssetBundle, buildTarget);
                        Debuger.LogWarning(configName + " Success");
                }
        }
}
