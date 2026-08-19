using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 技能/战斗表现视图组件（Room 级）：消费 LSCast 的 Just\* 标记 + LSBuff 的 Just\* 标记 + HP 变化。
    /// 本阶段：Log 钩子（UI 接入点标记）——血条/伤害数字/Buff 图标的消费方就在这里写。
    /// Route B：轮询逻辑标记，回滚安全。
    /// </summary>
    [ComponentOf(typeof(Room))]
    public class LSCastViewComponent : Entity, IAwake, IUpdate
    {
        /// <summary>每单位 HP 缓存（diff 检测扣血→飞伤害数字的接入点）</summary>
        public readonly Dictionary<long, int> LastHp = new();
    }
}
