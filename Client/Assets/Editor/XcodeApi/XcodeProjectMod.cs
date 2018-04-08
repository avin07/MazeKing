using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode; // ←さっきいれたXcodeAPI
using System.Collections;
using System.IO;
 
public class XcodeProjectMod : MonoBehaviour
{
 
    // ちょっとしたユーティリティ関数（http://goo.gl/fzYig8を参考）
    internal static void CopyAndReplaceDirectory(string srcPath, string dstPath)
    {
        if (Directory.Exists(dstPath))
            Directory.Delete(dstPath);
        if (File.Exists(dstPath))
            File.Delete(dstPath);
 
        Directory.CreateDirectory(dstPath);
 
        foreach (var file in Directory.GetFiles(srcPath))
            File.Copy(file, Path.Combine(dstPath, Path.GetFileName(file)));
 
        foreach (var dir in Directory.GetDirectories(srcPath))
            CopyAndReplaceDirectory(dir, Path.Combine(dstPath, Path.GetFileName(dir)));
    }
 
    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
    {
        if (buildTarget == BuildTarget.iOS)
        {
                Debuger.Log(path);
                string projPath = PBXProject.GetPBXProjectPath(path);
                PBXProject proj = new PBXProject();

                proj.ReadFromString(File.ReadAllText(projPath));
                string target = proj.TargetGuidByName(PBXProject.GetUnityTargetName());

//                 //Handle xcodeproj  
//                 File.Copy(Application.dataPath + "/Editor/xcodeapi/Res/KeychainAccessGroups.plist", path + "/KeychainAccessGroups.plist", true);
// 
//                 proj.AddFile("KeychainAccessGroups.plist", "KeychainAccessGroups.plist");
// 
//                 var codesign = Debuger.isDebugBuild ? CODE_SIGN_DEVELOPER : CODE_SIGN_DISTRIBUTION;
//                 var provision = Debuger.isDebugBuild ? PROVISIONING_DEVELOPER : PROVISIONING_DISTRIBUTION;
// 
//                 proj.SetBuildProperty(target, "CODE_SIGN_IDENTITY", codesign);
//                 proj.SetBuildProperty(target, "PROVISIONING_PROFILE", provision);
//                 proj.SetBuildProperty(target, "CODE_SIGN_ENTITLEMENTS", "KeychainAccessGroups.plist");
//                 proj.SetBuildProperty(target, "ENABLE_BITCODE", "NO");
//                 proj.SetGeneralTeam(target, "xxxxx");//fix source code  
// 
//                 File.WriteAllText(projPath, proj.WriteToString());
// 
                //Handle plist  
                string plistPath = path + "/Info.plist";
                PlistDocument plist = new PlistDocument();
                plist.ReadFromString(File.ReadAllText(plistPath));
                PlistElementDict rootDict = plist.root;
                PlistElementDict ATSDict = rootDict.CreateDict("NSAppTransportSecurity");
                ATSDict.SetBoolean("NSAllowsArbitraryLoads", true);

                //rootDict.SetString("CFBundleVersion", GetVer());//GetVer() 返回自定义自增值  

                File.WriteAllText(plistPath, plist.WriteToString());
        }
    }
}
