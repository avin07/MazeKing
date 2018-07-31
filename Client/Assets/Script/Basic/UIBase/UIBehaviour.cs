using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UIBehaviour : MonoBehaviour
{


        public enum UI_LEVEL
        {
                MAIN,
                NORMAL,
                TIP,
        };

        public UI_LEVEL UILevel = UI_LEVEL.NORMAL;

        public bool IsFullScreen = false;

        protected Button GetButton(string name)
        {
                return GetButton(this.gameObject, name);
        }

        protected Button GetButton(GameObject root, string name)
        {
                Button[] temps = root.GetComponentsInChildren<Button>(true);

                for (int i = 0; i < temps.Length; i++)
                {
                        if (temps[i].name == name)
                        {
                                return temps[i];
                        }
                }
                return null;
        }

        protected Image GetImage(GameObject root, string name)
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

        protected Image GetImage(string name)
        {
                return GetImage(this.gameObject, name);
        }

        protected Text GetText(GameObject root, string name)
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

        protected Text GetText(string name)
        {
                return GetText(this.gameObject, name);
        }

        protected InputField GetInputField(GameObject root, string name)
        {
                InputField[] temps = root.GetComponentsInChildren<InputField>(true);
                for (int i = 0; i < temps.Length; i++)
                {
                        if (temps[i].name == name)
                        {
                                return temps[i];
                        }
                }
                return null;
        }

        protected InputField GetInputField(string name)
        {
                return GetInputField(this.gameObject, name);
        }

        protected Slider GetSlider(string name)
        {
                return GetSlider(this.gameObject, name);
        }

        protected Slider GetSlider(GameObject root, string name)
        {
                Slider[] temps = root.GetComponentsInChildren<Slider>(true);
                for (int i = 0; i < temps.Length; i++)
                {
                        if (temps[i].name == name)
                        {
                                return temps[i];
                        }
                }
                return null;
        }

        protected Toggle GetToggle(string name)
        {
                return GetToggle(this.gameObject, name);
        }

        protected Toggle GetToggle(GameObject root, string name)
        {
                Toggle[] temps = root.GetComponentsInChildren<Toggle>(true);
                for (int i = 0; i < temps.Length; i++)
                {
                        if (temps[i].name == name)
                        {
                                return temps[i];
                        }
                }
                return null;
        }


        protected GameObject GetGameObject(GameObject root, string name)
        {
                Transform[] temps = root.GetComponentsInChildren<Transform>(true);
                for (int i = 0; i < temps.Length; i++)
                {
                        if (temps[i].name == name)
                        {
                                return temps[i].gameObject;
                        }
                }
                return null;
        }

        protected GameObject GetGameObject(string name)
        {
                return GetGameObject(this.gameObject, name);
        }


        protected Canvas GetCanvas(string name)
        {
                Canvas[] temps = this.gameObject.GetComponentsInChildren<Canvas>(true);
                for (int i = 0; i < temps.Length; i++)
                {
                        if (temps[i].name == name)
                        {
                                return temps[i];
                        }
                }
                return null;
        }

        protected T FindComponent<T>(Transform root, string name) where T : Component
        {
                Transform tf = root.Find(name);
                if (tf != null)
                {
                        return tf.GetComponent<T>();
                }
                return null;
        }

        protected T FindComponent<T>(GameObject root, string name) where T : Component
        {
                Transform tf = root.transform.Find(name);
                if (tf != null)
                {
                        return tf.GetComponent<T>();
                }
                return null;
        }


        protected T FindComponent<T>(string name) where T : Component
        {
                return FindComponent<T>(this.transform, name);
        }


        protected void SetGameObjectHide(string name, GameObject root, int num = 999)
        {
                for (int i = 0; i < num; i++)
                {
                        GameObject temp = GetGameObject(root, name + i);
                        if (temp != null)
                        {
                                temp.SetActive(false);
                        }
                        else
                        {
                                break;
                        }
                }
        }

        protected void SetChildActive(Transform root, bool isShow)
        {
                for (int i = 0; i < root.childCount; i++)
                {
                        root.GetChild(i).SetActive(isShow);
                }
        }

        protected RectTransform GetChildByIndex(Transform root, int index)
        {
                if (index < root.childCount)
                {
                        return root.GetChild(index) as RectTransform;
                }
                return null;
        }


        public virtual void RefreshCurWnd(object data)
        {
        }

        public bool IsVisible()
        {
                Canvas canvas = GetComponent<Canvas>();
                if (!canvas.enabled)
                {
                        return canvas.enabled;
                }
                else
                {
                        return gameObject.activeSelf;
                }

        }


        public virtual void OnClose(float time)
        {
                if (this != null)
                {
                        if (GetComponent<CanvasGroup>() != null && gameObject.activeSelf)
                        {
                                //StartCoroutine(AlphaAnimClose(time));
                        }
                        else
                        {
                                //CleanUI();
                        }
                        CleanUI();

                        GuideManager.GetInst().CheckUIGuideClose(this.gameObject);
                }              
        }

        void CleanUI()
        {
                UIManager.GetInst().RemoveUI(this.name);
                this.enabled = false;
                GameObject.Destroy(gameObject);  //窗口关闭延时在特殊需求窗口处理，不在底层处理！
                UIUtility.CleanUIEffect(this.name);

        }

        protected Image fullScreenImage;
        public void SetupFullScreen(float alpha = 0.5f)
        {
                if (this.gameObject.GetComponent<Image>() == null)
                {
                        fullScreenImage = this.gameObject.AddComponent<Image>();
                        fullScreenImage.color = new Color(0f, 0f, 0f, alpha);
                }
        }

        public virtual void OnShow(float time)
        {
                if (Application.loadedLevelName != "RaidEditor")
                {
                        SetUICamera();
                }
#if UNITY_EDITOR
                if (Application.loadedLevelName != "RaidEditor")
                {
                        ResolutionRatio();
                }
#else
                ResolutionRatio();
#endif
                SetFont();
                if (IsFullScreen)
                {
                        SetupFullScreen();
                }

                //*********处理某些情况下实例化后第一帧不被绘制***********
                //if (use_scaler_anim)
                //{
                //        AppMain.GetInst().StartCoroutine(ScalerAnimShow(time));
                //}
                //else
                //{
                //        AppMain.GetInst().StartCoroutine(AlphaAnimShow(time));
                //}
                if (time > 0)
                {
                        AppMain.GetInst().StartCoroutine(AlphaAnimShow(time));
                }
                GuideManager.GetInst().CheckUIGuide(this.name);
        }

        void SetFont()
        {

                Text [] temps = gameObject.GetComponentsInChildren<Text>(true);

                for (int i = 0; i < temps.Length; i++ )
                {
                        if (temps[i].CompareTag("Finish")) //此处偷懒用这个tag和ui工程中的数字字体同步标记//
                        {
                                temps[i].font = UIManager.GetInst().LithographFont;
                        }
                        else
                        {
                                temps[i].font = UIManager.GetInst().MicrosoftFont;
                        }
                }
        }

        public IEnumerator AlphaAnimShow(float last_time)
        {
                if (last_time > 0f)
                {
                        CanvasGroup cg = GetComponent<CanvasGroup>();
                        if (cg == null)
                        {
                                cg = gameObject.AddComponent<CanvasGroup>();
                        }

                        float time = Time.realtimeSinceStartup;
                        float delt_alph;
                        while (Time.realtimeSinceStartup - time <= last_time)
                        {
                                if (this != null)
                                {
                                        delt_alph = Mathf.Lerp(0, 1, (Time.realtimeSinceStartup - time) / last_time);
                                        cg.alpha = delt_alph;
                                        yield return null;
                                }
                        }

                        if (this != null)
                        {
                                cg.alpha = 1.0f;
                        }     
                }           
        }

        public IEnumerator ScalerAnimShow(float last_time)
        {
                float time = Time.realtimeSinceStartup;
                float delt_alph;
                while (Time.realtimeSinceStartup - time <= last_time)
                {
                        if (this != null)
                        {
                                delt_alph = Mathf.Lerp(0, 1, (Time.realtimeSinceStartup - time) / last_time);
                                GetComponent<Canvas>().scaleFactor = delt_alph;
                                yield return null;
                        }

                }
                if (this != null)
                {
                        GetComponent<Canvas>().scaleFactor = 1.0f;
                }

        }

        public IEnumerator AlphaAnimClose(float last_time)
        {
                float time = Time.realtimeSinceStartup;
                float delt_alph;
                while (Time.realtimeSinceStartup - time <= last_time)
                {
                        delt_alph = Mathf.Lerp(1, 0, (Time.realtimeSinceStartup - time) / last_time);
                        GetComponent<CanvasGroup>().alpha = delt_alph;
                        yield return null;
                }
                GetComponent<CanvasGroup>().alpha = 0.0f;
                CleanUI();
        }

        public GameObject CloneElement(GameObject oriElem, string name = "")
        {
                return CloneElement(oriElem, oriElem.transform.parent, name);
        }

        public GameObject CloneElement(GameObject oriElem, Transform parentObj, string name)
        {
                GameObject elem;
                elem = GameObject.Instantiate(oriElem) as GameObject;
                elem.name = name;               
                elem.transform.SetParent(parentObj);
                elem.transform.localScale = oriElem.transform.localScale;
                elem.transform.localPosition = oriElem.transform.localPosition;
                elem.transform.localRotation = oriElem.transform.localRotation;
                EventTriggerListener oriListener = oriElem.GetComponent<EventTriggerListener>();

                if (oriListener != null)
                {
                        EventTriggerListener.Get(elem).onEnter = oriListener.onEnter;
                        EventTriggerListener.Get(elem).onExit = oriListener.onExit;
                        
                }
                elem.SetActive(true);
                return elem;
        }

        public virtual void OnClickClose(GameObject go)
        {
                UIManager.GetInst().CloseUI(this.name);
                AudioManager.GetInst().PlaySE("SE_Cancel");
        }

        public void ResolutionRatio()  //分辨率适配方案
        {
                CanvasScaler canvascaler = GetComponent<CanvasScaler>();
                canvascaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvascaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                canvascaler.matchWidthOrHeight = 1.0f;
                canvascaler.referenceResolution = new Vector2(1136, 640);
        }

        public void SetUICamera()
        {
                Canvas canvas = GetComponent<Canvas>();
                canvas.worldCamera = CameraManager.GetInst().UI_Camera;              
        }

        public void SetAlpha(float alpha)
        {
                if (GetComponent<CanvasGroup>() != null)
                {
                        GetComponent<CanvasGroup>().alpha = alpha;
                }
        }

        //星级显示
        public void SetStar(Pet pet,Transform star_root)
        {
                int now_star = pet.GetPropertyInt("star");
                int max_star = pet.GetPropertyInt("max_star");
                SetChildActive(star_root, false);
                for (int i = 0; i < max_star; i++)
                {
                        Transform m_star = null;
                        if (i < star_root.childCount)
                        {
                                m_star = star_root.GetChild(i);
                        }
                        else
                        {
                                m_star = CloneElement(star_root.GetChild(0).gameObject).transform;
                        }
                        m_star.SetActive(true);
                        if (i < now_star)
                        {
                                UIUtility.SetImageGray(false, m_star);
                        }
                        else
                        {
                                UIUtility.SetImageGray(true, m_star);
                        }
                }
        }

        //显示压力
        public void SetPressure(int pressure, GameObject root)
        {
                int count = pressure / 10;
                for (int i = 0; i < 20; i++)
                {
                        GetImage(root, "pressure" + i).enabled = i < count;
                }
        }

        public Vector3 CalcScreenPosition(RectTransform rt)
        {
                Canvas canvas = this.GetComponent<Canvas>();
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                        return rt.position;
                }
                else if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
                {
                        return canvas.worldCamera.WorldToScreenPoint(rt.position);
                }
                else
                {
                        return Camera.main.WorldToScreenPoint(rt.position);
                }
                return rt.position;
        }

}
