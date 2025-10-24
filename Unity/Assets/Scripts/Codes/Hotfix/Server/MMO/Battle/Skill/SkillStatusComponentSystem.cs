using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ET.Server
{
    public class SkillStatusComponentDestroySystem : DestroySystem<SkillStatusComponent>
    {
        protected override void Destroy(SkillStatusComponent self)
        {
            self.CurSkillCastInstanceId = default;
            self.CurSkillCastID = default;
            self.CurSkillStartTime = default;
            self.CurSkillStatus = SkillStatusType.New;
            self.CoolDowns.Clear();
        }
    }
    [FriendOfAttribute(typeof(ET.Server.SkillStatusComponent))]
    [FriendOfAttribute(typeof(ET.Server.Cast))]
    public static class SkillStatusComponentSystem
    {
        public static int CanCastSkill(this SkillStatusComponent self, int castConfigId)
        {
            Unit unit = self.GetParent<Unit>();
            if (unit == null)
            {
                return ErrorCode.ERR_Cast_UnitIsNull;
            }

            NumericComponent numericComponent = unit.GetComponent<NumericComponent>();
            if (numericComponent == null)
            {
                return ErrorCode.ERR_Cast_NumIsNull;
            }

            if (numericComponent[NumericType.ForbidSkill] > 0)
            {
                return ErrorCode.ERR_Cast_ForbirdSkill;
            }
            if (self.CoolDowns.TryGetValue(castConfigId, out long tarTime))
            {
                if (TimeHelper.ServerNow() <= tarTime)
                {
                    return ErrorCode.ERR_Cast_SkillCDDown;
                }

            }

            return ErrorCode.ERR_Success;
        }

        public static bool StartSkill(this SkillStatusComponent self, Cast cast)
        {
            if (self.CanCastSkill(cast.ConfigId) != ErrorCode.ERR_Success)
            {
                return false;
            }
            int castConfigId = cast.ConfigId;
            if (cast.Config.StatusSkill == 0)
            {
                return true;
            }

            long now = TimeHelper.ServerNow();
            self.CurSkillCastID = castConfigId;
            self.CurSkillCastInstanceId = cast.InstanceId;
            self.CurSkillStartTime = now;
            self.CurSkillStatus = SkillStatusType.Init;
            int coolDown = CastConfigCategory.Instance.Get(castConfigId).CoolDown;
            if (coolDown > 0)
            {
                self.CoolDowns[castConfigId] = now + coolDown;
                Unit unit = self.GetParent<Unit>();

                M2C_CoolDownChange m2CCoolDownChange = new M2C_CoolDownChange()
                {
                    CastConfigIds = new List<int>(),
                    CoolDownTimes = new List<long>(),
                    CoolDownStartTime = new List<long>()

                };
                m2CCoolDownChange.CastConfigIds.Add(castConfigId);
                m2CCoolDownChange.CoolDownTimes.Add(self.CoolDowns[castConfigId]);
                m2CCoolDownChange.CoolDownTimes.Add(now);
                MMOMessageHelper.SendClient(unit, m2CCoolDownChange, NoticeClientType.Self);

            }
            return true;

        }

    }
}
