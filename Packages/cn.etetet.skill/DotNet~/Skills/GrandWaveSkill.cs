using TrueSync;

namespace ET
{
    /// <summary>
    /// 鬼剑士·邪光斩（慢速爬行穿透波，grandwave.nut 仅门禁壳——PO 数据反推重建）。
    /// DNF：剑气以 col0/3000 px/ms 慢速爬行，走到 30% 射程消散；每 350ms resetHitObjectList
    /// 对同目标多段结算（~3 跳）。命中反应 grandwave.atk：push 300 / lift 200 / blow。
    /// 蓄力（修罗邪光斩 51）/light 变体跳过；施法动画 = wave.ani 切片（DNF 引擎借用同款）。
    /// 参考：Notes/技能实现/鬼剑士技能解析/050-GrandWave.md
    /// </summary>
    [SkillId(SkillIds.GrandWave)]
    public class GrandWaveSkill : SkillLogic
    {
        public override int CooldownMs => 2000;    // .skl 10000（用户定案：本批 CD 全 2s）
        public override int TotalTimeMs => 500;    // 施法挥手（wave.ani 切片 F1-F8）

        public override void OnCast(SkillContext ctx)
        {
            ctx.SetSubState(0);
            ctx.PlayAnim(AnimId.SwordmanWaveCast);
        }

        public override void OnUpdate(SkillContext ctx, int dtMs)
        {
            // 挥剑中段发波（SubState 守卫一次性）
            if (ctx.GetSubState() == 0 && ctx.GetElapsedMs() >= 150)
            {
                ctx.SetSubState(1);
                ctx.CreateBullet(BulletIds.GrandWave);
            }
        }

        public override void OnEnd(SkillContext ctx)
        {
            ctx.PlayDefaultAnim();
        }
    }
}
