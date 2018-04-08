using UnityEngine;
using System.Collections;
using UnityEngine.UI;
class UI_ResourceLoading : UIBehaviour
{
        public Image m_UIProgress;
        public Text m_Text;

        int m_nMax = 0;
        int m_nNow = 0;

        string prefixStr = "";
        void Awake()
        {
                m_Text.text = "Loading...";
        }
        public void SetLocal(bool bLocal)
        {
                prefixStr = bLocal ? "解压安装包" : "更新服务器";
        }

        public void SetProgressMax(int max, bool bLocal = false)
        {
                SetLocal(bLocal);
                m_nNow = 0;
                m_nMax = max;

                m_Text.text = m_nNow + CommonString.divideStr + m_nMax;
        }
        public void AddProgress(int offset, string filename)
        {
                m_nNow += offset;
                m_UIProgress.rectTransform.sizeDelta = new Vector2(m_nNow / (float)m_nMax * 1000f, m_UIProgress.rectTransform.sizeDelta.y);
                int percent= m_nNow * 100 / m_nMax;
                m_Text.text = prefixStr + "：" + percent + "%";
        }
}
