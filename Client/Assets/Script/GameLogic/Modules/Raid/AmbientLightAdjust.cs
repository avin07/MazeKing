using UnityEngine;
using System.Collections;

public class AmbientLightAdjust : MonoBehaviour
{
        public int m_nMode;
        public Color[] AmbientColors;
        // Use this for initialization
        void Start()
        {
                SetMode(m_nMode);
        }
        public void SetModeImme(int nextMode)
        {
                m_nMode = nextMode;
                if (m_nMode < AmbientColors.Length)
                {
                        RenderSettings.ambientLight = AmbientColors[m_nMode];
                }
        }
        public void SetMode(int mode)
        {
                int nextMode = Mathf.Clamp(mode, 0, AmbientColors.Length - 1);
                //if (nextMode != m_nMode)
                {
                        StartCoroutine(ProcessChange(nextMode));
                }
        }
        IEnumerator ProcessChange(int nextMode)
        {
                float time = Time.realtimeSinceStartup;
                while (Time.realtimeSinceStartup - time < 1f)
                {
                        float deltaT = (Time.realtimeSinceStartup - time) / 1f;
                        if (this.GetComponent<Light>() != null)
                        {
                                if (m_nMode < AmbientColors.Length && nextMode < AmbientColors.Length)
                                {
                                        RenderSettings.ambientLight = Color.Lerp(AmbientColors[m_nMode], AmbientColors[nextMode], deltaT);
                                }
                        }
                        yield return null;
                }
                SetModeImme(nextMode);
        }

}
