using UnityEngine;
using System.Collections;

public class SceneLightAdjust : MonoBehaviour
{
        public int m_nMode = 0;//0黄昏 1夜晚 2白天 3无光 4中夜 5深夜 6深深夜

        public Color[] Colors;
        public Vector3[] Rotations;
        public float[] Intensities;
        public float[] Ranges;
        public void SetModeImme(int nextMode)
        {
                m_nMode = nextMode;

                if (m_nMode < Colors.Length)
                {
                        this.GetComponent<Light>().color = Colors[m_nMode];
                }
                if (m_nMode < Rotations.Length)
                {
                        this.transform.rotation = Quaternion.Euler(Rotations[m_nMode]);
                }
                if (m_nMode < Ranges.Length)
                {
                        this.GetComponent<Light>().range = Ranges[m_nMode];
                }
                if (m_nMode < Intensities.Length)
                {
                        this.GetComponent<Light>().intensity = Intensities[m_nMode];
                }

        }
        public void SetMode(int mode)
        {
                int nextMode = Mathf.Clamp(mode, 0, Colors.Length - 1);
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
                                if (m_nMode < Colors.Length && nextMode < Colors.Length)
                                {
                                        this.GetComponent<Light>().color = Color.Lerp(Colors[m_nMode], Colors[nextMode], deltaT);
                                }
                                if (this.GetComponent<Light>().type == LightType.Directional)
                                {
                                        if (m_nMode < Rotations.Length && nextMode < Rotations.Length)
                                        {
                                                this.transform.rotation = Quaternion.Euler(Vector3.Lerp(Rotations[m_nMode], Rotations[nextMode], deltaT));
                                        }
                                }
                                else if (this.GetComponent<Light>().type == LightType.Point)
                                {
                                        if (m_nMode < Ranges.Length && nextMode < Ranges.Length)
                                        {
                                                this.GetComponent<Light>().range = Mathf.Lerp(Ranges[m_nMode], Ranges[nextMode], deltaT);
                                        }
                                }
                                if (m_nMode < Intensities.Length)
                                {
                                        this.GetComponent<Light>().intensity = Mathf.Lerp(Intensities[m_nMode], Intensities[nextMode], deltaT);
                                }
                        }
                        yield return null;
                }
                SetModeImme(nextMode);
        }
}
