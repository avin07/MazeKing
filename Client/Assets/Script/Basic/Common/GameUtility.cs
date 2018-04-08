using UnityEngine;
using System.Collections;
using Pathfinding;
using SevenZip.Compression.LZMA;
using System.IO;
using System;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using DG.Tweening;
using System.Text;
using ICSharpCode.SharpZipLib.GZip;

public static class GameUtility 
{
        public static GameObject GetGameObject(GameObject rootObj, string name)
        {
                Transform t = GetTransform(rootObj, name);
                if (t != null)
                {
                        return t.gameObject;
                }
                return null;
        }

        public static Transform GetTransform(GameObject obj, string name)
        {
                Transform[] trans = obj.GetComponentsInChildren<Transform>(true);
                for (int i = 0; i < trans.Length; i++)
                {
                        if (trans[i].name == name)
                        {
                                return trans[i];
                        }
                }
                return null;
        }

        public static void SetLayer(GameObject obj, string layerName)
        {
                if (obj == null)
                {
                        return;
                }
                int layer = 0;
                Transform[] trans = obj.GetComponentsInChildren<Transform>(true);

                for (int i = 0; i < trans.Length; i++)
                {
                        layer = LayerMask.NameToLayer(layerName);
                        if (layer < 0)
                        {
                                singleton.GetInst().ShowMessage("不存在层" + layerName);
                                return;
                        }
                        trans[i].gameObject.layer = layer;
                }
        }

        public static void SetLayer(GameObject obj, int layer)
        {
                if (obj == null)
                {
                        return;
                }
                Transform[] trans = obj.GetComponentsInChildren<Transform>(true);

                for (int i = 0; i < trans.Length; i++)
                {
                        trans[i].gameObject.layer = layer;
                }
        }
        
        public static void SetColor(GameObject obj, Color color)
        {
                if (obj == null)
                {
                        return;
                }
                Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
                for (int i = 0; i < renderers.Length; i++ )
                {
                        renderers[i].material.color = color;
                }
        }

        public static float GetAnimTime(GameObject obj, string animName)
        {
                if (obj != null)
                {
                        Animation anim = obj.GetComponentInChildren<Animation>();
                        if (anim != null)
                        {
                                if (anim.GetClip(animName) != null)
                                {
                                        return anim[animName].length;
                                }
                        }
                }
                return 0f;
        }
        public static void ObjStopAnim(GameObject obj)
        {
                Animation anim = obj.GetComponentInChildren<Animation>();
                if (anim != null)
                {
                        anim.Stop();
                }
        }

        public static void ObjPlayAnim(GameObject obj, string animName, bool bLoop, float speed = 1.0f)
        {
                ObjPlayAnim(obj, animName, bLoop, "", speed);
        }

        public static void ObjPlayAnim(GameObject obj, string animName, bool bLoop, string nextAnim,float speed = 1.0f)
        {
                if (obj == null)
                        return;

                Animation anim = obj.GetComponentInChildren<Animation>();
                if (anim != null && anim.GetClip(animName) != null)
                {
                        if (!anim.IsPlaying(animName))
                        {
                                anim.wrapMode = bLoop ? WrapMode.Loop : WrapMode.Once;
                                anim[animName].speed = speed;

                                anim.Play(animName);
                                //Debuger.Log(anim.transform.name + " " + animName + " " + anim[animName].length + " " + nextAnim);
                                if (!string.IsNullOrEmpty(nextAnim))
                                {
                                        anim[nextAnim].speed = speed;
                                        anim.CrossFadeQueued(nextAnim);
                                        //Debuger.Log(nextAnim);
                                }
                        }
                }
        }
        public static void RewindAnim(GameObject obj, string animName)
        {
                if (obj == null)
                        return;

                Animation anim = obj.GetComponentInChildren<Animation>();
                if (anim != null && anim.GetClip(animName) != null)
                {
                        anim.Stop();
                        anim.wrapMode = WrapMode.Once;
                        anim[animName].time = anim[animName].length;
                        anim[animName].speed = -1f;
                        anim.Play(animName);
                        //Debuger.Log("RewindAnim " + animName);
                }
        }

