namespace ET
{
    [FriendOf(typeof(LSCombatComponent))]   // 工厂写 HurtAnimId（ET0002）
    public static partial class LSUnitFactory
    {
        public static LSUnit Init(LSWorld lsWorld, LockStepUnitInfo unitInfo)
        {
	        LSUnitComponent lsUnitComponent = lsWorld.GetComponent<LSUnitComponent>();
	        LSUnit lsUnit = lsUnitComponent.AddChildWithId<LSUnit>(unitInfo.PlayerId);
			
	        lsUnit.Position = unitInfo.Position;
	        lsUnit.Rotation = unitInfo.Rotation;

			lsUnit.AddComponent<LSInputComponent>();
            lsUnit.AddComponent<LSAnimComponent>().Play(AnimId.SwordmanIdle);   // 鬼剑士待机（覆盖默认 bantu Idle）

            // 数值组件（skill 包）：HP/速度/攻击力 等
            var num = lsUnit.AddComponent<LSNumericComponent>();
            num.Set(NumericType.HpBase, 1000);
            num.Set(NumericType.MaxHpBase, 1000);

            // 战斗状态（硬直计时，默认动画 Idle）+ 输入缓冲。
            // 必须先于 LSHitboxComponent 挂（组件 Id 序 = LSUpdate 执行序）：命中写在 Hitbox 系统里，
            // Combat 先记上帧值再递减 → 视图层能 diff 到 0→>0 "刚被击中"边沿
            lsUnit.AddComponent<LSCombatComponent, int>(AnimId.SwordmanIdle);   // 鬼剑士待机
            lsUnit.GetComponent<LSCombatComponent>().HurtAnimId = AnimId.SwordmanHurt;
            lsUnit.AddComponent<LSInputBufferComponent>();

            // 击退/浮空飞行（push aside/lift up）：静默组件，被 LaunchOwner 激活
            lsUnit.AddComponent<LSFlightComponent>();

            // 技能（阶段4）：先于 Hitbox 挂——清 cast 标记 + 消费缓冲施放，都在 Hitbox 设 JustHit 之前
            lsUnit.AddComponent<LSSkillComponent>();
            lsUnit.AddComponent<LSCastComponent>();

            // Buff 容器（阶段5）：先于 Hitbox 挂（命中挂 Buff 设 JustAdded 在清标记之后）
            lsUnit.AddComponent<LSBuffComponent>();

            // 命中盒组件（skill 包）：受击/攻击盒采样 + 命中检测结算（攻击动作状态机在 Cast 框架）
            lsUnit.AddComponent<LSHitboxComponent>();

            return lsUnit;
        }
    }
}
