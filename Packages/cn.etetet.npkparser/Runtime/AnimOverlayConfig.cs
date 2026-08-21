using System;

namespace ET
{
    /// <summary>
    /// .als 特效叠加配置（DNF .ani 同名边车的翻译产物，JsonUtility 反序列化）。
    /// 挂接在父动画 AnimId 上：父动画播放时，视图层（LSAnimOverlayViewComponent）
    /// 按 startFrame 起播各叠加层特效——DNF 引擎零代码声明式叠加的同构实现。
    /// </summary>
    [EnableClass]
    [Serializable]
    public class AnimOverlayConfig
    {
        public AnimOverlayEntry[] overlays;
    }

    /// <summary>
    /// 一条叠加层：父动画到 startFrame 起在 z 层播放 effectAni 特效动画。
    /// 帧号/层号 DNF 原样直译，游戏侧解释。
    /// </summary>
    [Serializable]
    public class AnimOverlayEntry
    {
        public int startFrame;      // -1 = 全帧生效（DNF .als [add] 帧号）
        public int z;               // 层号：负 = 施法者身后；10001+ = DNF 前景标记段（身前）；-10001+ = 深后景
        public string effectAni;    // 特效动画别名（.als [use animation] 注册名，调试用）

        /// <summary>注册时由别名解析填充（AnimId；0=未映射——如空占位动画，视图层跳过该层）</summary>
        [NonSerialized] public int effectAnimId;
    }
}
