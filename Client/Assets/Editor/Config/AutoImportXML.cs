using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
[InitializeOnLoadAttribute]
public static class AutoImportXML
{
    static bool m_isEnableAutoGenerateConfig;
    // Use this for initialization
    static AutoImportXML()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        Debug.Log("OnPlayModeStateChanged " + state);
        if (m_isEnableAutoGenerateConfig && state == PlayModeStateChange.ExitingEditMode)
        {
        }
    }
}
