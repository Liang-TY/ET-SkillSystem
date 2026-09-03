using TrueSync;

namespace ET
{
    /// <summary>按整数 bulletId 读取 JSON 参数的定义基类。</summary>
    public abstract class ParametricBulletDefinition : BulletDefinition
    {
        public virtual int ConfiguredBulletId => 0;

        private BulletParam Param => ConfiguredBulletId > 0 ? SkillParamLoader.GetBullet(ConfiguredBulletId) : null;

        public override FP Speed => Param?.Speed ?? base.Speed;
        public override int TotalTimeMs => Param?.TotalTimeMs ?? base.TotalTimeMs;
        public override TSVector HalfExtents => Param?.HalfExtents ?? base.HalfExtents;
        public override bool DestroyOnHit => Param?.DestroyOnHit ?? base.DestroyOnHit;
        public override int[] HitActions => Param?.HitActions ?? base.HitActions;
        public override HitReaction HitReaction => Param?.HitReaction ?? base.HitReaction;
        public override TSVector SpawnOffset => Param?.SpawnOffset ?? base.SpawnOffset;
        public override bool ViewGrounded => Param?.ViewGrounded ?? base.ViewGrounded;
        public override TSVector ViewOffset => Param?.ViewOffset ?? base.ViewOffset;
        public override int HitResetIntervalMs => Param?.HitResetIntervalMs ?? base.HitResetIntervalMs;
        public override int ViewAnimId => Param?.ViewAnimId ?? base.ViewAnimId;
    }

    /// <summary>按整数 areaId 读取 JSON 参数的定义基类。</summary>
    public abstract class ParametricAreaDefinition : AreaDefinition
    {
        public virtual int ConfiguredAreaId => 0;

        private AreaParam Param => ConfiguredAreaId > 0 ? SkillParamLoader.GetArea(ConfiguredAreaId) : null;

        public override int TotalTimeMs => Param?.TotalTimeMs ?? base.TotalTimeMs;
        public override int TickTimeMs => Param?.TickTimeMs ?? base.TickTimeMs;
        public override TSVector HalfExtents => Param?.HalfExtents ?? base.HalfExtents;
        public override int[] EnterActions => Param?.EnterActions ?? base.EnterActions;
        public override int[] TickActions => Param?.TickActions ?? base.TickActions;
        public override int[] ExitActions => Param?.ExitActions ?? base.ExitActions;
        public override HitReaction HitReaction => Param?.HitReaction ?? base.HitReaction;
        public override int ViewAnimId => Param?.ViewAnimId ?? base.ViewAnimId;
        public override int ViewEndAnimId => Param?.ViewEndAnimId ?? base.ViewEndAnimId;
        public override int ViewBackAnimId => Param?.ViewBackAnimId ?? base.ViewBackAnimId;
    }

    /// <summary>按整数 buffId 读取 JSON 参数的定义基类。</summary>
    public abstract class ParametricBuffDefinition : BuffDefinition
    {
        public virtual int ConfiguredBuffId => 0;

        private BuffParam Param => ConfiguredBuffId > 0 ? SkillParamLoader.GetBuff(ConfiguredBuffId) : null;

        public override int TotalTimeMs => Param?.DurationMs ?? base.TotalTimeMs;
        public override int TickTimeMs => Param?.TickTimeMs ?? base.TickTimeMs;
        public override int MaxStacks => Param?.MaxStacks ?? base.MaxStacks;
        public override bool RefreshOnApply => Param?.RefreshOnApply ?? base.RefreshOnApply;
        public override int[] AddActions => Param?.AddActions ?? base.AddActions;
        public override int[] TickActions => Param?.TickActions ?? base.TickActions;
        public override int[] RemoveActions => Param?.RemoveActions ?? base.RemoveActions;
    }
}
