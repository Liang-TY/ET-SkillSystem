namespace ET.Client
{
    /// <summary>
    /// DemoUI：怪物全灭 → 顶部倒计时提示（3 秒后 BattleEnd 回城，由 TownSceneInitFinish 收口关闭）。
    /// </summary>
    [Event(SceneType.LockStep)]
    [FriendOf(typeof(BattleTipPanelComponent))]
    public class MonsterAllDead_ShowBattleTip: AEvent<Scene, MonsterAllDead>
    {
        protected override async ETTask Run(Scene scene, MonsterAllDead args)
        {
            BattleTipPanelComponent tip = await scene.YIUIRoot().OpenPanelAsync<BattleTipPanelComponent>();
            if (tip != null)
                tip.u_ComTextTip.text = "怪物已全部消灭，3 秒后返回城镇…";
        }
    }
}
