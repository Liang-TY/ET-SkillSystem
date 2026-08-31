using TrueSync;

namespace ET
{
    /// <summary>
    /// 邪光斩·慢速爬行波（PO 24349 子 id 11 数据反推：3000ms 爬向 col0 射程，走到 30% 消散）。
    /// 穿透多目标 + 350ms 多段重置（HitResetIntervalMs——框架首个投射物多段用例，~3 跳）。
    /// 波体视觉 loop（grandwave_light_grandwave2.ani），碰撞盒取视觉尺寸（PO 动画无盒——§8 存疑）。
    /// 参考：Notes/技能实现/鬼剑士技能解析/050-GrandWave.md §2.3
    /// </summary>
    [BulletId(BulletIds.GrandWave)]
    public class GrandWaveBullet : BulletDefinition
    {
        public override FP Speed => (FP)25 / 10;         // 慢速爬行（Lv1 658px/3s ≈ 2.2 单位/s 折中）
        public override int TotalTimeMs => 900;          // 存活 = 走到 30% 射程（射程 ≈ 2.3 单位）
        public override bool DestroyOnHit => false;      // 穿透多目标
        public override int HitResetIntervalMs => 350;   // static[0]=350ms 多段间隔（~3 跳）

        public override TSVector HalfExtents => new((FP)15 / 10, (FP)5 / 10, (FP)75 / 100);

        // grandwave.atk 直译：push 300 / lift 200 / blow；伤害 45×3 跳（col0 拆分 demo 惯例）
        private static readonly HitReaction Reaction = new()
        {
            Damage = 45,
            HitstunMs = 400,
            KnockbackX = 300,
            LaunchY = 200,
        };
        public override HitReaction HitReaction => Reaction;

        private static readonly int[] HitActionsArr = { ActionIds.MeleeHit };
        public override int[] HitActions => HitActionsArr;

        public override int ViewAnimId => AnimId.GrandWaveWheel;
        public override bool ViewGrounded => true;
    }
}
