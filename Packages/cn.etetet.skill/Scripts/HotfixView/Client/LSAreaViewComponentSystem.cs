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
            List<long> finished = null;   // 收尾动画播完的区域（循环中不改字典，收集后统一销毁）
            foreach (var kv in self.Areas)
            {
                LSArea area = areaComponent.GetChild<LSArea>(kv.Key);
                AreaViewInfo info = kv.Value;

                if (area != null)
                    info.Go.transform.position = new Vector3((float)area.Position.x, 0f, (float)area.Position.z);

                // 主层（主动画播完 = 视图生命周期终点）；背面层独立帧推进（播完停末帧，不触发销毁）
                bool done = AdvanceOne(info.Renderer, info.OriginalMaterial, info.AnimId,
                    ref info.FrameIndex, ref info.Timer, info.Go, res, Time.deltaTime);
                if (info.BackRenderer != null)
                {
                    AdvanceOne(info.BackRenderer, info.BackOriginalMaterial, info.BackAnimId,
                        ref info.BackFrameIndex, ref info.BackTimer, info.Go, res, Time.deltaTime);
                }
                if (done)
                {
                    finished ??= new List<long>();
                    finished.Add(kv.Key);
                }
            }
            if (finished != null)
            {
                foreach (long id in finished) RemoveView(self, id);   // 收尾动画播完 → 销毁
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

            // 背面层（爆炸前后两层，如浴血之怒 boomback）：取 prefab 第二个 renderer（子 GO "1"），
            // 排主层之后（4 < 5），独立帧推进
            SpriteRenderer backRenderer = null;
            if (def.ViewBackAnimId != AnimId.None)
            {
                foreach (SpriteRenderer r in go.GetComponentsInChildren<SpriteRenderer>(true))
                {
                    if (r == renderer || r.sortingOrder != 1) continue;
                    backRenderer = r;
                    break;
                }
                if (backRenderer != null) backRenderer.sortingOrder = 4;   // 主层(5)之后
                else Log.Warning("[AreaView] 找不到背面层 renderer（prefab 子 GO sortingOrder=1），背面层跳过");
            }

            self.Areas[area.Id] = new AreaViewInfo
            {
                Go = go,
                Renderer = renderer,
                OriginalMaterial = renderer.sharedMaterial,
                BackRenderer = backRenderer,
                BackOriginalMaterial = backRenderer != null ? backRenderer.sharedMaterial : null,
                AnimId = def.ViewAnimId,
                EndAnimId = def.ViewEndAnimId,
                FrameIndex = 0,
                Timer = 0,
                BackAnimId = def.ViewBackAnimId,
                BackFrameIndex = 0,
                BackTimer = 0,
                Ending = false,
            };
        }

        private static void RemoveView(LSAreaViewComponent self, long areaId)
        {
            if (!self.Areas.TryGetValue(areaId, out AreaViewInfo info)) return;
            if (info.Go != null) UnityEngine.Object.Destroy(info.Go);
            self.Areas.Remove(areaId);
        }

        /// <summary>
        /// 渲染时间自推帧（循环动画持续播；非循环播完停末帧返回 true）。
        /// 主层与背面层共用：主层返回 true = 视图销毁；背面层返回值忽略（播完停末帧）。
        /// </summary>
        private static bool AdvanceOne(SpriteRenderer renderer, Material originalMaterial, int animId,
            ref int frameIndex, ref float timer, GameObject go, LSAnimResComponent res, float dt)
        {
            AnimClipData clip = AnimConfigRegistry.Get(animId);
            if (clip?.frames == null || clip.frames.Length == 0) return true;   // 无帧数据视为播完

            timer += dt;
            float delay = clip.frames[frameIndex].delay / 1000f;
            if (delay <= 0) delay = 0.05f;

            while (timer >= delay)
            {
                timer -= delay;
                frameIndex++;
                if (frameIndex >= clip.frames.Length)
                {
                    if (clip.loop)
                    {
                        frameIndex = 0;   // 循环
                    }
                    else
                    {
                        frameIndex = clip.frames.Length - 1;
                        return true;   // 播完 → 停末帧
                    }
                }
                delay = clip.frames[frameIndex].delay / 1000f;
                if (delay <= 0) delay = 0.05f;
            }

            AnimFrameData frame = clip.frames[frameIndex];
            Sprite sprite = res?.GetSprite(frame.image.path, frame.image.index);
            renderer.sprite = sprite;

            // §2.1 绝对摆位（同单位/弹）
            Vector2 center = res?.GetFrameCenter(frame.image.path, frame.image.index) ?? Vector2.zero;
            Transform parentT = renderer.transform.parent;
            Vector3 chain = parentT != null ? parentT.position - go.transform.position : Vector3.zero;
            renderer.transform.localPosition = new Vector3(
                (frame.imagePos.x + center.x) / 100f - chain.x,
                -(frame.imagePos.y + center.y) / 100f - chain.y,
                0f);

            // 加法混合（火圈/爆炸 LINEARDODGE）
            if (frame.graphicEffect == 1 && res != null && res.AdditiveMaterial != null)
                renderer.sharedMaterial = res.AdditiveMaterial;
            else if (originalMaterial != null)
                renderer.sharedMaterial = originalMaterial;

            return false;   // 还在播
        }
    }
}
