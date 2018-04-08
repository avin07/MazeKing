using UnityEngine;
using System.Collections;
public class SetRenderQueue : MonoBehaviour
{
        public int _mRendererQueue;
        public int mRenderQueue
        {
                get { return _mRendererQueue; }
                set
                {
                        _mRendererQueue = value;
                        _Update();
                }
        }
        public void SetValue(int value)
        {
                _mRendererQueue = value;
                _Update();
        }
        Renderer[] mRd;
        void Start()
        {
                //	enabled = false;
                mRd = GetComponentsInChildren<Renderer>();
                //	GameObject.Destroy(this.gameObject);
                _Update();
        }
        void _Update()//作为跳转点特效时  内存一直增长 所以暂停这个效果
        {
                //return;
                if (mRd != null && mRd.Length > 0)
                {
                        foreach (Renderer tmpR in mRd)
                        {
                                if (tmpR != null && tmpR.sharedMaterials != null && tmpR.sharedMaterials.Length > 0)
                                {
                                        foreach (Material tmpM in tmpR.sharedMaterials)
                                        {
                                                if (tmpM == null)
                                                        continue;
                                                tmpM.renderQueue = tmpM.shader.renderQueue + _mRendererQueue;
                                                //Debuger.Log("Material : " + tmpM.name + "renderQueue=" + tmpM.renderQueue);
                                        }
                                }
                        }
                }

        }


}