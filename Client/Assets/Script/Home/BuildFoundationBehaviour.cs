//using UnityEngine;
//using DG.Tweening;
//using HighlightingSystem;
//using System;

//public class BuildFoundationBehaviour : BuildBaseBehaviour  //地基建筑
//{
//        GameObject FoundationObj;
//        public override void SetBuild(BuildInfo build_info, bool isNew)
//        {
//                FoundationObj = ModelResourceManager.GetInst().GenerateObject(6602);  //6格大小

//                FoundationObj.transform.localScale = Vector3.one / 6 * HomeManager.GetInst().baseFoundationSize;         
//                FoundationObj.transform.SetParent(transform);

//                gameObject.name = build_info.id.ToString();
//                mBuildInfo = build_info;
//                offsetSize = mBuildInfo.size * 0.5f - 0.5f;
//                GameUtility.SetLayer(gameObject, "InBuildObj");

//                SetPostionByServer();

//                //SetBuildingState(0, 0);
//                transform.localScale = new Vector3(0.1f, 1, 0.1f);
//                transform.DOScale(1, 0.5f);

//                mListener = gameObject.AddComponent<EventTriggerListener>();
//                mListener.onClick = OnBuildClick;
//                mListener.onDrag = OnBuildDarg;
//                mListener.onBeginDrag = OnBeginBuildDrag;
//                mListener.onEndDrag = OnEndBuildDrag;

//                SetMyPlane(build_info.size);
//                mPlane.transform.localPosition = new Vector3(0, 0.05f, 0);     

//                BoxCollider bc = gameObject.AddComponent<BoxCollider>();
//                bc.size = new Vector3(build_info.size, 0.2f, build_info.size);
//                bc.center = new Vector3(0, 0.1f, 0);

//                if (HomeManager.GetInst().GetState() == HomeState.None)
//                {
//                        bc.enabled = true;
//                }
//                else
//                {
//                        bc.enabled = false;
//                }
//        }

//        protected override void UpdateBuildingObjMove()  //统一使用射线检测//
//        {
//                RaycastHit hit = new RaycastHit();
//                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
//                if (Physics.Raycast(ray, out hit, Camera.main.farClipPlane, 1 << LayerMask.NameToLayer("Scene")))
//                {
//                        //转换后的客户端点击位置//
//                        float x = (int)(hit.point.x - offsetSize);
//                        float z = (int)(hit.point.z - offsetSize);
//                        if (x != oldHitPos.x || z != oldHitPos.y)
//                        {
//                                x = Mathf.Clamp(x, 0, HomeManager.HomeSize - mBuildInfo.size);
//                                z = Mathf.Clamp(z, 0, HomeManager.HomeSize - mBuildInfo.size);
//                                int y = HomeManager.GetInst().GetMaxBuildHieght((int)x, (int)z, mBuildInfo.size);
//                                Vector3 pos_client = new Vector3(x + offsetSize, y, z + offsetSize);
//                                Vector2 pos = new Vector2(x, z);
//                                if (transform.position != pos_client)
//                                {
//                                        transform.position = pos_client;
//                                        canPutDown = HomeManager.GetInst().CheckCanPutDown(pos, mBuildInfo);
//                                }
//                                oldHitPos = pos;
//                        }
//                }
//        }

//        protected override Highlighter GetHighlighter()
//        {
//                //Highlighter h = FoundationObj.GetComponent<Highlighter>();
//                //if (h == null)
//                //{
//                //        h = FoundationObj.AddComponent<Highlighter>();
//                //        h.SeeThroughOff();
//                //        h.OccluderOn();
//                //}
//                return null;
//        }

//        protected override void BuildPutDown()
//        {
//                if (canPutDown)
//                {
//                        BuildInfo info = mBuildInfo;
//                        Vector2 old_pos = info.pos;

//                        info.pos_client = new Vector2(transform.position.x, transform.position.z);
//                        info.pos = info.pos_client - Vector2.one * offsetSize;
//                        mBuildInfo = info;    //更新地基信息
//                        SetBuildState(BuildOperateState.CanMove);
//                }
//                HomeManager.GetInst().SetState(HomeState.None);
//        }

//        protected override string GetFunctionStr()
//        {
//                return "7";
//        }

//        public override void PlayCleanEffect()
//        {
//                transform.DOScale(0, 0.5f).OnComplete((() => DeleteFoundation()));
//        }

//        void DeleteFoundation()
//        {
//                Destroy(gameObject);
//                HomeManager.GetInst().RemoveBuildingBehaviour(mBuildInfo.id);
//        }

//        public override void Creat()
//        {
//                int id = HomeManager.GetInst().GetFirstCreatRoom();

//                BuildingInfoHold bih = HomeManager.GetInst().GetBuildInfoCfg(id * 100 + 1);
//                string[] layouts = bih.room_layout.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
//                int index = UnityEngine.Random.Range(0, layouts.Length);
//                string need = bih.level_up_cost;

//                string data = id + CommonString.pipeStr + layouts[index];
//                UIManager.GetInst().ShowUI<UI_NeedConfirm>("UI_NeedConfirm").SetConfirmAndCancel("建造", need, string.Empty, ConfirmCreat, null, data);

//        }

//        void ConfirmCreat(object data)
//        {
//                string []temp = ((string)data).Split('|');
//                HomeManager.GetInst().SendCreatBuild(int.Parse(temp[0]), int.Parse(temp[1]));  //建造空房间//
//        }


//}