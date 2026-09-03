using TrueSync;

namespace ET
{
    /// <summary>地裂·波动剑弹；参数位于 SkillParams/bullets/normalwave.json。</summary>
    [BulletId(BulletIds.NormalWave)]
    public class NormalWaveBullet : ParametricBulletDefinition
    {
        public override int ConfiguredBulletId => BulletIds.NormalWave;
    }
}
