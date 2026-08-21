using TrueSync;

namespace ET
{
    /// <summary>
    /// 波动爆发（Y 键；DNF mod 技能，E:\Projects\cs\dnf-pvf-learn\第四期nut技能波动爆发，state 156）。
    ///
    /// DNF 流程同构（releasewave.nut）：
    /// - onSetState：播冲刺动画（releasewavedash_body.ani 3 帧 230ms，.als 11 层特效由视图层自动叠加）
    ///   + 角色攻击信息 releasewave_light.atk（伤害/击退/浮空 → HitReaction）；
    /// - 帧 F0（SET FLAG 10001）：在施放点原地创建爆炸（→ ReleaseWaveArea，被动对象 24389 子id9 同构）；
    /// - 帧 F1/F2 攻击盒：.ani 自带 attackBox → 帧驱动自动激活（LSHitboxComponentSystem 判定帧表）；
    /// - onProc：动画时长内匀速前冲 300px（位移 = ElapsedMs 纯函数，无需存起点，回滚重放一致）；
    /// - onEndCurrentAni：回待机。
    /// 跳过（记档 §5）：霸体帧（SUPERARMOR ×3）、施放后僵直 600ms（static data）、
    /// 元素二选一（雷神之息 251 → 子id10 光属性版）、等级缩放（列1 爆发大小%）、MP 消耗。
    /// </summary>
    [SkillId(SkillIds.ReleaseWave)]
    public class ReleaseWaveSkill : SkillLogic
    {
        private static readonly FP DashDistance = (FP)3;   // DNF onProc sq_GetUniformVelocity(0,300,t,总时长) → 3 单位

        // releasewave_light.atk：魔法/击倒/push 400/lift 400/blow（demo：伤害 80，硬直 800ms 表现击倒）
        private static readonly HitReaction DashReaction = new()
        {
            Damage = 80,
            HitstunMs = 800,
            KnockbackX = 400,
            LaunchY = 400,
        };
        public override HitReaction HitReaction => DashReaction;

        public override int CooldownMs => 5000;     // 原版 15s 随等级递减，demo 固定 5s
        public override int TotalTimeMs => 230;     // 冲刺动画 3 帧（60+120+50）

        // 冲刺 F1/F2 攻击盒命中 → MeleeHit（读上面 DashReaction：伤害/击退/浮空）
        private static readonly int[] HitActionsArr = { ActionIds.MeleeHit };
        public override int[] HitActions => HitActionsArr;

        public override void OnCast(SkillContext ctx)
        {
            ctx.PlayAnim(AnimId.SwordmanReleaseWaveDash);
            ctx.ClearHitTargets();

            // 帧 F0（SET FLAG 10001）即时引爆：DNF flag 挂在首帧 = 施放瞬间，以施放点为中心（偏移 0,0,0）
            ctx.CreateAreaInFront(AreaIds.ReleaseWave, FP.Zero);
        }

        public override void OnUpdate(SkillContext ctx, int dtMs)
        {
            // 匀速前冲（DNF onProc 同构）：位移 = min(t, 总时长)/总时长 × 3 单位，按帧差走增量。
            // 纯函数（无起点状态）→ 回滚重放天然一致。
            // 注意：t0 用未钳制的 elapsed 算（最后一 tick elapsed=250 > 230，区间是 [200,230] 而非 [180,230]），
            // 先钳 t1 再算 t0 会把已走完的 20ms 再走一遍（总位移 3.26 而非 3）。
            int elapsed = ctx.GetElapsedMs();
            int t0 = elapsed - dtMs;
            if (t0 < 0) t0 = 0;
            int t1 = elapsed < TotalTimeMs ? elapsed : TotalTimeMs;
            if (t1 > t0)
                ctx.MoveCasterForward(DashDistance * (t1 - t0) / TotalTimeMs);
        }

        public override void OnEnd(SkillContext ctx)
        {
            ctx.PlayDefaultAnim();   // onEndCurrentAni → 回待机
        }
    }
}
