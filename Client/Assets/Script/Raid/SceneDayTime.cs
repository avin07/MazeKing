using UnityEngine;
using System.Collections;

public class SceneDayTime : MonoBehaviour
{
        public int m_nMode = 0;//0黄昏 1夜晚 2白天 3无光 4中夜 5深夜

        void Awake ()
        {
                SetMode(m_nMode);
	}
        public void SetModeImme(int mode)
        {
                m_nMode = Mathf.Clamp(mode, 0, 5);
                foreach (SceneLightAdjust sla in this.gameObject.GetComponentsInChildren<SceneLightAdjust>())
                {
                        sla.SetModeImme(m_nMode);
                }
                foreach (WaterColorAdjust wca in this.gameObject.GetComponentsInChildren<WaterColorAdjust>())
                {
                        wca.SetMode(m_nMode);
                }
                AmbientLightAdjust ala = this.gameObject.GetComponentInChildren<AmbientLightAdjust>();
                if (ala != null)
                {
                        ala.SetModeImme(m_nMode);
                }

        }
        public void SetMode(int mode)
        {
                m_nMode = Mathf.Clamp(mode, 0, 5);
                foreach (SceneLightAdjust sla in this.gameObject.GetComponentsInChildren<SceneLightAdjust>())
                {
                        sla.SetMode(m_nMode);
                }
                foreach (WaterColorAdjust wca in this.gameObject.GetComponentsInChildren<WaterColorAdjust>())
                {
                        wca.SetMode(m_nMode);
                }
                AmbientLightAdjust ala = this.gameObject.GetComponentInChildren<AmbientLightAdjust>();
                if (ala != null)
                {
                        ala.SetMode(m_nMode);
                }
        }
}
