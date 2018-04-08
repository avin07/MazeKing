using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RaidDoor
{

        public int idx0;
        public int idx1;
        public bool m_bEnable = true;//（默认true是因为初始化的时候，房间预设都是门）
        public string key
        {
                get
                {

                        if (idx0 < idx1)
                        {
                                return idx0 + CommonString.underscoreStr + idx1;
                        }
                        else
                        {
                                return idx1 + CommonString.underscoreStr + idx0;
                        }
                }
        }

        public int doorId
        {
                get
                {
                        if (mainNode != null)
                        {
                                return mainNode.id;
                        }
                        return 0;
                }
        }
        public RaidNodeBehav mainNode;

        public RaidDoor(int _idx0, int _idx1)
        {
                idx0 = _idx0;
                idx1 = _idx1;
                //Debug.Log("NewRaidDoor " + idx0 + " " + idx1);
        }
        public Dictionary<int, int> wallMeshDict = new Dictionary<int, int>();
        public RaidElemConfig doorElemCfg = null;
        int GetMeshID(int x, int y, int z)
        {
                return (y * 1000000 + x * 1000 + z);
        }
        public void InitDoorMesh(List<int> wallFloorlist, int rotateDirect)
        {
                Vector3 realSize = mainNode.elemCfg.size;
                bool bXMinus = rotateDirect == 0 || rotateDirect == 3;
                bool bZMinus = rotateDirect == 0 || rotateDirect == 1;
                if (rotateDirect == 1 || rotateDirect == 3)
                {
                        realSize = new Vector3(realSize.z, realSize.y, realSize.x);
                }
                
                for (int x = 0; x < realSize.x; x++)
                {
                        for (int z = 0; z < realSize.z; z++)
                        {
                                for (int y = 0; y < realSize.y; y++)
                                {
                                        int rx = mainNode.posX + x * (bXMinus ? -1 : 1);
                                        int rz = mainNode.posY + z * (bZMinus ? -1 : 1);
                                        int ry = (0 - (int)mainNode.FloorY) + y;
                                        if (ry < wallFloorlist.Count)
                                        {
                                                if (wallFloorlist[ry] > 0)
                                                {
                                                        //BrickSceneManager.GetInst().SetMeshArray(rx, rz, ry);
                                                }
                                                wallMeshDict.Add(GetMeshID(rx, ry, rz), wallFloorlist[ry]);
                                        }
                                }
                        }
                }
        }

        public void EnableDoor(RaidRoomBehav room, bool bEnable)
        {
                //                 if (m_bEnable == bEnable)
                //                         return;
                //m_bEnable = bEnable;
                if (bEnable)
                {
                        if (mainNode.elemCfg != doorElemCfg)
                        {
                                mainNode.elemCfg = doorElemCfg;
                                mainNode.ResetElemObj();
                                mainNode.ShowElemAppear(0f);
                        }
                        foreach (int meshId in wallMeshDict.Keys)
                        {
                                int y = meshId / 1000000;
                                int x = (meshId % 1000000) / 1000;
                                int z = meshId % 1000;
                                BrickSceneManager.GetInst().ClearMeshArray(x, z, y);
                        }
                        foreach (int meshId in wallMeshDict.Keys)
                        {
                                int y = meshId / 1000000;
                                int x = (meshId % 1000000) / 1000;
                                int z = meshId % 1000;
                                //Debuger.Log(x + " " + y + " " + z + " 0 " + mainNode.id);
                                //mainNode.belongRoom.AddMesh(x, z, y, 0);
                                //room.AddMesh(x, z, y, 0);
                                BrickSceneManager.GetInst().SetNodeBrick(x, y, z, 0);
                        }
                }
                else
                {
                        mainNode.elemCfg = null;
                        mainNode.ResetElemObj();
                        foreach (int meshId in wallMeshDict.Keys)
                        {
                                int y = meshId / 1000000;
                                int x = (meshId % 1000000) / 1000;
                                int z = meshId % 1000;
                                BrickSceneManager.GetInst().SetMeshArray(x, z, y);
                        }

                        foreach (int meshId in wallMeshDict.Keys)
                        {
                                int y = meshId / 1000000;
                                int x = (meshId % 1000000) / 1000;
                                int z = meshId % 1000;

                                //mainNode.belongRoom.AddMesh(x, z, y, wallMeshDict[meshId]);
                                BrickSceneManager.GetInst().SetNodeBrick(x, y, z, wallMeshDict[meshId]);
                        }
                }
        }
}