        public static Vector3 GetCameraFocusPoint()
        {
                RaycastHit hit = new RaycastHit();
                if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit))
                {
                        return new Vector3(hit.point.x, hit.point.y + 0.01f, hit.point.z);
                }
                return Vector3.zero;
        }

        public static void RotateTowards(Transform from, Vector3 targetPosition)
        {
                if (from == null)
                {
                        return;
                }

                Vector3 dir = targetPosition - from.transform.position;
                Quaternion rotation = Quaternion.LookRotation(dir);
                rotation.eulerAngles = new Vector3(0f, rotation.eulerAngles.y, 0f);
                from.transform.rotation = rotation;
        }

        public static void RotateTowards(Transform  from, Transform target)
        {
                RotateTowards(from, target.position);
        }
        static bool g_bRescanPath = false;
        public static void ReScanPath()
        {
                if (!GameStateManager.GetInst().IsGameCanScanPath())
                {
                        return;
                }
                if (g_bRescanPath == false)
                {
                        g_bRescanPath = true;

                        AppMain.GetInst().StartCoroutine(WaitScan());
                }
        }
        static IEnumerator WaitScan()
        {
                yield return null;
                if (AstarPath.active != null)
                {
                    AstarPath.active.Scan();
                    g_bRescanPath = false;
                }
        }

        public static void UpdateGraph(GameObject obj, int tag)
        {
                BoxCollider box = obj.GetComponentInChildren<BoxCollider>();
                if (box != null)
                {// Update the graph below the door
                        // Set the tag of the nodes below the door
                        // To something indicating that the door is open or closed
                        GraphUpdateObject guo = new GraphUpdateObject(box.bounds);

                        guo.modifyTag = true;
                        guo.setTag = tag;
                        guo.updatePhysics = false;

                        AstarPath.active.UpdateGraphs(guo);
                        //Debuger.Log("UpdateGraph " + obj.name + " tag= " + tag);
                }
        }

        public static void BindEffect(string effectName, GameObject obj)
        {
                if (!string.IsNullOrEmpty(effectName))
                {
                        GameObject effectobj = EffectManager.GetInst().GetEffectObj(effectName);
                        if (effectobj != null)
                        {
                                effectobj.transform.SetParent(obj.transform);
                                effectobj.transform.localPosition = Vector3.zero;
                                //Debuger.Log("BindEffect " + effectName);
                        }
                }
        }
        public static void PopupMessage(string text)
        {
                UIManager.GetInst().ShowUI<UI_MessageBox>("UI_MessageBox").SetText(text);
        }


        public static void AddNotify(NotifyType type, Action<object> action, string des,object data)
        {
                UI_HomeMain uim = UIManager.GetInst().GetUIBehaviour<UI_HomeMain>();
                if (uim != null)
                {
                        uim.ShowNewTip(type, action,des, data);
                }
        }

        public static void ShowTip(string text)
        {
                if (string.IsNullOrEmpty(text))
                        return;
                UI_RaidTip uis = UIManager.GetInst().ShowUI<UI_RaidTip>("UI_RaidTip");
                if (uis != null)
                {
                        uis.ShowText(text);
                }
        }
        public static void ShowTipAside(string text)
        {
                UIManager.GetInst().ShowUI<UI_RaidAside>("UI_RaidAside").SetText(text);
        }

        //判断两个矩形是否相交,不考虑旋转（左下角） API  Rect.Overlaps()没有测试过//
        static public bool IsRectOverlaps(Vector2 pointA, Vector2 sizeA, Vector2 pointB, Vector2 sizeB)
        {
                Vector2 centerA = pointA + new Vector2(sizeA.x * 0.5f, sizeA.y * 0.5f);
                Vector2 centerB = pointB + new Vector2(sizeB.x * 0.5f, sizeB.y * 0.5f);
                float delta_x = Mathf.Abs(centerA.x - centerB.x);
                float delta_y = Mathf.Abs(centerA.y - centerB.y);

                if (delta_x < (sizeA.x + sizeB.x) * 0.5f)
                {
                        if (delta_y < (sizeA.y + sizeB.y) * 0.5f)
                        {
                                return true;
                        }
                }
                return false;
        }

        //判断一个矩形是否完全在一个矩形内
        static public bool IsRectAllInRect(Vector2 pointA, Vector2 sizeA, Vector2 pointB, Vector2 sizeB) //第一个大第二个小
        {
                if (pointB.x >= pointA.x && pointB.y >= pointA.y && sizeB.x + pointB.x <= sizeA.x + pointA.x && sizeB.y + pointB.y <= sizeA.y + pointA.y)
                {
                        return true;
                }
                return false;
        }

        public static int ClearFlagBit(int flag, int idx)
        {
                int bit = 1 << idx;
                int nMark = 0;
                nMark = (~nMark) ^ bit;
                flag &= nMark;
                return flag;
        }

        public static int GetFlagValInt(int idx)
        {
                return ((int)1) << idx;
        }

        public static ulong GetFlagVal(int idx)
        {
                return ((ulong)1) << idx;
        }

        public static bool IsFlagOn(int flag0, int idx)
        {
                return (flag0 & GetFlagValInt(idx)) != 0;
        }

        public static bool IsFlagOn(ulong flag0, int idx)
        {
                return (flag0 & GetFlagVal(idx)) != 0;
        }
        /// <summary>
        /// 6 7 8       15 16 17
        /// 3 4 5       12 13 14
        /// 0 1 2         9 10 11
        /// n:7 3:w 1:s 5:e
        /// 
        /// </summary>
        /// <param name="bit"></param>
        /// <returns></returns>
        public static string ConvertLinkBit(ulong bit)
        {
                StringBuilder sb = new StringBuilder(6);
                if (!IsFlagOn(bit, 7))
                {
                        sb.Append("n");
                }
                if (!IsFlagOn(bit, 3))
                {
                        sb.Append("w");
                }
                if (!IsFlagOn(bit, 1))
                {
                        sb.Append("s");
                }
                if (!IsFlagOn(bit, 5))
                {
                        sb.Append("e");
                }
                if (!IsFlagOn(bit, 4))
                {
                        sb.Append("_t");
                }
                return sb.ToString();
        }

        public static void DecompressFileLZMA(string inFile, string outFile)
        {
                SevenZip.Compression.LZMA.Decoder coder = new SevenZip.Compression.LZMA.Decoder();
                FileStream input = new FileStream(inFile, FileMode.Open);
                FileStream output = new FileStream(outFile, FileMode.Create);

                // Read the decoder properties
                byte[] properties = new byte[5];
                input.Read(properties, 0, 5);

                // Read in the decompress file size.
                byte[] fileLengthBytes = new byte[8];
                input.Read(fileLengthBytes, 0, 8);
                long fileLength = BitConverter.ToInt64(fileLengthBytes, 0);

                // Decompress the file.
                coder.SetDecoderProperties(properties);
                coder.Code(input, output, input.Length, fileLength, null);
                Debug.Log(inFile);
                output.Flush();
                output.Close();
                input.Close();
        }

        public static int GetPropInt(string[] props, int index)
        {
                int ret = 0;
                if (props.Length > index)
                {
                        int.TryParse(props[index], out ret);                        
                }
                return ret;
        }

        public static void SetAstarParam()
        {
                GridGraph gg = AstarPath.active.astarData.gridGraph;
                GraphCollision collision = gg.collision;
                switch (GameStateManager.GetInst().GameState)
                {
                        case GAMESTATE.RAID_PLAYING:
                                gg.width = 128;
                                gg.depth = 128;
                                gg.nodeSize = 1;
                                gg.center = new Vector3(63.5f, 0, 63.5f);
                                gg.neighbours = NumNeighbours.Four;
                                gg.UpdateSizeFromWidthDepth();
                                collision.heightMask = 1 << LayerMask.NameToLayer("NonBlockObj") /*| 1 << LayerMask.NameToLayer("SpotNonBlockObj")*/;
                                collision.diameter = 0.8f;
                                collision.heightCheck = true;
                                collision.height = 1f;
                                collision.type = ColliderType.Sphere;
                                collision.mask = 1 << LayerMask.NameToLayer("BlockObj") | 1 << LayerMask.NameToLayer("NpcObj");
                                break;
                        case GAMESTATE.HOME:
                                gg.width = 50;
                                gg.depth = 50;
                                gg.nodeSize = 1;
                                gg.center = new Vector3(24.5f, 0, 24.5f);
                                gg.neighbours = NumNeighbours.Eight;
                                gg.UpdateSizeFromWidthDepth();

                                collision.heightCheck = true;
                                collision.heightMask = 1 << LayerMask.NameToLayer("Scene") /* | 1 << LayerMask.NameToLayer("BlockObj") */;
                                collision.height = 16f;
                                collision.diameter = 0.8f;
                                collision.type = ColliderType.Capsule;
                                collision.mask = 1 << LayerMask.NameToLayer("BlockObj") | 1 << LayerMask.NameToLayer("Water");
                                break;
                }
        }

        public static UI_CombatDamage AddDamageUI(Transform parent, SkillResultData skillResult, float posY = 0f)
        {
                GameObject uiObj = UIManager.GetInst().ShowUI_Multiple<UI_CombatDamage>("UI_CombatDamage");
                //uiObj.transform.SetParent(parent);
                //uiObj.transform.localScale = Vector3.one / 50f;
                Vector3 worldPos = parent.position+ new UnityEngine.Vector3(0f, posY, 0f);
                GameUtility.SetLayer(uiObj, "UI");
                UI_CombatDamage uis_damage = uiObj.GetComponent<UI_CombatDamage>();
                uis_damage.ShowNum(skillResult, GlobalParams.GetFloat("damage_show_time")/ 1000f, worldPos);
                return uis_damage;
        }

        public static void DoCameraShake(Camera cam, float time = 0.5f)
        {
                CameraShake cs = cam.gameObject.GetComponent<CameraShake>();
                if (cs == null)
                {
                        cs = cam.gameObject.AddComponent<CameraShake>();
                }
                cs.MaxTime = time;
        }


        public static float EnlargeSpeedByDistance(float oDistance, float nDistance, float speed)
        {
                if (Mathf.Abs(oDistance - nDistance) < 2.0f)
                {
                        return 0;
                }

                return speed * (nDistance - oDistance) * Time.deltaTime;
        }


        public static float EnlargeDistance(float oDistance, float nDistance, float speed)
        {
                if (Mathf.Abs(oDistance - nDistance) < 4.0f)
                {
                        return 0;
                }
                else
                {
                        if (oDistance < nDistance)
                        {
                                return speed;
                        }
                        else
                        {
                                return -speed;
                        }
                }
        }

        public static void BeginProfiler(string name)
        {
#if UNITY_EDITOR
                UnityEngine.Profiling.Profiler.BeginSample(name);
#endif
        }

        public static void EndProfiler()
        {
#if UNITY_EDITOR
                UnityEngine.Profiling.Profiler.EndSample();
#endif
        }

        public static void ResetCameraAspect()
        {
                if (Camera.main != null)
                {
                        Camera.main.aspect = 1.775f;
                }
        }

        public static List<T> ToList<T>(this string str, char split, Converter<string, T> convertHandler)
        {
                if (string.IsNullOrEmpty(str))
                {
                        return new List<T>();
                }
                else
                {
                        string[] arr = str.Split(new char[] { split }, StringSplitOptions.RemoveEmptyEntries);
                        T[] Tarr = Array.ConvertAll(arr, convertHandler);
                        return new List<T>(Tarr);
                }
        }


        public static string ListToString<T>(List<T> list, char spilt) where T : struct
        {
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < list.Count; i++)
                {
                        sb.Append(list[i]);
                        if (i < list.Count - 1)
                        {
                                sb.Append(spilt);
                        }
                }
                return sb.ToString();
        }


        public static HashSet<T> ToHashSet<T>(this string str, char split, Converter<string, T> convertHandler)
        {
                if (string.IsNullOrEmpty(str))
                {
                        return new HashSet<T>();
                }
                else
                {
                        string[] arr = str.Split(new char[] { split }, StringSplitOptions.RemoveEmptyEntries);
                        T[] Tarr = Array.ConvertAll(arr, convertHandler);
                        return new HashSet<T>(Tarr);
                }
        }

        public static Dictionary<int, int> ParseCommonStringToDict(string str, char splitChar0 = '|', char splitChar1 = '&')
        {
                Dictionary<int, int> dict = new Dictionary<int, int>();
                if (!string.IsNullOrEmpty(str))
                {
                        string[] infos = str.Split(splitChar0);
                        for (int i = 0; i < infos.Length; i++)
                        {
                                if (string.IsNullOrEmpty(infos[i]))
                                        continue;

                                string[] tmps = infos[i].Split(splitChar1);
                                if (tmps.Length == 2)
                                {
                                        int key = 0, value = 0;
                                        int.TryParse(tmps[0], out key);
                                        int.TryParse(tmps[1], out value);
                                        if (dict.ContainsKey(key))
                                        {
                                                dict[key] = value;
                                        }
                                        else
                                        {
                                                dict.Add(key, value);
                                        }
                                }
                        }

                }
                return dict;
        }

        public static void SetMeshUVIndex(Mesh mesh, int index, int max)
        {                
                Vector2[] uvs = new Vector2[mesh.uv.Length];
                Vector2[] uv1s = new Vector2[mesh.uv2.Length];
                Rect rect = new Rect(index / (float)max, 0f, 1f / (float)max, 1f);
                for (int i = 0; i < uvs.Length; i++)
                {
                        uvs[i].x = rect.x + mesh.uv[i].x * rect.width;
                        uvs[i].y = rect.y + mesh.uv[i].y * rect.height;
                }
                for (int i = 0; i < uv1s.Length; i++)
                {
                        uv1s[i].x = rect.x + mesh.uv2[i].x * rect.width;
                        uv1s[i].y = rect.y + mesh.uv2[i].y * rect.height;
                }

                mesh.uv = uvs;
                mesh.uv2 = uv1s;
        }


        public static void DestroyChild(Transform root)
        {
                GameObject child = null;
                for (int i = 0; i < root.childCount; i++)
                {
                        child = root.GetChild(i).gameObject;
                        //child.SetActive(false);
                        GameObject.Destroy(child);
                }
        }


        public static void SetActive(this Transform tf, bool isShow)
        {
                if(tf != null)
                {
                        tf.gameObject.SetActive(isShow);
                }
        }

        public static void SetLocalPositionX(this Transform tf, float x)
        {
                if (tf != null)
                {
                        tf.localPosition = new Vector3(x, tf.localPosition.y, tf.localPosition.z);
                }
        }

        public static void SetLocalPositionY(this Transform tf, float y)
        {
                if (tf != null)
                {
                        tf.localPosition = new Vector3(tf.localPosition.x, y, tf.localPosition.z);
                }
        }

        public static void SetLocalPositionZ(this Transform tf, float z)
        {
                if (tf != null)
                {
                        tf.localPosition = new Vector3(tf.localPosition.x, tf.localPosition.y, z);
                }
        }

        public static void SetPositionX(this Transform tf, float x)
        {
                if (tf != null)
                {
                        tf.position = new Vector3(x, tf.position.y, tf.position.z);
                }
        }

        public static void SetPositionY(this Transform tf, float y)
        {
                if (tf != null)
                {
                        tf.position = new Vector3(tf.position.x, y, tf.position.z);
                }
        }

        public static void SetPositionZ(this Transform tf, float z)
        {
                if (tf != null)
                {
                        tf.position = new Vector3(tf.position.x, tf.position.y, z);
                }
        }

        public static void SetAnchoredPositionX(this RectTransform rt, float x)
        {
                if (rt != null)
                {
                        rt.anchoredPosition = new Vector2(x, rt.localPosition.y);
                }
        }

        public static void SetAnchoredPositionY(this RectTransform rt, float y)
        {
                if (rt != null)
                {
                        rt.anchoredPosition = new Vector2(rt.localPosition.x,y);
                }
        }

        static System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        public static void TimeTestStart()
        {
                sw.Start();
        }

        public static void TimeTestStop()
        {
                sw.Stop();
                Debuger.Log(string.Format("total: {0} ms", sw.ElapsedMilliseconds));
        }

        //计算ugui在传入父节点中的anchoredPosition
        public static Vector2 TransformToCanvasLocalPosition(this Transform current, Transform parent,Transform traget)
        {
                Camera camera = CameraManager.GetInst().UI_Camera;
                Vector2 screenPos = camera.WorldToScreenPoint(current.transform.position);
                Vector2 pos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(parent as RectTransform, screenPos, camera, out pos);

                if (traget != null)  //保证在中心//
                {
                        RectTransform rtfTraget = traget as RectTransform;
                        RectTransform rtfCurrent = current as RectTransform;

                        //坐标偏移矫正
                        pos.x = pos.x + (float)(rtfCurrent.rect.width * (rtfTraget.pivot.x - rtfCurrent.pivot.x));
                        pos.y = pos.y + (float)(rtfCurrent.rect.height * (rtfTraget.pivot.y - rtfCurrent.pivot.y));
                }

                return pos;
        }

        public static uint GetTimeStamp()
        {
                DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0);
                TimeSpan ts = DateTime.Now - dt;// Convert.ToDateTime("1970-1-1 00:00:00");
                return Convert.ToUInt32(ts.TotalSeconds);
        }

        public static void SavePlayerData(string key , string value) //存储玩家数据在客户端
        {
                long id = PlayerController.GetInst().PlayerID;
                string real_key = id + CommonString.underscoreStr + key;
                PlayerPrefs.SetString(real_key, value);

                PlayerPrefs.Save();
        }

        public static string GetPlayerData(string key)  
        {
                long id = PlayerController.GetInst().PlayerID;
                string real_key = id + CommonString.underscoreStr + key;
                return PlayerPrefs.GetString(real_key,""); 
        }

        public static int GetPlayerDataInt(string key)
        {
                string temp = GetPlayerData(key);
                int num = 0;
                int.TryParse(temp,out num);
                return num;
        }

        public static int GetMeshID(int x, int y, int z)
        {
                return (y * 10000 + x * 100 + z) * 10;
        }

        public static float LinearEquationInTwoUnknowns(float x0, float y0, float x1, float y1, float x)  //二元一次方程
        {
                float k = (y0 - y1) / (x0 - x1);
                float b = y0 - k * x0;
                return k * x + b;
        }

        public static bool IsStringValid(string str, string spilt = "") //验证str是否有效
        {
                if (String.IsNullOrEmpty(str))
                {
                        return false;
                }
                if (str.Equals(CommonString.zeroStr))
                {
                        return false;
                }
                if (spilt.Length > 0)
                {
                        if (!str.Contains(spilt))
                        {
                                return false;
                        }
                }
                return true;
        }

        public static void StopAllDoTween()
        {
                DOTween.KillAll();
        }


        /// <summary>
        /// 数组转十六进制字符串（数组元素小于16）
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        /// 
        static char[] HexString = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        public static string ToHexString(byte[] arr)
        {
                StringBuilder sb = new StringBuilder(arr.Length);
                for(int i = 0; i < arr.Length; i++)
                {
                        sb.Append(HexString[arr[i]]);
                }
                return sb.ToString();
        }

        static Dictionary<char, byte> StringToByte = new Dictionary<char, byte>()
        {
                {'0', 0},
                {'1', 1},
                {'2', 2},
                {'3', 3},
                {'4', 4},
                {'5', 5},
                {'6', 6},
                {'7', 7},
                {'8', 8},
                {'9', 9},
                {'A', 10},
                {'B', 11},
                {'C', 12},
                {'D', 13},
                {'E', 14},
                {'F', 15},                 
        };

              /// <summary>
        /// 十六进制字符串转数组（数组元素小于16）
        /// </summary>
        /// <param name="strHex"></param>
        /// <returns></returns>
        public static byte[] ToByteArray(string strHex)
        {
            byte[] arr = new byte[strHex.Length];
            for (int i = 0; i < strHex.Length; i++ )
            {
                    arr[i] = StringToByte[strHex[i]];
            }
            return arr;
        }

        public static string GzipCompress(string input)
        {
                byte[] buffer = Encoding.UTF8.GetBytes(input);
                using (MemoryStream outputStream = new MemoryStream())
                {
                        using (GZipOutputStream zipStream = new GZipOutputStream(outputStream))
                        {
                                zipStream.Write(buffer, 0, buffer.Length);
                                zipStream.Close();
                        }
                        return Convert.ToBase64String(outputStream.ToArray());
                }
        }

        public static string GzipDecompress(string input)
        {
                string result = string.Empty;
                byte[] buffer = Convert.FromBase64String(input);
                using (Stream inputStream = new MemoryStream(buffer))
                {
                        GZipInputStream zipStream = new GZipInputStream(inputStream);
                        using (StreamReader reader = new StreamReader(zipStream, Encoding.UTF8))
                        {
                                result = reader.ReadToEnd();
                        }
                }
                return result;
        }

        public static void EnableCameraRaycaster(bool bEnable)
        {
                if (Camera.main != null)
                {
                        PhysicsRaycaster pr = Camera.main.gameObject.GetComponent<PhysicsRaycaster>();
                        if (pr != null)
                        {
                                pr.enabled = bEnable;
                        }
                }
        }

        public static void SendGM(string gmSrt)
        {
                Message.CSMsgChat msg = new Message.CSMsgChat();
                msg.idTarget = 0;
                msg.byChannel = 0;
                msg.strTargetName = CommonString.zeroStr;
                msg.strText = gmSrt;
                NetworkManager.GetInst().SendMsgToServer(msg);
        }

        public static void ShowConfirmWnd(string languageId, UI_CheckBox.Handler confirm, UI_CheckBox.Handler cancel = null, object data = null)
        {
                UI_CheckBox uis = UIManager.GetInst().ShowUI<UI_CheckBox>("UI_CheckBox");
                uis.SetConfirmAndCancel("", LanguageManager.GetText(languageId), confirm, cancel, data);
        }
        public static void ShowConfirmWndEx(string text, UI_CheckBox.Handler confirm, UI_CheckBox.Handler cancel = null, object data = null)
        {
                UI_CheckBox uis = UIManager.GetInst().ShowUI<UI_CheckBox>("UI_CheckBox");
                uis.SetConfirmAndCancel("", text, confirm, cancel, data);
        }

        public static string StringConnect(string a0, string a1, string a2, string a3)
        {
                StringBuilder sb = new StringBuilder(64);
                sb.Append(a0).Append(a1).Append(a2).Append(a3);
                return sb.ToString();
        }

        public static string StringConnect(string a0, string a1, string a2, string a3, string a4)
        {
                StringBuilder sb = new StringBuilder(64);
                sb.Append(a0).Append(a1).Append(a2).Append(a3).Append(a4);
                return sb.ToString();
        }

        public static string StringConnect(string a0, string a1, string a2, string a3, string a4, string a5)
        {
                StringBuilder sb = new StringBuilder(64);
                sb.Append(a0).Append(a1).Append(a2).Append(a3).Append(a4).Append(a5);
                return sb.ToString();
        }
        public static void SetupQualityAA()
        {
                if (QualitySettings.GetQualityLevel() == 5)
                {
                        QualitySettings.antiAliasing = 8;
                }
                else if (QualitySettings.GetQualityLevel() > 1)
                {
                        QualitySettings.antiAliasing = 2;
                }
                else
                {
                        QualitySettings.antiAliasing = 0;
                }
        }

        public static bool RandomChance(int probability)
        {
                int random = UnityEngine.Random.Range(0, 100);
                if (random < probability)
                {
                        return true;
                }
                else
                {
                        return false;
                }
        }

        public static bool pointInRegion(Vector2 pt, List<Vector2> plist)
        {
                int nCross = 0;    // 定义变量，统计目标点向右画射线与多边形相交次数

                for (int i = 0; i < plist.Count; i++)
                {   //遍历多边形每一个节点

                        Vector2 p1;
                        Vector2 p2;

                        p1 = plist[i];
                        p2 = plist[(i + 1) % plist.Count];  // p1是这个节点，p2是下一个节点，两点连线是多边形的一条边
                                                            // 以下算法是用是先以y轴坐标来判断的

                        if (p1.y == p2.y)
                                continue;   //如果这条边是水平的，跳过

                        if (pt.y < Math.Min(p1.y, p2.y)) //如果目标点低于这个线段，跳过
                                continue;

                        if (pt.y >= Math.Max(p1.y, p2.y)) //如果目标点高于这个线段，跳过
                                continue;
                        //那么下面的情况就是：如果过p1画水平线，过p2画水平线，目标点在这两条线中间
                        double x = (double)(pt.y - p1.y) * (double)(p2.x - p1.x) / (double)(p2.y - p1.y) + p1.x;
                        // 这段的几何意义是 过目标点，画一条水平线，x是这条线与多边形当前边的交点x坐标
                        if (x > pt.x)
                                nCross++; //如果交点在右边，统计加一。这等于从目标点向右发一条射线（ray），与多边形各边的相交（crossing）次数
                }

                if (nCross % 2 == 1)
                {
                        return true; //如果是奇数，说明在多边形里
                }
                else
                {
                        return false; //否则在多边形外 或 边上
                }
        }

        public static int GetNumInList<T>(List<T> list, T item)  //统计某个元素在list中的出现次数
        {
                int num = 0;
                if(list != null)
                {
                        for (int i = 0; i < list.Count; i++)
                        {
                                if (list[i].Equals(item))
                                {
                                        num++;
                                }
                        }
                }
                return num;
        }
}


