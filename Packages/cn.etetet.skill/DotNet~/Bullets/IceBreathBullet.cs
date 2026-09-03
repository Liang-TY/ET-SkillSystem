using TrueSync;

namespace ET
{
    /// <summary>冰息弹；参数位于 SkillParams/bullets/icebreath.json。</summary>
    [BulletId(BulletIds.IceBreath)]
    public class IceBreathBullet : ParametricBulletDefinition
    {
        public override int ConfiguredBulletId => BulletIds.IceBreath;
    }
}
