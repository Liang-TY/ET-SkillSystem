namespace ET
{
    [EntitySystemOf(typeof(LSCast))]
    [LSEntitySystemOf(typeof(LSCast))]
    [FriendOf(typeof(LSCast))]
    [FriendOf(typeof(LSSkillComponent))]
    public static partial class LSCastSystem
    {
        [EntitySystem]
        private static void Awake(this LSCast self, int skillId)
        {
            self.SkillId = skillId;
            self.TargetIds ??= new();
        }

        /// <summary>创建施放实例并立刻 OnCast（SkillCastHelper.TryCast 调；回滚后从快照恢复不重跑）</summary>
        public static LSCast Create(this LSCastComponent parent, LSUnit caster, int skillId)
        {
            LSCast cast = parent.AddChild<LSCast, int>(skillId);
            cast.CasterId = caster.Id;
            SkillLogic logic = SkillLoader.Get(skillId);
            cast.TotalTimeMs = logic?.TotalTimeMs ?? 0;
            Log.Info($"[Skill] unit{caster.Id} cast {skillId}（{logic?.GetType().Name ?? "未注册"}）");   // 诊断：谁放了什么招

            SkillContext ctx = new(parent.LSWorld(), caster, cast);
            logic?.OnCast(ctx);
            cast.JustStarted = true;
            return cast;
        }

        [LSEntitySystem]
        private static void LSUpdate(this LSCast self)
        {
            // Just* 标记的清理由 Hotfix 侧 LSSkillComponentSystem.LSUpdate 开头做（它先于 Hitbox/本系统跑，
            // "设标记的都在清之后"）——本系统只驱动逻辑与生命周期。
            if (self.Finished)
            {
                self.Dispose();   // 标记已被视图读过一帧，确定性回收（下帧首条 LSUpdate）
                return;
            }

            LSUnit caster = self.GetParent<LSCastComponent>().GetParent<LSUnit>();
            self.ElapsedMs += LSConstValue.UpdateInterval;

            SkillContext ctx = new(caster.LSWorld(), caster, self);
            SkillLoader.Get(self.SkillId)?.OnUpdate(ctx, LSConstValue.UpdateInterval);

            // 自动结束（TotalTimeMs > 0 到时）
            if (!self.Finished && self.TotalTimeMs > 0 && self.ElapsedMs >= self.TotalTimeMs)
            {
                self.EndNow(ctx);
            }
        }

        /// <summary>立即结束：OnEnd → 关盒兜底 → ManualCooldown 起 CD（直写 Cooldowns，不走 Hotfix 扩展方法避免循环依赖）</summary>
        public static void EndNow(this LSCast self, SkillContext ctx)
        {
            if (self.Finished) return;
            SkillLogic logic = SkillLoader.Get(self.SkillId);
            logic?.OnEnd(ctx);
            ctx.DisableAttackHitbox();   // 技能契约是 OnEnd 自己关盒，这里兜底
            self.Finished = true;
            self.JustFinished = true;

            // CD 双机制：ManualCooldown（多段技能）在 OnEnd 才起 CD
            if (logic != null && logic.ManualCooldown && logic.CooldownMs > 0)
            {
                LSSkillComponent skill = self.GetParent<LSCastComponent>().GetParent<LSUnit>().GetComponent<LSSkillComponent>();
                if (skill != null) skill.Cooldowns[self.SkillId] = logic.CooldownMs;
            }
        }

        /// <summary>命中回写（LSHitboxComponentSystem.ApplyHit 调；Route B：JustHit + TargetIds）</summary>
        public static void NotifyHit(this LSCast self, long targetId)
        {
            if (self.Finished || self.TargetIds.Contains(targetId)) return;
            self.TargetIds.Add(targetId);
            self.JustHit = true;
        }
    }
}
