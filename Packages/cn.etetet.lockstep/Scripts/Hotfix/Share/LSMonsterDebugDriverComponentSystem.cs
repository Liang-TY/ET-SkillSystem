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

        /// <summary>轮播表（index → SkillId）。注意括号：`a % b switch {...}` 会解析成 `a % (b switch {...})`
        /// ——switch 表达式作右操作数时优先级高于二元运算符，曾因此变成 index%10 循环放全技能（2026-08-22 排查实录）</summary>
        private static int GetSkillId(int index) => (index % RotationCount) switch
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
            // 编译期常量自检（const 是内联的——这行打印的是 ET.Hotfix 编译时链接的 ET.Skill.dll 里的值，
            // 不是当前源码的值。若与源码（7/8/9/10/4）不符 = 编译产物陈旧）
            Log.Info($"[MonsterDriver] 常量自检：LowKick={SkillIds.MonsterLowKick} KneeKick={SkillIds.MonsterKneeKick} " +
                     $"HighKick={SkillIds.MonsterHighKick} IceBreath={SkillIds.MonsterIceBreath} RotationCount={RotationCount}");
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
            bool ok = SkillCastHelper.TryCast(unit, skillId);
            Log.Info($"[MonsterDriver] unit{unit.Id} 第{self.SkillIndex}发 skillId={skillId} → {(ok ? "成功" : "被门禁拦下")}");
        }
    }
}
