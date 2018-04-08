using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class RaidBattlePointBehav : MonoBehaviour
{
        public float rotY;
        public Vector3 effect_pos = Vector3.zero;
        public Vector3[] posArray = new Vector3[6];
        public FighterBehav[] fighterArray = new FighterBehav[6];

        public int GetEmptyPos(FighterBehav fighter)
        {
                int ret = -1;
                int startIdx = fighter.IsFront ? 0 : 3;
                for (int i = startIdx; i < startIdx + 3; i++)
                {
                        if (fighterArray[i] == null)
                        {
                                float dist = Vector3.Distance(fighter.transform.position, posArray[i]);
                                if (ret < 0 || dist < Vector3.Distance(fighter.transform.position, posArray[ret]))
                                {
                                        ret = i;
                                }
                        }
                }
                return ret;
        }

        public void ResetPos(bool bFront, int idx, Vector3 pos)
        {
                return;

                int startIdx = bFront ? 0 : 3;

                if (idx % 3 == 1)
                {
                        this.transform.position = pos;
                }
                else if (idx % 3 == 0)
                {
                        this.transform.position = pos - this.transform.right * GlobalParams.GetFloat("battle_position_x_distance");
                }
                else if (idx % 3 == 2)
                {
                        this.transform.position = pos + this.transform.right * GlobalParams.GetFloat("battle_position_x_distance");
                }
                for (int i = 0; i < 6; i++)
                {
                        posArray[i] = GetHeroPosition(i);
                }
        } 
        public void Init()
        {
                for (int i = 0; i < 6; i++)
                {
                        posArray[i] = GetHeroPosition(i);
                        fighterArray[i] = null;
                }
                
        }
        Vector3 GetHeroPosition(int idx)
        {
                Vector3 pos = this.transform.position;
                if (idx % 3 == 2)
                {
                        pos += this.transform.right * GlobalParams.GetFloat("battle_position_x_distance");
                }
                else if (idx % 3 == 0)
                {
                        pos -= this.transform.right * GlobalParams.GetFloat("battle_position_x_distance");
                }

                if (idx / 3 == 1)
                {
                        pos -= this.transform.forward * GlobalParams.GetFloat("battle_position_y_distance");
                }
                
                return pos;
        }



}
