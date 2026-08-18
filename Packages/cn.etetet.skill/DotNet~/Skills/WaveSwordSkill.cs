namespace ET
{
    /// <summary>
    /// 地裂·波动剑（I 键）：向前发一道穿透地波，命中怪物 伤害+硬直（配置在 NormalWaveBullet）。
    /// 阶段6 验证投射物链路（CreateBullet → 飞行 → 碰撞 → HitActions）。
    /// </summary>
    [SkillId(SkillIds.WaveSword)]
    public class WaveSwordSkill : SkillLogic
    {
        public override int CooldownMs => 2000;
        public override int TotalTimeMs => 350;   // 施放动作时长（暂不换角色动画，原地发波）

        public override void OnCast(SkillContext ctx)
        {
            ctx.CreateBullet(BulletIds.NormalWave);
        }
    }
}
