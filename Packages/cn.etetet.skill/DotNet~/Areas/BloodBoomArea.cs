using TrueSync;

namespace ET
{
    /// <summary>
    /// 浴血之怒爆炸（DNF 被动对象 24370 同构）：以施法者为中心的一次性大范围血爆。
    /// 单位进区结算一次（MeleeHit 300 伤害 + 挂出血），不 Tick（4 段/8 段多段延后，demo 单次大伤害）。
    /// 区域不影响施法者（LSAreaSystem 跳过 CasterId），自爆自免伤与 DNF 行为一致。
    /// 范围 = finish3.ani 帧 2 攻击盒 x ±400px ≈ 半径 4 单位。
    /// 视觉：正面 boomfront（445ms）+ 背面 boomback（365ms）两层爆炸（ViewBackAnimId）。
    /// </summary>
    [AreaId(AreaIds.BloodBoom)]
    public class BloodBoomArea : AreaDefinition
    {
        private static readonly HitReaction Reaction = new() { Damage = 300, HitstunMs = 1000 };
        public override HitReaction HitReaction => Reaction;

        public override int TotalTimeMs => 500;    // 爆炸一闪（boomfront 445ms + 余量）
        public override int TickTimeMs => 0;       // 单次（EnterActions 打完即止）

        public override TSVector HalfExtents => new(4, (FP)15 / 10, (FP)15 / 10);   // 8×3×3 单位血爆

        private static readonly int[] EnterActionsArr = { ActionIds.MeleeHit, ActionIds.AddBleedBuff };
        public override int[] EnterActions => EnterActionsArr;

        public override int ViewAnimId => AnimId.BloodboomBoomFront;      // 正面爆炸（10 帧 445ms）
        public override int ViewBackAnimId => AnimId.BloodboomBoomBack;  // 背面爆炸（8 帧 365ms）
        public override int ViewEndAnimId => AnimId.None;                // 爆炸无收尾动画
    }
}
