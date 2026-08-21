using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// .als 特效叠加视图系统（单位侧）：AnimId 变化 → 差分重建叠加组；推进/销毁全部走
    /// LSAnimOverlayUtil 共享助手（区域视图同源，见 LSAreaViewComponentSystem）。
    /// </summary>
    [EntitySystemOf(typeof(LSAnimOverlayViewComponent))]
    [LSEntitySystemOf(typeof(LSAnimOverlayViewComponent))]
    [FriendOf(typeof(LSAnimOverlayViewComponent))]
    [FriendOf(typeof(LSUnitView))]
    [FriendOf(typeof(LSAnimComponent))]
    public static partial class LSAnimOverlayViewComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LSAnimOverlayViewComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this LSAnimOverlayViewComponent self)
        {
            LSAnimOverlayUtil.DestroyOverlays(self.Overlays);
            LSAnimOverlayUtil.DestroyOverlays(self.Detaching);
        }

        [LSEntitySystem]
        private static void LSRollback(this LSAnimOverlayViewComponent self)
        {
            // 回滚后父动画/帧号可能重置：清缓存强制下一帧重建（重建会重置各层帧计时）
            self.LastParentAnimId = -1;
            LSAnimOverlayUtil.DestroyOverlays(self.Overlays);
            LSAnimOverlayUtil.DestroyOverlays(self.Detaching);
        }

        [EntitySystem]
        private static void Update(this LSAnimOverlayViewComponent self)
        {
            LSUnitView view = self.GetParent<LSUnitView>();
            LSUnit unit = view.Unit;   // EntityRef<LSUnit> 隐式转 LSUnit（struct 不能直接 ?.）
            LSAnimComponent anim = unit?.GetComponent<LSAnimComponent>();
            if (anim == null)
            {
                if (self.Overlays.Count > 0)
                {
                    LSAnimOverlayUtil.DestroyOverlays(self.Overlays);
                    self.LastParentAnimId = -1;
                }
                LSAnimResComponent resNoAnim = self.Room()?.GetComponent<LSAnimResComponent>();
                LSAnimOverlayUtil.AdvanceDetaching(self.Detaching, resNoAnim, Time.deltaTime);
                return;
            }

            // 1) 父动画变化 → 现有层转入 Detaching 继续播完（DNF none-effect 语义），再重建叠加组
            //    （绝大多数动画无 overlay 配置，这里只是一次字典查询）
            if (anim.AnimId != self.LastParentAnimId)
            {
                foreach (OverlayViewInfo info in self.Overlays) self.Detaching.Add(info);
                self.Overlays.Clear();
                self.LastParentAnimId = anim.AnimId;
                // 单位身体层动态 sortingOrder ≈ -(z*100)（通常 ~0），层号直译：负 = 身后，正/10001+ = 身前
                List<OverlayViewInfo> overlays = LSAnimOverlayUtil.CreateOverlays(
                    view.GameObject.transform, anim.AnimId, 0);
                foreach (OverlayViewInfo info in overlays) self.Overlays.Add(info);
            }

            LSAnimResComponent res = self.Room()?.GetComponent<LSAnimResComponent>();

            // 2) 推进：startFrame 门控读逻辑层父动画帧号 + 渲染时间自推帧
            if (self.Overlays.Count > 0)
                LSAnimOverlayUtil.AdvanceOverlays(self.Overlays, anim.FrameIndex, res, Time.deltaTime);

            // 3) 脱离层收尾推进（播完自毁）
            LSAnimOverlayUtil.AdvanceDetaching(self.Detaching, res, Time.deltaTime);
        }
    }
}
