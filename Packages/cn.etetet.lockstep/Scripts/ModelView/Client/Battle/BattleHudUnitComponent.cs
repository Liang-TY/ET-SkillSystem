using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ET.Client
{
    /// <summary>
    /// 战斗 HUD 轮询（L6）：每帧读怪物 Hp → 更新 BattleInfo（血条+飘字触发）。
    /// 用 View 层轮询而非 skill 包发事件——避免触碰 skill 热更 DLL 编译链。
    /// 注意：本类在 ModelView 程序集（不引用 TrueSync 之外的 FP 运算），Hp 缓存用 float。
    /// 飘字状态挂本实体（Hotfix 无状态红线 ET0004/0005：数据归实体，行为归 System）。
    /// </summary>
    [ComponentOf(typeof(Room))]
    public class BattleHudUnitComponent: Entity, IAwake<BattleInfoPanelComponent>, IUpdate, IDestroy
    {
        public EntityRef<BattleInfoPanelComponent> m_Panel;
        public BattleInfoPanelComponent Panel => m_Panel;

        public long LastMonsterId;
        public float LastMonsterHp;
        public bool MonsterShown;

        // 飘字三态平行表（下标对应）
        public readonly List<Text> FloatTexts = new List<Text>();
        public readonly List<RectTransform> FloatRects = new List<RectTransform>();
        public readonly List<float> FloatElapsed = new List<float>();
    }
}
