namespace ET
{
    [EntitySystemOf(typeof(LSCombatComponent))]
    [LSEntitySystemOf(typeof(LSCombatComponent))]
    [FriendOf(typeof(LSCombatComponent))]
    public static partial class LSCombatComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LSCombatComponent self, int defaultAnimId)
        {
            self.DefaultAnimId = defaultAnimId;
        }

        [LSEntitySystem]
        private static void LSUpdate(this LSCombatComponent self)
        {
            // 先记上帧值再递减（视图 Route B diff：0→>0 = 刚被击中）。
            // 命中在 LSHitboxComponentSystem（组件 Id 更大、同帧稍后）写 HitstunTimer，
            // 下一渲染帧能看到 last=0 / cur>0 的边沿。
            self.LastHitstunTimer = self.HitstunTimer;

            if (self.HitstunTimer > 0)
            {
                self.HitstunTimer -= LSConstValue.UpdateInterval;
                if (self.HitstunTimer < 0) self.HitstunTimer = 0;

                // 硬直结束：受击/倒地动画切回默认（用动画 Id 匹配——每角色可能不同，不硬编码）
                if (self.HitstunTimer == 0)
                {
                    LSAnimComponent anim = self.GetParent<LSUnit>().GetComponent<LSAnimComponent>();
                    if (anim != null && (anim.AnimId == self.HurtAnimId || anim.AnimId == self.DownAnimId))
                        anim.Play(self.DefaultAnimId);
                }
            }

            // 顿帧暂不启用（DNF 实证攻方不停帧），仅维持倒计时语义
            if (self.HitstopTimer > 0)
            {
                self.HitstopTimer -= LSConstValue.UpdateInterval;
                if (self.HitstopTimer < 0) self.HitstopTimer = 0;
            }
        }
    }
}
