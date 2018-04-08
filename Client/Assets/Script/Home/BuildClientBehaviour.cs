using UnityEngine;
using UnityEngine.EventSystems;

public class BuildClientBehaviour : BuildBaseBehaviour  //客户端本地建造建筑//
{
        UI_BuildCreatConfirm mBuildCreatConfirm;
        public override void SetBuild(BuildInfo build_info)
        {
                Debug.Log("SetBuild " + transform.position);
                GameUtility.SetLayer(gameObject, "TestObj");
                

                mBuildCreatConfirm = UIManager.GetInst().ShowUI<UI_BuildCreatConfirm>("UI_BuildCreatConfirm");
                mBuildCreatConfirm.SetConfirmAndCancel(LanguageManager.GetText(build_info.buildCfg.name), Creat, Cancel, null);
                mBuildCreatConfirm.SetForFurnitureCreat();

                SetBuildInfo(build_info);
                GetModel(build_info, true);
                SetBuildOperateState(BuildOperateState.CanMove);
                canPutDown = HomeManager.GetInst().CheckCanPutDown(mBuildInfo.pos, mBuildInfo);

                mPlane.SetActive(true);
                UpdateHighlightColor();
        }


        void Creat(object data)
        {
                if (canPutDown)
                {

                        int pos = (int)(transform.position.x - mBuildInfo.offset.x) * 100 + (int)(transform.position.z - mBuildInfo.offset.y);
                        HomeManager.GetInst().SendCreatBuild(mBuildInfo.buildId, pos);
                        //string need = mBuildInfo.buildCfg.cost_material; //建造花费
                        //UIManager.GetInst().ShowUI<UI_NeedConfirm>("UI_NeedConfirm").SetConfirmAndCancel("建造", need, string.Empty, ConfirmCreat, null, null);
                }
                else
                {
                        GameUtility.PopupMessage("这里不能放置该家具！");
                }
        }

        void ConfirmCreat(object data)
        {
                int pos = (int)(transform.position.x - mBuildInfo.offset.x) * 100 + (int)(transform.position.z - mBuildInfo.offset.y);
                HomeManager.GetInst().SendCreatBuild(mBuildInfo.buildId, pos);
        }


        void Cancel(object data)
        {
                HomeManager.GetInst().QuitCreatMode();               
        }

        void Update()
        {
                if (mBuildCreatConfirm != null)
                {
                        mBuildCreatConfirm.SetPosition(transform.position);
                        mBuildCreatConfirm.SetOKInteractable(canPutDown);
                }               
        }


        public override void OnBuildClick(GameObject go, PointerEventData data)
        {

        }


        public override void OnBeginBuildDrag(GameObject go, PointerEventData data)
        {
                HomeManager.GetInst().SetState(HomeState.Move);
        }

        public override void OnEndBuildDrag(GameObject go, PointerEventData data)
        {
                HomeManager.GetInst().SetState(HomeState.FurnitureCreat);
        }

}
