using TrueSync;

namespace ET
{
    /// <summary>连突刺剑气弹；参数位于 SkillParams/bullets/thrustbeam.json。</summary>
    [BulletId(BulletIds.ThrustBeam)]
    public class ThrustBeamBullet : ParametricBulletDefinition
    {
        public override int ConfiguredBulletId => BulletIds.ThrustBeam;
    }
}
