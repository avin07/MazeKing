using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;


public class UIUtility
{

        public static Color32 activeColor = new Color32(255, 239, 218, 255);
        public static Color32 unactiveColor = new Color32(59, 36, 4, 255);

        public static string GetTimeString1(int seconds)
        {
                StringBuilder sb = new StringBuilder(64);
                sb.AppendFormat("{0:D2}:{1:D2}:{2:D2}", seconds / 3600, seconds % 3600 / 60, seconds % 60);
                return sb.ToString();
        }

        public static string GetTimeString2(int seconds)
        {
                StringBuilder sb = new StringBuilder(64);
                sb.AppendFormat("{0:D2}小时{1:D2}分", seconds / 3600, seconds % 3600 / 60);
                return sb.ToString();
        }

        ///万雨强留要求的时间显示格式//当时间大于等于1小时时,倒计时改为“XX小时XX分”,2.当时间小于1小时时,倒计时改为“XX分XX秒”3.当时间小于1分钟时,倒计时改为“XX秒”
        public static string GetTimeString3(int seconds) 
        {
                StringBuilder sb = new StringBuilder(64);
                if (seconds >= 3600)
                {
                        sb.AppendFormat("{0}小时{1}分", seconds / 3600, seconds % 3600 / 60);
                }
                else if (seconds > 60)
                {
                        sb.AppendFormat("{0}分{1}秒", seconds % 3600 / 60, seconds % 60);
                }
                else
                {
                        sb.AppendFormat("{0}秒", seconds);
                }
                return sb.ToString();

        }

        static public T FindInParents<T>(GameObject go) where T : Component
        {
                if (go == null) return null;
                var comp = go.GetComponent<T>();

                if (comp != null)
                        return comp;

                Transform t = go.transform.parent;
                while (t != null && comp == null)
                {
                        comp = t.gameObject.GetComponent<T>();
                        t = t.parent;
                }
                return comp;
        }



        #region UI特效
        public static Dictionary<string, GameObject> mUIEffectLib = new Dictionary<string, GameObject>();

        public static void SetUIEffect(string window_name, GameObject elem, bool isshow, string m_effect_name)
        {
                string name = window_name + "|" + elem.name;

                if (mUIEffectLib.ContainsKey(name))
                {
                        if (mUIEffectLib[name] != null)
                        {
                                mUIEffectLib[name].SetActive(isshow);
                        }
                        else
                        {
                                if (isshow)
                                {
                                        CreatUIeffect(elem, name, m_effect_name);
                                        //被删了
                                }
                                else
                                {
                                        //不存在这个特效//
                                }
                        }
                }
                else
                {
                        if (isshow)
                        {
                                CreatUIeffect(elem, name, m_effect_name);
                        }

                }
        }

        static private void CreatUIeffect(GameObject elem, string name, string effectName)
        {
                GameObject effect = EffectManager.GetInst().GetEffectObj("UIEffect/" + effectName);
                if (elem != null && effect != null)
                {
                        effect.transform.SetParent(elem.transform, false);
                        RectTransform elemRT= elem.GetComponent<RectTransform>();

                        RectTransform effectRT = effect.GetComponent<RectTransform>();
                        if (effectRT == null)
                        {
                                effectRT = effect.AddComponent<RectTransform>();
                                effect.transform.localScale = Vector3.one * 100f;
                        }
                        else
                        {
                                effect.transform.localScale = new Vector3(elemRT.sizeDelta.x / effectRT.sizeDelta.x, elemRT.sizeDelta.y / effectRT.sizeDelta.y, 1f) * 100f;
                        }
                        effectRT.anchoredPosition3D = Vector3.zero;
                        GameUtility.SetLayer(effect, LayerMask.LayerToName(16)); //ui特效层//
                }
                if (!mUIEffectLib.ContainsKey(name))
                {
                        mUIEffectLib.Add(name, effect);
                }
        }

        static public void CleanUIEffect(string wndName, string elemName)
        {
                string effectName = wndName + "|" + elemName;
                if (mUIEffectLib.ContainsKey(effectName))
                {
                        GameObject.Destroy(mUIEffectLib[effectName]);
                        mUIEffectLib.Remove(effectName);
                }
        }

        static public void CleanUIEffect(string wndName)
        {
                List<string> UI_Name = new List<string>();
                foreach (string name in mUIEffectLib.Keys)
                {
                        if (name.Split('|')[0].Equals(wndName))
                        {
                                UI_Name.Add(name);
                        }
                }
                for (int i = 0; i < UI_Name.Count; i++)
                {
                        mUIEffectLib.Remove(UI_Name[i]);
                }
        }

        #endregion



