using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class FormationUnitBehav : MonoBehaviour, IPointerClickHandler
{
        public Pet m_Pet;
        public GameObject PetBtnObj;
        public void OnPointerClick(PointerEventData data)
        {
                UI_RaidFormation uis = UIManager.GetInst().GetUIBehaviour<UI_RaidFormation>();
                if (uis != null)
                {
                        uis.StartDragUnit(this);
                }
        }
}
