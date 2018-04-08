//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine.UI;
//public class UI_BuildingList : UIBehaviour
//{
//        int m_nPage = 1;
//        public Button m_BuildGroup0;
//        public Button m_BuildGroup1;
//        List<GameObject> m_IconList = new List<GameObject>();

//        int m_nCount = 0;
//        Dictionary<string, CustomBuildingConfig> m_IconBuildDict = new Dictionary<string, CustomBuildingConfig>();

//        void Awake()
//        {
//                for (int i = 0; i < 15; i++)
//                {
//                        m_IconList.Add(GetButton("buildGroup" + i).gameObject);
//                }
//        }

//        public void OnClickPage(GameObject go)
//        {
//                int page = int.Parse(go.name.Replace("btnpage", ""));
//                SetPage(page);
//        }

//        GameObject GetIconGroup()
//        {
//                GameObject icongroup;
//                if (m_nCount < m_IconList.Count)
//                {
//                        icongroup = m_IconList[m_nCount];
//                }
//                else
//                {
//                        icongroup = Object.Instantiate(m_BuildGroup0.gameObject) as GameObject;
//                        icongroup.name = "buildGroup0" + m_nCount;
//                        icongroup.transform.SetParent(m_BuildGroup0.transform.parent);
//                        icongroup.transform.localScale = Vector3.one;
//                        m_IconList.Add(icongroup);
//                }
//                return icongroup;
//        }
//        IEnumerator SetIcons()
//        {
//                yield return null;
//                List<CustomBuildingConfig> list = HomelandBuildManager.GetInst().GetBuildingPage();
//                m_nCount = 0;
//                m_IconBuildDict.Clear();

//                foreach (CustomBuildingConfig cbc in list)
//                {
//                        GameObject icongroup = GetIconGroup();
//                        icongroup.SetActive(true);
//                        GetText(icongroup, "buildname").text = cbc.desc;
//                        string iconurl = ModelResourceManager.GetInst().GetIconRes(cbc.model_id);

//                        ResourceManager.GetInst().LoadBuildingIcon(iconurl, GetImage(icongroup, "buildicon").transform);
//                        m_IconBuildDict.Add(icongroup.name, cbc);
//                        m_nCount++;
//                }
//        }
//        public void SetPage(int page)
//        {
//                StartCoroutine(SetIcons());
//        }

//        public void OnClickBuildGroup(GameObject go)
//        {
//                if (m_IconBuildDict.ContainsKey(go.name))
//                {
//                        CustomBuildingConfig cbc = m_IconBuildDict[go.name];
//                        HomelandBuildManager.GetInst().StartPlaceBuilding(cbc);
//                   //     UIManager.GetInst().CloseUI(this.name);
//                }
//        }

//        public void OnClickClose(GameObject go)
//        {
//                UIManager.GetInst().CloseUI(this.name);
//        }

//        public void OnClickSave(GameObject go)
//        {
//                HomelandBuildManager.GetInst().SaveToXml();
//        }
//        public void OnClickLoad(GameObject go)
//        {
//                HomelandBuildManager.GetInst().LoadXml();
//        }

//        public void OnClickHero(GameObject go)
//        {
//                UI_TestHero uis = UIManager.GetInst().ShowUI<UI_TestHero>("UI_TestHero");
//                uis.SetHero(true);
//        }
//}

