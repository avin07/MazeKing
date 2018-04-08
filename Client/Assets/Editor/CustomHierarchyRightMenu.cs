using UnityEngine;
using System.IO;
using System.Collections;
using UnityEditor;
public class CustomHierarchyRightMenu
{
        [MenuItem("GameObject/UI_Menu_Save", false, 0)]
        static void SaveUIPrefab()
        {
                Debuger.Log(Selection.activeObject.name);
                PrefabUtility.CreatePrefab("Assets/Resources/UI/" + Selection.activeGameObject.name + ".prefab", Selection.activeGameObject, ReplacePrefabOptions.ConnectToPrefab);

        }
}
