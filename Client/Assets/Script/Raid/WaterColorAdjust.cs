using UnityEngine;
using System.Collections;

public class WaterColorAdjust : MonoBehaviour
{
        public int m_nMode = 0;//0白天 1黄昏 2夜晚
        public Color[] basewater_color;
        public Color[] Tile_basecolor;
        public Color[] Tile_reflectioncolor;
        public Color[] Tile_specularcolor;
        public Vector4[] Tile_distortparams;

        public Transform base_water;
        public Transform Tile;

        void Start ()
        {
                if (base_water == null)
                {
                        base_water = this.transform.Find("Water4Example (Simple)/basewater");
                }
                if (Tile == null)
                {
                        Tile = this.transform.Find("Water4Example (Simple)/Tile");
                }
                SetMode(m_nMode);
        }

        public void SetMode(int mode)
        {
                m_nMode = Mathf.Clamp(mode, 0, basewater_color.Length - 1);
                if (base_water.GetComponent<Renderer>().sharedMaterial != null)
                {
                        base_water.GetComponent<Renderer>().sharedMaterial.SetColor("_Color", basewater_color[m_nMode]);
                }
                if (Tile.GetComponent<Renderer>().sharedMaterial != null)
                {
                        if (m_nMode < Tile_basecolor.Length)
                        {
                                Tile.GetComponent<Renderer>().sharedMaterial.SetColor("_BaseColor", Tile_basecolor[m_nMode]);
                        }
                        if (m_nMode < Tile_reflectioncolor.Length)
                        {
                                Tile.GetComponent<Renderer>().sharedMaterial.SetColor("_ReflectionColor", Tile_reflectioncolor[m_nMode]);
                        }
                        if (m_nMode < Tile_specularcolor.Length)
                        {
                                Tile.GetComponent<Renderer>().sharedMaterial.SetColor("_SpecularColor", Tile_specularcolor[m_nMode]);
                        }
                        if (m_nMode < Tile_distortparams.Length)
                        {
                                Tile.GetComponent<Renderer>().sharedMaterial.SetVector("_DistortParams", Tile_distortparams[m_nMode]);
                        }
                }
        }
}
