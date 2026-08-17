namespace ET
{
    public static partial class LSUnitFactory
    {
        public static LSUnit Init(LSWorld lsWorld, LockStepUnitInfo unitInfo)
        {
	        LSUnitComponent lsUnitComponent = lsWorld.GetComponent<LSUnitComponent>();
	        LSUnit lsUnit = lsUnitComponent.AddChildWithId<LSUnit>(unitInfo.PlayerId);
			
	        lsUnit.Position = unitInfo.Position;
	        lsUnit.Rotation = unitInfo.Rotation;

			lsUnit.AddComponent<LSInputComponent>();
            lsUnit.AddComponent<LSAnimComponent>();   // Half B: 动画状态（Awake 自动 Play(Idle)）

            // 数值组件（skill 包）：HP/速度/攻击力 等
            var num = lsUnit.AddComponent<LSNumericComponent>();
            num.Set(NumericType.HpBase, 1000);
            num.Set(NumericType.MaxHpBase, 1000);

            // 战斗状态（硬直计时，默认动画 Idle）+ 输入缓冲。
            // 必须先于 LSHitboxComponent 挂（组件 Id 序 = LSUpdate 执行序）：命中写在 Hitbox 系统里，
            // Combat 先记上帧值再递减 → 视图层能 diff 到 0→>0 "刚被击中"边沿
            lsUnit.AddComponent<LSCombatComponent, int>(AnimId.Idle);
            lsUnit.AddComponent<LSInputBufferComponent>();

            // 命中盒组件（skill 包）：受击/攻击盒采样 + 攻击动作状态机（帧驱动，判定帧=有 attackBoxes 的帧）
            lsUnit.AddComponent<LSHitboxComponent>();

            return lsUnit;
        }
    }
}
