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

            // 命中盒组件（skill 包）：受击盒采样 + 攻击盒（阶段2临时 AttackEnabled 供碰撞验证）
            lsUnit.AddComponent<LSHitboxComponent>().SetAttackEnabled(true);

            return lsUnit;
        }
    }
}
