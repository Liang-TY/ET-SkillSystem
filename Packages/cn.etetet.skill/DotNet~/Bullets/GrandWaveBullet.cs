using TrueSync;

namespace ET
{
    /// <summary>邪光斩慢速波；参数位于 SkillParams/bullets/grandwave.json。</summary>
    [BulletId(BulletIds.GrandWave)]
    public class GrandWaveBullet : ParametricBulletDefinition
    {
        public override int ConfiguredBulletId => BulletIds.GrandWave;
    }
}
