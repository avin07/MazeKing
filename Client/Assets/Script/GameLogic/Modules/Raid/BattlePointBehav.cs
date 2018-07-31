using UnityEngine;
using System.Collections;

public class BattlePointBehav : MonoBehaviour 
{

        void OnMouseUp()
        {
                if (InputManager.GetInst().IsPointerOverUgui())
                        return;

        }
}
