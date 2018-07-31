using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TransparentSwitch : MonoBehaviour
{
    Dictionary<Transform, Material[]> m_OriMatDict = new Dictionary<Transform, Material[]>();

    public float _fAlpha = 0.4f;

    void SetAlpha(float alpha)
    {
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            if (!m_OriMatDict.ContainsKey(renderer.transform))
            {
                m_OriMatDict.Add(renderer.transform, renderer.sharedMaterials);
            }

            foreach (Material mat in renderer.materials)
            {
                //Debuger.Log(renderer.transform.name + " " + mat.shader.name);
                if (mat.shader.name == "Self-Illumin/Bumped Specular")
                {
                    //mat.shader = Shader.Find("Custom/Transparent_Self-Illumin/Bumped Specular");
                    mat.shader = Shader.Find("Custom/Transparent_Bumped Specular");
                    mat.SetColor("_Color", new Color(1f, 1f, 1f, alpha));
                }
                else if (mat.shader.name == "DF/Bumped Specular")
                {
                    mat.shader = Shader.Find("Custom/Transparent_Bumped Specular");
                    mat.SetColor("_Color", new Color(1f, 1f, 1f, alpha));
                }
            }
        }
    }

    void OnEnable()
    {
        SetAlpha(_fAlpha);
    }
    void OnDisable()
    {
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            if (m_OriMatDict.ContainsKey(renderer.transform))
            {
                for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    renderer.sharedMaterials = m_OriMatDict[renderer.transform];
                }
            }
        }
    }
}