        //图片变灰
        static public void SetImageGray(bool is_gray, Transform tf)
        {
                if (tf != null)
                {
                        if (tf.gameObject.activeSelf)
                        {
                                if (tf.GetComponent<Image>() != null)
                                {
                                        if (is_gray)
                                        {
                                                tf.GetComponent<Image>().material = (Material)Resources.Load("Material/UIGray");
                                        }
                                        else
                                        {
                                                tf.GetComponent<Image>().material = null;
                                        }
                                }
                                else if (tf.GetComponent<SpriteRenderer>() != null)
                                {
                                        if (is_gray)
                                        {
                                                tf.GetComponent<SpriteRenderer>().material = (Material)Resources.Load("Material/UIGray");
                                        }
                                        else
                                        {
                                                GameObject help = new GameObject();
                                                SpriteRenderer sr = help.AddComponent<SpriteRenderer>();
                                                tf.GetComponent<SpriteRenderer>().material = sr.sharedMaterial; //不知道怎么使用引擎中默认的sprite材质球，只能使用该方式！
                                                GameObject.Destroy(help);
                                        }
                                }
                        }

                }
        }


        static public void SetGroupGray(bool is_gray, GameObject root)
        {
                Transform[] temps = root.GetComponentsInChildren<Transform>(true);
                for (int i = 0; i < temps.Length; i++ )
                {
                        SetImageGray(is_gray, temps[i]);
                }
        }

        public static Vector2 WorldToCanvas(Canvas canvas, Vector3 world_position, Camera camera = null)
        {
                if (camera == null)
                {
                        camera = Camera.main;
                }

                var viewport_position = camera.WorldToViewportPoint(world_position);
                var canvas_rect = canvas.GetComponent<RectTransform>();

                return new Vector2((viewport_position.x * canvas_rect.sizeDelta.x) - (canvas_rect.sizeDelta.x * 0.5f),
                                   (viewport_position.y * canvas_rect.sizeDelta.y) - (canvas_rect.sizeDelta.y * 0.5f));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rectT"></param>
        /// <param name="worldPos"></param>
        /// <param name="uiCamera">OverlayMode下传null</param>
        /// <param name="outPos"></param>
        public static Vector2 ScenePositionToUIPosition(RectTransform rectT, Vector3 worldPos, Camera worldCamera, Camera uiCamera = null)
        {                
                Vector2 outpos;
                Vector2 obj2Dpos = worldCamera.WorldToScreenPoint(worldPos);
                Debug.Log(worldPos + " " + obj2Dpos);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(rectT, obj2Dpos, uiCamera, out outpos);
                return outpos;
        }
        public static Image GetImage(GameObject root, string name)
        {
                Image[] temps = root.GetComponentsInChildren<Image>(true);
                for (int i = 0; i < temps.Length; i++)
                {
                        if (temps[i].name == name)
                        {
                                return temps[i];
                        }
                }
                return null;
        }
        public static Text GetText(GameObject root, string name)
        {
                Text[] temps = root.GetComponentsInChildren<Text>(true);
                for (int i = 0; i < temps.Length; i++)
                {
                        if (temps[i].name == name)
                        {
                                return temps[i];
                        }
                }
                return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="elemObj"></param>
        /// <param name="di"></param>
        public static void SetupItemElem(GameObject elemObj, DropObject di)
        {
                if (elemObj == null)
                        return;

                if (di != null)
                {
                        string iconname = di.GetIconName();
                        if (!string.IsNullOrEmpty(iconname))
                        {
                                ResourceManager.GetInst().LoadIconSpriteSyn(iconname, GetImage(elemObj, "itemicon").transform);
                                GetText(elemObj, "itemcount").text = di.nOverlap.ToString();

                                Image coverImage = GetImage(elemObj, "itemiconcover");
                                if (coverImage != null)
                                {
                                        coverImage.enabled = true;
                                }

                                string qualityName = di.GetQualityIconName();
                                Image qualityImage = GetImage(elemObj, "itemquality");
                                if (qualityImage != null)
                                {
                                        if (!string.IsNullOrEmpty(qualityName))
                                        {
                                                qualityImage.enabled = true;
                                                ResourceManager.GetInst().LoadIconSpriteSyn(qualityName, qualityImage.transform);
                                        }
                                        else
                                        {
                                                qualityImage.enabled = false;
                                        }
                                }
                        }
                }
                else
                {
                        Image iconImage = GetImage(elemObj, "itemicon");
                        if (iconImage != null)
                        {
                                iconImage.sprite = null;
                                iconImage.enabled = false;
                        }

                        Image coverImage = GetImage(elemObj, "itemiconcover");
                        if (coverImage != null)
                        {
                                coverImage.enabled = false;
                        }

                        Image qualityImage = GetImage(elemObj, "itemquality");
                        if (qualityImage != null)
                        {
                                qualityImage.enabled = false;
                        }

                        GetText(elemObj, "itemcount").text = "";
                }
        }
}
