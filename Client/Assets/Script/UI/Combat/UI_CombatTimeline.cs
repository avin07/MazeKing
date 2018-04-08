using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
public class UI_CombatTimeline : UIBehaviour
{
        const int MAX_PLAYER = 7;
        Image[] m_ActorImageBg = new Image[MAX_PLAYER];
        Image[] m_ActorImages = new Image[MAX_PLAYER];
        public Sprite m_EnemyBg, m_MyBg;
        public Sprite m_EnemyBg0, m_MyBg0;
        // Use this for initialization
        void Awake()
        {
                for (int i = 0; i < MAX_PLAYER; i++)
                {
                        m_ActorImages[i] = GetImage("Actor" + i);
                        m_ActorImageBg[i] = GetImage("Actorbg" + i);
                }

                this.UILevel = UI_LEVEL.MAIN;
        }

        // Update is called once per frame
        void Update()
        {

        }

        Queue<FighterBehav> m_ActionQueue = new Queue<FighterBehav>();
        public void ResetTimeline()
        {
                List<FighterBehav> timelineList = new List<FighterBehav>();
                Dictionary<long, int> backupDelayVal = new Dictionary<long, int>();

                foreach (FighterBehav behav in CombatManager.GetInst().GetAllFighters())
                {
                        if (behav.FighterProp.Hp > 0)
                        {
                                timelineList.Add(behav);
                                backupDelayVal.Add(behav.FighterId, behav.FighterProp.DelayVal);
                        }
                }
                
                for (int i = 0; i < MAX_PLAYER; i++)
                {
                        timelineList.Sort(FighterBehav.CompareSpeed);
                        FighterBehav actor = timelineList[0];
                        if (i < m_ActorImages.Length)
                        {
                                m_ActorImages[i].enabled = true;
                                ResourceManager.GetInst().LoadIconSpriteSyn(ModelResourceManager.GetInst().GetIconRes(actor.CharacterCfg.modelid), m_ActorImages[i].transform);

                                if (actor.IsEnemy)
                                {
                                        m_ActorImageBg[i].sprite = i == 0 ? m_EnemyBg0 : m_EnemyBg;
                                }
                                else
                                {
                                        m_ActorImageBg[i].sprite = i == 0 ? m_MyBg0 : m_MyBg;
                                }
                        }
                        
                        int delta = actor.FighterProp.DelayVal;
                        for (int idx = 1; idx < timelineList.Count; idx++)
                        {
                                timelineList[idx].FighterProp.DelayVal -= delta;
                        }
                        actor.FighterProp.ResetDelay();
                }

                foreach (FighterBehav behav in timelineList)
                {
                        behav.FighterProp.DelayVal = backupDelayVal[behav.FighterId];
                }
        }

        public FighterBehav GetNextActor()
        {
                return m_ActionQueue.Dequeue();
        }
}
