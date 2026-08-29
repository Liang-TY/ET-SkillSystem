using TrueSync;

namespace ET
{
    /// <summary>
    /// 连突刺·激光剑气弹（dashattackmultihitsub.obj 单相位直译：pass all 全穿透，播完即毁）。
    /// 贴身出生不飞行（Speed=0），盒覆盖身前延伸段；视觉 thrust_beemsword.ani 自带光束增长观感。
    /// 参考：Notes/技能实现/鬼剑士技能解析/011-DashAttackMultiHit.md §2.2
    /// </summary>
    [BulletId(BulletIds.ThrustBeam)]
    public class ThrustBeamBullet : BulletDefinition
    {
        public override FP Speed => FP.Zero;             // 不飞行（贴身光束段，PO 判定体同构）
        public override int TotalTimeMs => 425;          // PO 动画 6 帧 425ms，播完即毁
        public override bool DestroyOnHit => false;      // pass all 全穿透（HitTargets 去重）

        // PO 盒 F1-F4 折算：x 半幅 ~0.75 单位（光束前伸段），高度 0.3，纵深薄盒
        public override TSVector HalfExtents => new((FP)75 / 100, (FP)3 / 10, (FP)25 / 100);

        // 出生：身前 1.65 单位（PO 盒 F4 中心 x=165px 直译），腰位高度
        public override TSVector SpawnOffset => new((FP)165 / 100, (FP)5 / 10, FP.Zero);

        // dashattackmultihitsub.atk 直译：damage/push 30/lift 30
        private static readonly HitReaction Reaction = new()
        {
            Damage = 40,
            HitstunMs = 300,
            KnockbackX = 30,
            LaunchY = 30,
        };
        public override HitReaction HitReaction => Reaction;

        private static readonly int[] HitActionsArr = { ActionIds.MeleeHit };
        public override int[] HitActions => HitActionsArr;

        public override int ViewAnimId => AnimId.ThrustBeam;
        public override bool ViewGrounded => false;      // 腰位光束，不用贴地渲染

        // 视觉锚回 DNF PO 原点（=施法者位置）：PO 贴图 imagePos(-170,-233) 以自身原点为锚，
        // GO 若停在碰撞中心(1.65,0.55)会叠加 imagePos 抬升 → 飘到角色右上方。
        // 补偿后视觉=DNF 原版（刀刃处），碰撞盒仍在身前 0.9-2.4 不变。
        public override TSVector ViewOffset => new(-(FP)165 / 100, -(FP)55 / 100, FP.Zero);
    }
}
