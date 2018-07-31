using UnityEngine;
using System.Collections;
namespace Message
{

        class SCMsgPetSkillAll : SCMsgBaseAck //{info}; info: id&idConfig&level|id&idConfig&level|...
        {
                public string skill_info;  //伙伴id*100 + offset

                public SCMsgPetSkillAll()
                {
                        SetTag("sPetSkillSync");
                }
        }

        /// <summary>
        /// 服务端同步伙伴技能信息 S -> C: sPetSkill {id&idConfig&nLevel}; // id：技能id，伙伴id*100 + offset;
        /// </summary>
        class SCMsgPetSkillOne : SCMsgBaseAck
        {
                public string skill_info;  //伙伴id*100 + offset

                public SCMsgPetSkillOne()
                {
                        SetTag("sPetSkill");
                }
        }

        class CSMsgSkillReset : CSMsgBaseReq
        {
                public long idPet;
                public CSMsgSkillReset()
                {
                        SetTag("cSkillReset");
                }
        }

        class SCMsgSkillReset : SCMsgBaseAck
        {
                public long idPet;
                public SCMsgSkillReset()
                {
                        SetTag("sSkillReset");
                }
        }

        class CSMsgSkillLevelUp : CSMsgBaseReq
        {
                public long idPet;
                public long idSkill;
                public CSMsgSkillLevelUp()
                {
                        SetTag("cSkillLevelUp");
                }
        }

}
