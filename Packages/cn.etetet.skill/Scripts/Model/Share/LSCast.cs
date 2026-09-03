using System.Collections.Generic;
using MemoryPack;
using TrueSync;

namespace ET
{
    /// <summary>
    /// 一次技能施放实例（纯数据——ET 数据/逻辑分离，行为在 LSCastSystem/SkillLogic）。
    /// SkillLogic 无状态共享单例（SkillLoader.Get），技能运行时状态全存这里，回滚快照恢复。
    /// 生命周期：LSSkillComponentSystem.TryCast → LSCastSystem.Create（调 OnCast）→
    /// 每帧 LSCastSystem.LSUpdate（调 OnUpdate）→ TotalTimeMs 到时或技能自结束 → EndNow（调 OnEnd）。
    /// Route B：Just* 标记由 LSSkillComponentSystem 开头清（它先于 Hitbox/Cast 跑，设标记者都在清之后），
    /// 视图层轮询 diff（阶段7）。
    /// </summary>
    [ChildOf(typeof(LSCastComponent))]
    [MemoryPackable]
    public partial class LSCast : LSEntity, ILSUpdate, IAwake<int>, ISerializeToEntity
    {
        [MemoryPackOrder(0)]
        public int SkillId;

        [MemoryPackOrder(1)]
        public long CasterId;

        [MemoryPackOrder(2)]
        public int ElapsedMs;

        [MemoryPackOrder(3)]
        public bool Finished;

        // 施放时采样的目标点（投射物/位移技能用）
        [MemoryPackOrder(4)]
        public TSVector TargetPosition;

        // 总时长 ms（创建时从 SkillLogic.TotalTimeMs 拷入）；>0 到时自动 OnEnd，0=技能自己控制结束
        [MemoryPackOrder(5)]
        public int TotalTimeMs;

        // 已命中目标（LSHitboxComponentSystem.ApplyHit 回写；新增时 JustHit 置位）
        [MemoryPackOrder(6)]
        public List<long> TargetIds = new();

        // 技能专用相位（各技能按需用）
        [MemoryPackOrder(7)]
        public int Phase;

        // 连段子状态（DNF setSkillSubState 机制，三段斩实证；连段技能用）
        [MemoryPackOrder(8)]
        public int SubState;

        // --- Route B 状态标记（视图层轮询 diff 用）---
        [MemoryPackOrder(9)]
        public bool JustStarted;

        [MemoryPackOrder(10)]
        public bool JustHit;

        [MemoryPackOrder(11)]
        public bool JustFinished;

        // 参数化技能事件游标（回滚安全）：位图中的第 N 位表示该 cast 级事件
        // 已执行；PhaseEventMask 在进入新 phase 时清零，HitEventMask 记录一次性命中事件。
        // 当前编辑器限制每类事件最多 64 个，超过时由参数校验拒载。
        [MemoryPackOrder(12)]
        public ulong SpawnEventMask;

        [MemoryPackOrder(13)]
        public ulong PhaseEventMask;

        [MemoryPackOrder(14)]
        public ulong HitEventMask;

        [MemoryPackOrder(15)]
        public ulong PhaseHitEventMask;

        // 最近一次命中的目标，用于参数化 hitEvent 的 target 策略。
        [MemoryPackOrder(16)]
        public long LastHitTargetId;

        // OncePerTargetInPhase 按 hitEvent 索引分别去重；随实体快照恢复。
        [MemoryPackOrder(17)]
        public Dictionary<int, List<long>> HitEventTargets = new();

        // 本 tick 已完成结算的目标序列；允许同一 tick 多目标逐个触发 hitEvent。
        [MemoryPackOrder(18)]
        public List<long> ResolvedHitTargets = new();

        // Hitbox 在 Cast 逻辑更新之后运行。该列表跨一个逻辑 tick 保存待由
        // SkillLogic 消费的目标；ResolvedHitTargets 则只给视图/日志保留本帧结果。
        [MemoryPackOrder(19)]
        public List<long> PendingHitTargets = new();
    }
}
