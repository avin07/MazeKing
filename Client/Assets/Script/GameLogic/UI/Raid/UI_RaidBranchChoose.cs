using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
public class UI_RaidBranchChoose : UIBehaviour
{
        public GameObject m_Bg;
        public Button m_North;
        public Button m_East;
        public Button m_South;
        public Button m_West;

        public void SetRot(float rot)
        {
                m_Bg.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, rot));
        }

        public void OnClickDirection(GameObject go)
        {
                switch (go.name)
                {
                        case "North":
                                break;
                        case "East":
                                break;
                        case "South":
                                break;
                        case "West":
                                break;
                }
                UIManager.GetInst().CloseUI(this.name);
        }
        public void RemoveDirect(Dictionary<LINK_DIRECTION, int> dict, LINK_DIRECTION initDirect)
        {
                for (LINK_DIRECTION direct = LINK_DIRECTION.NORTH; direct < LINK_DIRECTION.MAX; direct++ )
                {
                        if (direct == initDirect || !dict.ContainsKey(direct))
                        {
                                switch (direct)
                                {
                                        case LINK_DIRECTION.NORTH:
                                                m_North.gameObject.SetActive(false);
                                                break;
                                        case LINK_DIRECTION.EAST:
                                                m_East.gameObject.SetActive(false);
                                                break;
                                        case LINK_DIRECTION.WEST:
                                                m_West.gameObject.SetActive(false);
                                                break;
                                        case LINK_DIRECTION.SOUTH:
                                                m_South.gameObject.SetActive(false);
                                                break;
                                }
                        }
                }
        }
}