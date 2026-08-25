using TrueSync;

namespace ET.Client
{
    /// <summary>
    /// 战斗 HUD 轮询（L6）：每帧读怪物 Hp → 更新 BattleInfo（血条+飘字触发）。
    /// 用 View 层轮询而非 skill 包发事件——避免触碰 skill 热更 DLL 编译链。
    /// </summary>
    [ComponentOf(typeof(Room))]
    public class BattleHudUnitComponent: Entity, IAwake<BattleInfoPanelComponent>, IUpdate, IDestroy
    {
        public EntityRef<BattleInfoPanelComponent> m_Panel;
        public BattleInfoPanelComponent Panel => m_Panel;

        public long LastMonsterId;
        public FP LastMonsterHp;
        public bool MonsterShown;
    }
}
