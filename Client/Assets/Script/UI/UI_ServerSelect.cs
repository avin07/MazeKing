using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class UI_ServerSelect : UIBehaviour
{
        public InputField m_Account;
        void Awake()
        {
                if (PlayerPrefs.HasKey("MY_ACCOUNT"))
                {
                        m_Account.text = PlayerPrefs.GetString("MY_ACCOUNT");
                }
                else
                {
                        m_Account.text = "zm01";
                }
        }

        public void OnClickServerBtn(InputField tf)
        {
                string ipstr = tf.text;
                
                UIManager.GetInst().CloseUI(this.name);
                LoginManager.GetInst().IP = ipstr;
                if (ipstr.Contains("192.168"))
                {
                        ResourceUpdater.SERVER_RES_IP = "192.168.0.104";
                }
                else
                {
                        ResourceUpdater.SERVER_RES_IP = ipstr;
                }
                if (!string.IsNullOrEmpty(m_Account.text))
                {
                        LoginManager.GetInst().m_sAccount = m_Account.text;
                        LoginManager.GetInst().m_sName = m_Account.text;
                        PlayerPrefs.SetString("MY_ACCOUNT", m_Account.text);
                }

                ResourceUpdater.GetInst().StartUpdateRes();  //手机上使用热更流程//
                //GameObject.Destroy(this.gameObject);
        }
}
