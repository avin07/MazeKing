using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class UI_SkillTips : UIBehaviour
{
        GameObject m_TipGroup;
        RectTransform m_TipRt;

        void Awake()
	{
                m_TipGroup = GetGameObject("tipgroup");
                m_TipRt = m_TipGroup.GetComponent<RectTransform>();
        }


        public void ShowSkillTip(int skillId, Vector3 position, FighterProperty fighterProp)
        {
                ShowSkillTip(skillId, position, fighterProp.GetActiveSkillLevel(skillId));
        }
        public void ShowSkillTip(int skillId, Vector3 position, int level = 0)
        {
                GetText(m_TipGroup, "skillname").text = SkillManager.GetInst().GetSkillName(skillId) + "Lv." + level.ToString();
                GetText(m_TipGroup, "skilldesc").text = SkillManager.GetInst().GetSkillDesc(skillId, level);
                GetText(m_TipGroup, "skillcd").text = "";

                SkillConfig activeSkill = SkillManager.GetInst().GetActiveSkill(skillId);
                if (activeSkill != null)
                {
                        GetText(m_TipGroup, "skillcd").text = activeSkill.GetLevelValue(activeSkill.cool_down, level).ToString();
                }
                StartCoroutine(SetTipPosition(position));
        }

        public void SetBuffTip(SkillBuffConfig buffCfg, Vector3 position)
        {
                GetText(m_TipGroup, "skillname").text = LanguageManager.GetText(buffCfg.name);
                GetText(m_TipGroup, "skilldesc").text = LanguageManager.GetText(buffCfg.desc);
                GetText(m_TipGroup, "skillcd").text = buffCfg.time.ToString();

                StartCoroutine(SetTipPosition(position));
        }
        IEnumerator SetTipPosition(Vector3 position)
        {
                yield return null;

                if (position.x + m_TipRt.sizeDelta.x > Screen.width)
                {
                        position.x -= m_TipRt.sizeDelta.x + 30f;
                }
                if (position.y + m_TipRt.sizeDelta.y > Screen.height)
                {
                        position.y -= m_TipRt.sizeDelta.y + 32f;
                }

                Vector3 outpos;
                RectTransformUtility.ScreenPointToWorldPointInRectangle(m_TipRt, new Vector2(position.x, position.y), CameraManager.GetInst().UI_Camera, out outpos);
                m_TipRt.position = outpos;
        }

}