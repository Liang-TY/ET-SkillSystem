using TrueSync;

namespace ET
{
    /// <summary>
    /// 冰息弹（DNF 被动对象 30413 BantuIceBreath1 同构）：冰雾直线飞行，穿透。
    /// ptl：x 250 速（2.5 单位/s）/寿命 1200ms；obj：动画播完自毁（6 帧×150ms=900ms ≤ 寿命，取动画时长）。
    /// bantuicebreath1.atk：魔法/水属性/+10% 加成/push 30/lift 100/**10% 冰冻 3.5s**。
    /// 视觉双层：主层 IceBreathBullet1 + 背层 IceBreathBullet2（全帧 LINEARDODGE 加法混合）。
    /// </summary>
    [BulletId(BulletIds.IceBreath)]
    public class IceBreathBullet : BulletDefinition
    {
        private static readonly HitReaction Reaction = new()
        {
            Damage = 40,
            HitstunMs = 400,
            KnockbackX = 30,
            LaunchY = 100,
            ProcBuffId = BuffIds.Freeze,
            ProcChance = 10,
        };
        public override HitReaction HitReaction => Reaction;

        public override FP Speed => (FP)25 / 10;    // ptl x 250px/s → 2.5 单位/s
        public override int TotalTimeMs => 1200;    // ptl life time
        public override bool DestroyOnHit => false; // [piercing power] 1000 穿透

        // bantuicebreath1.ani 雾团判定：ATTACK BOX 约 ±120px 宽/±15 深 → 半尺寸 1.2×0.4×0.2（DNF 原值缩小适配贴身弹）
        public override TSVector HalfExtents => new((FP)12 / 10, (FP)4 / 10, (FP)15 / 100);

        private static readonly int[] HitActionsArr = { ActionIds.MeleeHit };
        public override int[] HitActions => HitActionsArr;

        // 主层 IceBreathBullet1；第二层视觉 IceBreathBullet2 由 .als overlay 挂在主层上
        // （LSAnimClipRegistrar 注册 icebreath_bullet_overlay → 弹视图消费，base=11）
        public override int ViewAnimId => AnimId.IceBreathBullet1;
    }
}
