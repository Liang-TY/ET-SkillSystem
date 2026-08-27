using TrueSync;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 城镇多人系统（阶段D）：本地上报（移动中 200ms/静止 1000ms，位置没变跳过，停止发终包）
    /// + 远端插值渲染（~200ms 平滑 + IsMoving 沿 Forward 外推）。开关默认关——单人为零开销。
    /// 实体字段是 Unity 类型（ModelView 无 TrueSync）；TSVector 转换集中在本文件（HotfixView 有 TrueSync）。
    /// </summary>
    [EntitySystemOf(typeof(TownRemotePlayerManagerComponent))]
    [FriendOf(typeof(TownRemotePlayerManagerComponent))]
    [FriendOf(typeof(TownRemotePlayerView))]
    [FriendOf(typeof(TownPlayerComponent))]
    public static partial class TownRemotePlayerManagerComponentSystem
    {
        /// <summary>同步总开关（默认关：单人 demo 不发包；多人测试置 true）</summary>
        private const bool EnableTownSync = false;

        private const int MovingSendIntervalMs = 200;

        private const int IdleSendIntervalMs = 1000;

        /// <summary>远端插值平滑系数（~200ms 内追上目标）</summary>
        private const float InterpolateFactor = 5f;

        [EntitySystem]
        private static void Awake(this TownRemotePlayerManagerComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this TownRemotePlayerManagerComponent self)
        {
            // 子实体（远端视图）随 Dispose 级联销毁，各自 Destroy 销 GO
        }

        [EntitySystem]
        private static void Update(this TownRemotePlayerManagerComponent self)
        {
            Room room = self.GetParent<Room>();
            if (room?.Name != "Town") return;
            TownPlayerComponent player = room.GetComponent<TownPlayerComponent>();
            if (player == null) return;

            // 逻辑坐标 → 显示坐标（屏幕映射 (x, z+y, 0)——与本地玩家视图同款）
            TSVector p = player.Position;
            TSVector f = player.Forward;
            Vector3 display = new((float)p.x, (float)(p.z + p.y), 0f);
            Vector3 forward = new((float)f.x, 0f, 0f);

            long now = TimeInfo.Instance.ClientNow();
            bool moving = display != self.LastFramePos;
            self.LastFramePos = display;

            // ---- 本地上报（开关关=完全跳过）----
            if (EnableTownSync)
            {
                ClientSenderComponent sender = self.Root().GetComponent<ClientSenderComponent>();
                bool needFinal = self.LastMoving && !moving;   // 停止移动瞬间发终包
                if ((moving || needFinal) && now >= self.NextSendTime
                    && (needFinal || display != self.LastSentPos || forward != self.LastSentForward))
                {
                    C2T_PositionUpdate update = C2T_PositionUpdate.Create();
                    update.Position = p;
                    update.Forward = f;
                    update.IsMoving = moving;
                    sender.Send(update);
                    self.LastSentPos = display;
                    self.LastSentForward = forward;
                    self.NextSendTime = now + (moving ? MovingSendIntervalMs : IdleSendIntervalMs);
                }
                else if (now >= self.NextSendTime)
                {
                    self.NextSendTime = now + (moving ? MovingSendIntervalMs : IdleSendIntervalMs);
                }
                self.LastMoving = moving;
            }

            // ---- 远端插值 ----
            float dt = Time.deltaTime;
            foreach (var kv in self.Children)
            {
                if (kv.Value is not TownRemotePlayerView view || view.Root == null) continue;

                // 目标点 = 最新收包位置 + 移动中沿 Forward 外推 0.5 秒提前量
                Vector3 target = view.TargetPos;
                if (view.IsMoving) target.x += view.TargetForward.x * 3f;   // 6 单位/s × 0.5s
                view.DisplayPos = Vector3.Lerp(view.DisplayPos, target, Mathf.Min(1f, dt * InterpolateFactor));
                view.Root.transform.position = view.DisplayPos;

                // 像素对齐（2026-08-27）：同本地玩家视图——Lerp 小数坐标 snap 到 1/100 单位。
                Vector3 rp = view.Root.transform.position;
                view.Root.transform.position = new Vector3(Mathf.Round(rp.x * 100f) / 100f, Mathf.Round(rp.y * 100f) / 100f, rp.z);

                bool faceRight = view.TargetForward.x >= 0f;
                if (faceRight != view.FaceRight)
                {
                    view.FaceRight = faceRight;
                    view.Root.transform.localScale = new Vector3(faceRight ? 1f : -1f, 1f, 1f);
                }

                view.AnimId = view.IsMoving ? AnimId.SwordmanWalk : AnimId.SwordmanIdle;
                AdvanceFrame(self, view, dt);
            }
        }

        /// <summary>创建远端角色视图（收到 EnterTown / 进城成员列表；display=屏幕坐标）</summary>
        public static void CreateRemote(this TownRemotePlayerManagerComponent self, long playerId, Vector3 display)
        {
            if (self.GetChild<TownRemotePlayerView>(playerId) != null) return;

            Room room = self.GetParent<Room>();
            if (room.GetComponent<LSAnimResComponent>() == null) return;   // 资源未就绪（下次再建）
            GameObject prefab = room.GetComponent<TownPlayerViewComponent>()?.UnitPrefab;
            if (prefab == null) return;   // 本地玩家视图没建好（下次再建）

            GlobalComponent globalComponent = self.Root().GetComponent<GlobalComponent>();
            GameObject go = UnityEngine.Object.Instantiate(prefab, globalComponent.Unit, true);
            go.name = $"TownRemote_{playerId}";

            TownRemotePlayerView view = self.AddChildWithId<TownRemotePlayerView>(playerId);
            view.PlayerId = playerId;
            view.Root = go;
            view.TargetPos = display;
            view.TargetForward = Vector3.right;
            view.DisplayPos = display;
            go.transform.position = display;

            // 鬼剑士 3 层渲染（同本地玩家/战斗——demo 无选角，全员同款）
            view.RenderConfig = new UnitRenderConfig();
            view.RenderConfig.Layers.Add(new RenderLayer { AtlasName = "sm_body0000.img", SortingOrder = 10 });
            view.RenderConfig.Layers.Add(new RenderLayer { AtlasName = "katana9200b.img", SortingOrder = 16 });
            view.RenderConfig.Layers.Add(new RenderLayer { AtlasName = "katana9200c.img", SortingOrder = 17 });
            SpriteRenderer[] allRenderers = go.GetComponentsInChildren<SpriteRenderer>(true);
            foreach (RenderLayer layer in view.RenderConfig.Layers)
            {
                foreach (SpriteRenderer r in allRenderers)
                {
                    if (r.sortingOrder != layer.SortingOrder) continue;
                    layer.Renderer = r;
                    layer.OriginalMaterial = r.sharedMaterial;
                    break;
                }
            }

            view.AnimId = AnimId.SwordmanIdle;
            Log.Info($"[Town] 远端玩家{playerId}进入视野 @({display.x:F2},{display.y:F2})");
        }

        /// <summary>移除远端角色（收到 LeaveTown）</summary>
        public static void RemoveRemote(this TownRemotePlayerManagerComponent self, long playerId)
        {
            TownRemotePlayerView view = self.GetChild<TownRemotePlayerView>(playerId);
            if (view == null) return;
            view.Dispose();
            Log.Info($"[Town] 远端玩家{playerId}离开视野");
        }

        /// <summary>更新远端目标（收到 PositionBroadcast；display=屏幕坐标）</summary>
        public static void UpdateRemote(this TownRemotePlayerManagerComponent self, long playerId,
            Vector3 display, Vector3 forward, bool isMoving)
        {
            TownRemotePlayerView view = self.GetChild<TownRemotePlayerView>(playerId);
            if (view == null)
            {
                self.CreateRemote(playerId, display);   // 广播先于 Enter 到达——补建
                return;
            }
            view.TargetPos = display;
            view.TargetForward = forward;
            view.IsMoving = isMoving;
        }

        // ---- 自推帧（与 TownPlayerViewComponentSystem 同款拷贝，城镇多人版）----
        private static void AdvanceFrame(TownRemotePlayerManagerComponent self, TownRemotePlayerView view, float dt)
        {
            if (view.RenderConfig == null) return;
            AnimClipData clip = AnimConfigRegistry.Get(view.AnimId);
            if (clip?.frames == null || clip.frames.Length == 0) return;

            if (view.AnimId != view.LastAnimId)
            {
                view.FrameIndex = 0;
                view.Timer = 0;
            }
            view.Timer += dt;
            bool frameChanged = false;
            while (true)
            {
                float delay = clip.frames[view.FrameIndex].delay / 1000f;
                if (delay <= 0f) delay = 0.05f;
                if (view.Timer < delay) break;
                view.Timer -= delay;
                view.FrameIndex = (view.FrameIndex + 1) % clip.frames.Length;
                frameChanged = true;
            }
            if (!frameChanged && view.AnimId == view.LastAnimId && view.FrameIndex == view.LastFrameIndex) return;

            AnimFrameData frame = clip.frames[view.FrameIndex];
            if (frame.image.path == null || frame.image.path.Length == 0) return;

            LSAnimResComponent res = self.GetParent<Room>().GetComponent<LSAnimResComponent>();
            foreach (RenderLayer layer in view.RenderConfig.Layers)
            {
                if (layer.Renderer == null) continue;
                Sprite sprite = res?.GetSprite(layer.AtlasName, frame.image.index);
                if (sprite == null) continue;
                layer.Renderer.sprite = sprite;
                Vector2 center = res?.GetFrameCenter(layer.AtlasName, frame.image.index) ?? Vector2.zero;
                Transform parentT = layer.Renderer.transform.parent;
                Vector3 chain = parentT != null && parentT != view.Root.transform
                    ? parentT.localPosition : Vector3.zero;
                // 像素对齐（2026-08-27）：帧偏移 (imagePos+center) 里 center 奇宽带 .5px，随换帧翻转 → snap 整像素（成因②）
                float offX = Mathf.Round(frame.imagePos.x + center.x) / 100f;
                float offY = Mathf.Round(frame.imagePos.y + center.y) / 100f;
                layer.Renderer.transform.localPosition = new Vector3(offX - chain.x, -offY - chain.y, 0f);
            }
            view.LastAnimId = view.AnimId;
            view.LastFrameIndex = view.FrameIndex;
        }
    }

    /// <summary>远端角色视图的 Destroy（销 GO）——独立系统类</summary>
    [EntitySystemOf(typeof(TownRemotePlayerView))]
    [FriendOf(typeof(TownRemotePlayerView))]
    public static partial class TownRemotePlayerViewSystem
    {
        [EntitySystem]
        private static void Awake(this TownRemotePlayerView self)
        {
        }

        [EntitySystem]
        private static void Destroy(this TownRemotePlayerView self)
        {
            if (self.Root != null)
            {
                UnityEngine.Object.Destroy(self.Root);
                self.Root = null;
            }
        }
    }
}
