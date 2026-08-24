using System;
using UnityEngine;
using YIUIFramework;
using System.Collections.Generic;

namespace ET.Client
{

    /// <summary>
    /// 由YIUI工具自动创建 请勿修改
    /// </summary>
    [YIUI(EUICodeType.Panel, EPanelLayer.Scene)]
    [ComponentOf(typeof(YIUIChild))]
    public partial class BattleInfoPanelComponent : Entity, IDestroy, IAwake, IYIUIBind, IYIUIInitialize, IYIUIOpen
    {
        public const string PkgName = "Battle";
        public const string ResName = "BattleInfoPanel";

        public EntityRef<YIUIChild> u_UIBase;
        public YIUIChild UIBase => u_UIBase;
        public EntityRef<YIUIWindowComponent> u_UIWindow;
        public YIUIWindowComponent UIWindow => u_UIWindow;
        public EntityRef<YIUIPanelComponent> u_UIPanel;
        public YIUIPanelComponent UIPanel => u_UIPanel;
        public UnityEngine.UI.Text u_ComTextPlayerName;
        public UnityEngine.UI.Image u_ComImgPlayerHp;
        public UnityEngine.UI.Text u_ComTextMonsterName;
        public UnityEngine.UI.Image u_ComImgMonsterHp;

    }
}