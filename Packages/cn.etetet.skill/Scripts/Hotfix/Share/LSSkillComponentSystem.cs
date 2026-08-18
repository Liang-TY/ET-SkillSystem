namespace ET
{
    /// <summary>
    /// LSSkillComponent 的 Hotfix 侧 System：LSUpdate（Route B 清标记 + 冷却递减 + 缓冲消费）。
    /// TryCast/门禁在 ET.Skill 的 SkillCastHelper（SkillContext.RestartCurrentSkill 也要调，放这会循环依赖）。
    /// </summary>
    [EntitySystemOf(typeof(LSSkillComponent))]
    [LSEntitySystemOf(typeof(LSSkillComponent))]
    [FriendOf(typeof(LSSkillComponent))]
    [FriendOf(typeof(LSCast))]
    [FriendOf(typeof(LSInputBufferComponent))]
    public static partial class LSSkillComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LSSkillComponent self)
        {
            self.Cooldowns ??= new();
        }

        [LSEntitySystem]
        private static void LSUpdate(this LSSkillComponent self)
        {
            LSUnit unit = self.GetParent<LSUnit>();

            // 1) Route B：清子 cast 上一帧标记。本组件先于 Hitfix/Hitbox/Cast 更新 →
            //    之后设的标记（命中/创建/结束）活到下一个清理点，视图层有完整轮询窗口
            LSCastComponent castComp = unit.GetComponent<LSCastComponent>();
            if (castComp != null)
            {
                foreach (var kv in castComp.Children)
                {
                    if (kv.Value is LSCast cast)
                    {
                        cast.JustStarted = cast.JustHit = cast.JustFinished = false;
                    }
                }
            }

            // 2) 冷却递减
            if (self.Cooldowns.Count > 0)
            {
                // key 快照避免迭代中改集合；技能数个位数，分配可忽略
                int[] keys = new int[self.Cooldowns.Count];
                self.Cooldowns.Keys.CopyTo(keys, 0);
                for (int i = 0; i < keys.Length; i++)
                {
                    int remain = self.Cooldowns[keys[i]] - LSConstValue.UpdateInterval;
                    if (remain <= 0) self.Cooldowns.Remove(keys[i]);
                    else self.Cooldowns[keys[i]] = remain;
                }
            }

            // 3) 消费输入缓冲 → 施放（按下沿写入 LSInputComponentSystem；
            //    攻击中缓冲持有到取消窗口由技能 OnUpdate 自己消费——RestartCurrentSkill）
            LSInputBufferComponent buf = unit.GetComponent<LSInputBufferComponent>();
            if (buf == null || buf.BufferedButton == 0) return;
            if (!SkillIds.ButtonToSkill(buf.BufferedButton, out int skillId)) return;
            if (SkillCastHelper.TryCast(unit, skillId))
            {
                buf.BufferedButton = 0;
                buf.BufferTimer = 0;
            }
        }

        /// <summary>施放（Hotfix 侧入口，委托 ET.Skill 的门禁实现）</summary>
        public static bool TryCast(this LSSkillComponent self, int skillId)
        {
            return SkillCastHelper.TryCast(self.GetParent<LSUnit>(), skillId);
        }
    }
}
