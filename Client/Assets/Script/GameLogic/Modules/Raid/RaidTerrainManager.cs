using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class RaidTerrainManager : MonoBehaviour, IPointerClickHandler,IDragHandler
{
        public void OnPointerClick(PointerEventData data)
        {
                if (GameStateManager.GetInst().GameState == GAMESTATE.RAID_PLAYING)
                {
                        RaidManager.GetInst().OnClickPosition(data.pointerCurrentRaycast.worldPosition);
                }
        }
        public void OnDrag(PointerEventData eventData)
        {
                if (GameStateManager.GetInst().GameState == GAMESTATE.RAID_PLAYING)
                {
//                         if (Vector3.Distance(RaidManager.GetInst().MainHero.transform.position, eventData.pointerCurrentRaycast.worldPosition) >= 1f)
//                         {
//                                 RaidManager.GetInst().OnClickPosition(eventData.pointerCurrentRaycast.worldPosition);
//                         }
                }
        }

        GameObject m_BlockRoot;
        GameObject BlockRoot
        {
                get
                {

                        if (m_BlockRoot == null)
                        {
                                m_BlockRoot = new GameObject();
                                m_BlockRoot.transform.SetParent(this.transform);
                                m_BlockRoot .transform.position = Vector3.zero;
                                m_BlockRoot.transform.rotation = Quaternion.identity;
                                m_BlockRoot.transform.localScale = new Vector3(1f / this.transform.localScale.x, 1f / this.transform.localScale.y, 1f / this.transform.localScale.z);
                                GameUtility.SetLayer(m_BlockRoot, "BlockObj");
                        }
                        return m_BlockRoot;
                }
        }
        GameObject m_NonBlockRoot;
        GameObject NonBlockRoot
        {
                get
                {

                        if (m_NonBlockRoot == null)
                        {
                                m_NonBlockRoot = new GameObject();
                                m_NonBlockRoot.transform.SetParent(this.transform);
                                m_NonBlockRoot.transform.position = Vector3.zero;
                                m_NonBlockRoot.transform.rotation = Quaternion.identity;
                                m_NonBlockRoot.transform.localScale = new Vector3(1f / this.transform.localScale.x, 1f / this.transform.localScale.y, 1f / this.transform.localScale.z);
                                GameUtility.SetLayer(m_NonBlockRoot, "NonBlockObj");
                        }
                        return m_NonBlockRoot;
                }
        }

        Dictionary<int, BoxCollider> m_ColliderDict = new Dictionary<int, BoxCollider>();
        public void AddBoxCollider(int id, Vector3 size, bool bBlock)
        {
                BoxCollider collider;
                if (!m_ColliderDict.ContainsKey(id))
                {
                        if (bBlock)
                        {
                                collider = BlockRoot.AddComponent<BoxCollider>();
                        }
                        else
                        {
                                collider = NonBlockRoot.AddComponent<BoxCollider>();
                        }
                        m_ColliderDict.Add(id, collider);
                }
                else
                {
                        collider = m_ColliderDict[id];
                }
                collider.size = size;
                collider.center = new Vector3(id / 100 + size.x / 2f - 0.5f, size.y / 2f - 1f, id % 100 + size.z / 2f - 0.5f);
        }
}
