namespace ET.Client
{
    /// <summary>
    /// 战斗 HUD 轮询（L6）：每帧读怪物 Hp → 更新 BattleInfo（血条+飘字触发）。
    /// 用 View 层轮询而非 skill 包发事件——避免触碰 skill 热更 DLL 编译链。
    /// 注意：本类在 ModelView 程序集（不引用 TrueSync），Hp 缓存用 float（AsFloat 后的值）；
    /// FP 运算全部留在 HotfixView 的 Update 里做。
    /// </summary>
    [ComponentOf(typeof(Room))]
    public class BattleHudUnitComponent: Entity, IAwake<BattleInfoPanelComponent>, IUpdate, IDestroy
    {
        public EntityRef<BattleInfoPanelComponent> m_Panel;
        public BattleInfoPanelComponent Panel => m_Panel;

        public long LastMonsterId;
        public float LastMonsterHp;
        public bool MonsterShown;
    }
}
