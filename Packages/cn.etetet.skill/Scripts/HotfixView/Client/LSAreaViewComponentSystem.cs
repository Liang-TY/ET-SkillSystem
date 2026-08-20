using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 区域效果视图系统（同弹视图模式：差分建/销 GO + 渲染时间自推帧 + 绝对摆位公式）。
    /// 循环动画（火圈 loop=true）持续播；到时（JustRemoved）切收尾动画（loop=false），播完销毁。
    /// </summary>
    [EntitySystemOf(typeof(LSAreaViewComponent))]
    [FriendOf(typeof(LSAreaViewComponent))]
    [FriendOf(typeof(LSArea))]
    [FriendOf(typeof(LSAnimResComponent))]
    public static partial class LSAreaViewComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LSAreaViewComponent self)
        {
        }

        /// <summary>加载 Unit2D 预制体（同弹视图）</summary>
        public static async ETTask InitAsync(this LSAreaViewComponent self)
        {
            Room room = self.GetParent<Room>();
            ResourcesLoaderComponent resLoader = room.GetComponent<ResourcesLoaderComponent>();
            self.Prefab = await resLoader.LoadAssetAsync<GameObject>(
                "Packages/cn.etetet.lockstep/Bundles/Unit/Unit2D.prefab");
        }

        [EntitySystem]
        private static void Destroy(this LSAreaViewComponent self)
        {
            foreach (var kv in self.Areas)
            {
                if (kv.Value.Go != null) UnityEngine.Object.Destroy(kv.Value.Go);
            }
            self.Areas.Clear();
        }

        [EntitySystem]
        private static void Update(this LSAreaViewComponent self)
        {
            Room room = self.GetParent<Room>();
            LSWorld world = room.LSWorld;
            LSAreaComponent areaComponent = world?.GetComponent<LSAreaComponent>();
            LSAnimResComponent res = room.GetComponent<LSAnimResComponent>();

            // 1) 差分：新区域建 GO（+JustRemoved → 切收尾动画）
            if (areaComponent != null)
            {
                foreach (var kv in areaComponent.Children)
                {
                    if (kv.Value is not LSArea area) continue;
                    if (self.Areas.ContainsKey(area.Id))
                    {
                        // 已有：检测到时（JustRemoved → 切收尾）
                        AreaViewInfo info = self.Areas[area.Id];
                        if (area.JustRemoved && !info.Ending && info.EndAnimId != AnimId.None)
                        {
                            info.Ending = true;
                            info.FrameIndex = 0;
                            info.Timer = 0;
                            info.AnimId = info.EndAnimId;
                        }
                        continue;
                    }
                    CreateView(self, area);
                }

                // 消失（逻辑已 Dispose 的）→ 销毁 GO
                List<long> removed = null;
                foreach (var kv in self.Areas)
                {
                    if (areaComponent.GetChild<LSArea>(kv.Key) != null) continue;
                    removed ??= new List<long>();
                    removed.Add(kv.Key);
                }
                if (removed != null)
                {
                    foreach (long id in removed) RemoveView(self, id);
                }
            }
            else if (self.Areas.Count > 0)
            {
                foreach (long id in new List<long>(self.Areas.Keys)) RemoveView(self, id);
            }

            // 2) 推进：位置 + 帧自推（循环的持续播，收尾的播完销毁）
            if (areaComponent == null) return;
            foreach (var kv in self.Areas)
            {
                LSArea area = areaComponent.GetChild<LSArea>(kv.Key);
                AreaViewInfo info = kv.Value;

                if (area != null)
                    info.Go.transform.position = new Vector3((float)area.Position.x, 0f, (float)area.Position.z);

                bool done = AdvanceFrame(info, res, Time.deltaTime);
                if (done) RemoveView(self, kv.Key);   // 收尾动画播完 → 销毁
            }
        }

        private static void CreateView(LSAreaViewComponent self, LSArea area)
        {
            AreaDefinition def = AreaLoader.Get(area.ConfigId);
            if (def == null || def.ViewAnimId == AnimId.None) return;
            if (self.Prefab == null) return;

            GlobalComponent globalComponent = self.Root().GetComponent<GlobalComponent>();
            GameObject go = UnityEngine.Object.Instantiate(self.Prefab, globalComponent.Unit, true);
            go.name = "Area";
            SpriteRenderer renderer = go.GetComponentInChildren<SpriteRenderer>();
            renderer.sortingOrder = 5;   // 单位之下弹之上

            self.Areas[area.Id] = new AreaViewInfo
            {
                Go = go,
                Renderer = renderer,
                OriginalMaterial = renderer.sharedMaterial,
                AnimId = def.ViewAnimId,
                EndAnimId = def.ViewEndAnimId,
                FrameIndex = 0,
                Timer = 0,
                Ending = false,
            };
        }

        private static void RemoveView(LSAreaViewComponent self, long areaId)
        {
            if (!self.Areas.TryGetValue(areaId, out AreaViewInfo info)) return;
            if (info.Go != null) UnityEngine.Object.Destroy(info.Go);
            self.Areas.Remove(areaId);
        }

        /// <summary>渲染时间自推帧（循环动画持续播；收尾播完返回 true 触发销毁）</summary>
        private static bool AdvanceFrame(AreaViewInfo info, LSAnimResComponent res, float dt)
        {
            AnimClipData clip = AnimConfigRegistry.Get(info.AnimId);
            if (clip?.frames == null || clip.frames.Length == 0) return !info.Ending;

            info.Timer += dt;
            float delay = clip.frames[info.FrameIndex].delay / 1000f;
            if (delay <= 0) delay = 0.05f;

            while (info.Timer >= delay)
            {
                info.Timer -= delay;
                info.FrameIndex++;
                if (info.FrameIndex >= clip.frames.Length)
                {
                    if (clip.loop)
                    {
                        info.FrameIndex = 0;   // 循环
                    }
                    else
                    {
                        info.FrameIndex = clip.frames.Length - 1;
                        return true;   // 收尾播完 → 销毁
                    }
                }
                delay = clip.frames[info.FrameIndex].delay / 1000f;
                if (delay <= 0) delay = 0.05f;
            }

            AnimFrameData frame = clip.frames[info.FrameIndex];
            Sprite sprite = res?.GetSprite(frame.image.path, frame.image.index);
            info.Renderer.sprite = sprite;

            // §2.1 绝对摆位（同单位/弹）
            Vector2 center = res?.GetFrameCenter(frame.image.path, frame.image.index) ?? Vector2.zero;
            Transform parentT = info.Renderer.transform.parent;
            Vector3 chain = parentT != null ? parentT.position - info.Go.transform.position : Vector3.zero;
            info.Renderer.transform.localPosition = new Vector3(
                (frame.imagePos.x + center.x) / 100f - chain.x,
                -(frame.imagePos.y + center.y) / 100f - chain.y,
                0f);

            // 加法混合（火圈 LINEARDODGE）
            if (frame.graphicEffect == 1 && res != null && res.AdditiveMaterial != null)
                info.Renderer.sharedMaterial = res.AdditiveMaterial;
            else if (info.OriginalMaterial != null)
                info.Renderer.sharedMaterial = info.OriginalMaterial;

            return false;   // 还在播
        }
    }
}
