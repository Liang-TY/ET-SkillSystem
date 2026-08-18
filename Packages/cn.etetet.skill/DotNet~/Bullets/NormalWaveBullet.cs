using TrueSync;

namespace ET
{
    /// <summary>地裂·波动剑弹：快速穿透地波，命中 伤害+硬直+受击动画（复用 MeleeHit）。</summary>
    [BulletId(BulletIds.NormalWave)]
    public class NormalWaveBullet : BulletDefinition
    {
        public override FP Speed => 15;                 // 单位/秒（DNF 波动很快）
        public override int TotalTimeMs => 1500;        // 射程 ~22 单位
        public override bool DestroyOnHit => false;     // 穿透（DNF 波穿过敌人）

        public override TSVector HalfExtents => new((FP)5 / 10, (FP)4 / 10, (FP)3 / 10);

        private static readonly int[] HitActionsArr = { ActionIds.MeleeHit };
        public override int[] HitActions => HitActionsArr;

        public override int ViewAnimId => AnimId.NormalWave;
    }
}
