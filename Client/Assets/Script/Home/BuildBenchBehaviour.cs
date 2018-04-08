using UnityEngine;
using Message;


public class BuildBenchBehaviour : BuildBaseBehaviour  //制造类建筑
{
        protected override string GetFunctionStr()
        {
                string function = base.GetFunctionStr();

                if (mBuildInfo.eState == EBuildState.eWork)
                {
                        if (Formula_id > 0)
                        {
                                function = cancelAndQuickStr;
                        }
                }
                return function;
        }

        protected override void UpdateStateBar()
        {
                base.UpdateStateBar();
                if (Formula_id > 0)
                {
                        ShowMakeTip();
                }
        }

        int m_formula_id = 0;
        float m_formula_rest_time = 0;

        public int Formula_id
        {
                set
                {
                        m_formula_id = value;
                }
                get
                {
                        return m_formula_id;
                }
        }

        public float FormulaRestTime
        {
                set
                {
                        m_formula_rest_time = value;
                }
                get
                {
                        return m_formula_rest_time;
                }
        }

        public void SetMakeInfo(SCMsgBuildBench msg)
        {
                Formula_id = msg.m_idFormula;
                FormulaRestTime = msg.rest_time + Time.realtimeSinceStartup;
                ShowMakeTip();

                if (HomeManager.GetInst().GetSelectBuild() == this)
                {
                        ShowBuildMenu();
                }
        }

        public void ShowMakeTip()
        {
                if (Formula_id != 0)
                {
                        restTime = FormulaRestTime - Time.realtimeSinceStartup + bufferTime;
                        SetBuildingState((int)restTime, HomeManager.GetInst().GetFormulaCfg(Formula_id).make_time);
                        int num = 0;
                        string name = "";
                        int id = 0;
                        string des = "";
                        Thing_Type type;
                        CommonDataManager.GetInst().SetThingIcon(HomeManager.GetInst().GetFormulaCfg(Formula_id).output, BuildStateBar.GetBarIcon(),null, out name, out num, out id, out des, out type);
                        if (restTime <= bufferTime)
                        {
                                HomeManager.GetInst().SendBuildStateReq(mBuildInfo.id);
                                Formula_id = 0;
                                restTime = 0;
                                //HideBuildBar();
                        }
                }
                else
                {
                        HideBuildBar();
                }
        }

        public void PlayMakeFinishEffect() //制作完成特效
        {
                GameObject effect = EffectManager.GetInst().PlayEffect(GlobalParams.GetString("make_finish_effect"), transform);
                effect.transform.position += new Vector3(0, 2.0f, 0);
        }

        public override void Cancel()
        {
                string text = "";
                if (Formula_id > 0)  //制造中取消
                {
                        int num = 0;
                        string name = "";
                        int id = 0;
                        string des = "";
                        Thing_Type type;
                        CommonDataManager.GetInst().SetThingIcon(HomeManager.GetInst().GetFormulaCfg(Formula_id).output, null,null, out name, out num, out id, out des, out type);
                        text = "您确定要停止制作" + LanguageManager.GetText(HomeManager.GetInst().GetFormulaCfg(Formula_id).name) + "X" + num + "?" + "停止制作只返还材料的" + GlobalParams.GetInt("make_back_per") + "%";
                        UIManager.GetInst().ShowUI<UI_CheckBox>("UI_CheckBox").SetConfirmAndCancel("停止制造", text, ConfirmMakeCancel, null, null);
                }
                base.Cancel();
        }

        void ConfirmMakeCancel(object data)
        {
                HomeManager.GetInst().SendMakeCancel();
        }

        public override void Quick()
        {
                string text = string.Empty;
                if (Formula_id > 0)  //制造中取消
                {
                        int cost = Mathf.CeilToInt(restTime / GlobalParams.GetInt("diamond_time_bench"));
                        text = "您确定要花费" + cost + "钻石快速完成？";
                        UIManager.GetInst().ShowUI<UI_CheckBox>("UI_CheckBox").SetConfirmAndCancel("快速建造", text, ConfirmBenchQuick, null, null);
                }
                base.Quick();
        }

        void ConfirmBenchQuick(object data)
        {
                int cost = Mathf.CeilToInt(restTime / GlobalParams.GetInt("diamond_time_bench"));
                if (CommonDataManager.GetInst().GetNowResourceNum("diamond") >= cost)
                {
                        HomeManager.GetInst().SendBenchQuick();
                }
                else
                {
                        GameUtility.PopupMessage("钻石不足！");
                }
        }

}