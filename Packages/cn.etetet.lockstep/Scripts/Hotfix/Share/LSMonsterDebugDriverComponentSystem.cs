namespace ET
{
    /// <summary>
    /// 怪物技能轮播驱动系统（阶段1 测试；阶段2 AI 接管后移除）。
    /// 每 3 秒依次放一个技能：LowKick → KneeKick → HighKick → IceBreath 循环。
    /// </summary>
    [EntitySystemOf(typeof(LSMonsterDebugDriverComponent))]
    [LSEntitySystemOf(typeof(LSMonsterDebugDriverComponent))]
    [FriendOf(typeof(LSMonsterDebugDriverComponent))]
    public static partial class LSMonsterDebugDriverComponentSystem
    {
        private const int IntervalMs = 3000;
        private const int RotationCount = 4;   // 轮播表长度（ET0004：Hotfix 禁非 const 字段，表用 switch 表达式）

        /// <summary>轮播表（index → SkillId）</summary>
        private static int GetSkillId(int index) => index % RotationCount switch
        {
            0 => SkillIds.MonsterLowKick,
            1 => SkillIds.MonsterKneeKick,
            2 => SkillIds.MonsterHighKick,
            _ => SkillIds.MonsterIceBreath,
        };

        [EntitySystem]
        private static void Awake(this LSMonsterDebugDriverComponent self)
        {
            self.TimerMs = IntervalMs;
        }

        [LSEntitySystem]
        private static void LSUpdate(this LSMonsterDebugDriverComponent self)
        {
            self.TimerMs -= LSConstValue.UpdateInterval;
            if (self.TimerMs > 0) return;
            self.TimerMs = IntervalMs;

            LSUnit unit = self.GetParent<LSUnit>();
            int skillId = GetSkillId(self.SkillIndex);
            self.SkillIndex++;
            // 三重门禁照走（硬直中/在技中/CD 中放不出来——正常，下轮试下一个）
            SkillCastHelper.TryCast(unit, skillId);
        }
    }
}
