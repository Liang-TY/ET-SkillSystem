using System.Collections.Generic;
using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 投射物视图系统：IUpdate 轮询逻辑子弹集合做差分（视图侧状态，回滚安全——世界换血后按当前子集对齐）。
    /// 帧推进用渲染 deltaTime（表现层非确定性可接受——弹的逻辑帧不驱动任何判定）。
    /// </summary>
    [EntitySystemOf(typeof(LSBulletViewComponent))]
    [FriendOf(typeof(LSBulletViewComponent))]
    [FriendOf(typeof(LSBullet))]   // 视图读 Position/Direction/ConfigId（HotfixView 受 ET0002 管辖）
    [FriendOf(typeof(LSAnimResComponent))]  // 加法混合读 AdditiveMaterial（ET0002）
    public static partial class LSBulletViewComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LSBulletViewComponent self)
        {
        }

        /// <summary>加载 Unit2D 预制体（弹视图复用单位渲染层级——手搓 GO 缺单位层里的摆位补偿，差出常量偏移）</summary>
        public static async ETTask InitAsync(this LSBulletViewComponent self)
        {
            Room room = self.GetParent<Room>();
            ResourcesLoaderComponent resLoader = room.GetComponent<ResourcesLoaderComponent>();
            self.Prefab = await resLoader.LoadAssetAsync<GameObject>(
                "Packages/cn.etetet.lockstep/Bundles/Unit/Unit2D.prefab");
        }

        [EntitySystem]
        private static void Destroy(this LSBulletViewComponent self)
        {
            foreach (var kv in self.Bullets)
            {
                if (kv.Value.Go != null) UnityEngine.Object.Destroy(kv.Value.Go);   // 全限定：避开 ET.Object
            }
            self.Bullets.Clear();
        }

        [EntitySystem]
        private static void Update(this LSBulletViewComponent self)
        {
            Room room = self.GetParent<Room>();
            LSWorld world = room.LSWorld;
            LSBulletComponent bulletComponent = world?.GetComponent<LSBulletComponent>();
            LSAnimResComponent res = room.GetComponent<LSAnimResComponent>();

            // 1) 差分：新弹建 GO；消失销毁（GetChild 查 Children——弹是 LSBulletComponent 的子实体）
            if (bulletComponent != null)
            {
                foreach (var kv in bulletComponent.Children)
                {
                    if (kv.Value is not LSBullet bullet) continue;
                    if (self.Bullets.ContainsKey(bullet.Id)) continue;
                    CreateView(self, bullet);
                }

                List<long> removed = null;
                foreach (var kv in self.Bullets)
                {
                    if (bulletComponent.GetChild<LSBullet>(kv.Key) != null) continue;
                    removed ??= new List<long>();
                    removed.Add(kv.Key);
                }
                if (removed != null)
                {
                    foreach (long id in removed) RemoveView(self, id);
                }
            }
            else if (self.Bullets.Count > 0)
            {
                foreach (long id in new List<long>(self.Bullets.Keys)) RemoveView(self, id);
            }

            // 2) 推进：位置跟随逻辑弹 + 帧自推 + 朝向镜像
            if (bulletComponent == null) return;
            foreach (var kv in self.Bullets)
            {
                LSBullet bullet = bulletComponent.GetChild<LSBullet>(kv.Key);
                if (bullet == null) continue;
                BulletViewInfo info = kv.Value;
                // 屏幕映射与单位同款（z→屏幕Y、深度 0，03 文档 §9 第6轮定稿）：
                // ViewGrounded=贴地弹 Y=z；否则 Y=z+高度 y。碰撞盒中心的 y 只用于逻辑，视觉贴地。
                // ViewOffset：逻辑碰撞中心≠DNF PO 原点时把视觉锚回原点（如连突刺剑气）
                info.Go.transform.position = new Vector3(
                    (float)bullet.Position.x + info.ViewOffset.x,
                    (info.ViewGrounded ? (float)bullet.Position.z : (float)(bullet.Position.z + bullet.Position.y)) + info.ViewOffset.y,
                    0f);
                AdvanceFrame(info, res, Time.deltaTime);
                // .als 叠加子层自推（门控用弹主层帧号）
                LSAnimOverlayUtil.AdvanceOverlays(info.Overlays, info.FrameIndex, res, Time.deltaTime);
            }
        }

        private static void CreateView(LSBulletViewComponent self, LSBullet bullet)
        {
            BulletDefinition def = BulletLoader.Get(bullet.ConfigId);
            if (def == null || def.ViewAnimId == AnimId.None) return;
            if (self.Prefab == null) return;   // 预制体没加载好（InitAsync 前），下帧重试

            GlobalComponent globalComponent = self.Root().GetComponent<GlobalComponent>();
            // 复用 Unit2D 预制体（同单位渲染层级与摆位补偿），只换 sprite
            GameObject go = UnityEngine.Object.Instantiate(self.Prefab, globalComponent.Unit, true);
            go.name = "Bullet";
            // 朝向翻根 GO（单位同款口径）：所有层图像+摆位一体镜像——只翻 renderer 会让
            // imagePos 摆位不跟随（连突刺剑气面左仍画右边的坑）
            go.transform.localScale = bullet.Direction.x >= 0 ? Vector3.one : new Vector3(-1f, 1f, 1f);
            SpriteRenderer renderer = go.GetComponentInChildren<SpriteRenderer>();
            renderer.sortingOrder = 10;   // 单位之上

            self.Bullets[bullet.Id] = new BulletViewInfo
            {
                Go = go,
                Renderer = renderer,
                OriginalMaterial = renderer.sharedMaterial,   // 缓存原始材质（加法混合帧切走后要切回来）
                AnimId = def.ViewAnimId,
                FrameIndex = 0,
                Timer = 0,
                FaceRight = bullet.Direction.x >= 0,
                ViewGrounded = def.ViewGrounded,
                // 补偿按面右语义配置，面左镜像 x（弹心为轴同款口径）
                ViewOffset = new Vector2(
                    (float)def.ViewOffset.x * (bullet.Direction.x >= 0 ? 1f : -1f),
                    (float)def.ViewOffset.y),
                // .als 叠加子层（弹主层 sortingOrder=10，子层 base=11 绕主层排）
                Overlays = LSAnimOverlayUtil.CreateOverlays(go.transform, def.ViewAnimId, 11),
            };
        }

        private static void RemoveView(LSBulletViewComponent self, long bulletId)
        {
            if (!self.Bullets.TryGetValue(bulletId, out BulletViewInfo info)) return;
            LSAnimOverlayUtil.DestroyOverlays(info.Overlays);
            if (info.Go != null) UnityEngine.Object.Destroy(info.Go);
            self.Bullets.Remove(bulletId);
        }

        /// <summary>渲染时间自推帧：按 clip 的 delay 换帧，播完停末帧（非循环——波扩散后保持）；
        /// 摆位同单位绝对摆位（chain 用 localPosition——不受根 GO 翻转影响；像素对齐同单位）。
        /// 朝向镜像在 CreateView 翻根 GO（图像+摆位一体），此处不重复处理。</summary>
        private static void AdvanceFrame(BulletViewInfo info, LSAnimResComponent res, float dt)
        {
            AnimClipData clip = AnimConfigRegistry.Get(info.AnimId);
            if (clip?.frames == null || clip.frames.Length == 0) return;

            info.Timer += dt;
            while (info.FrameIndex < clip.frames.Length - 1)
            {
                float delay = clip.frames[info.FrameIndex].delay / 1000f;
                if (delay <= 0) delay = 0.05f;
                if (info.Timer < delay) break;
                info.Timer -= delay;
                info.FrameIndex++;
            }

            AnimFrameData frame = clip.frames[info.FrameIndex];
            Sprite sprite = res?.GetSprite(frame.image.path, frame.image.index);
            info.Renderer.sprite = sprite;   // 空路径帧 sprite=null（隐形占位）

            // §2.1 绝对摆位（与单位 LSSpriteAnimView 同款）：local = 内容中心 − 中间层链（localPosition）
            Vector2 center = res?.GetFrameCenter(frame.image.path, frame.image.index) ?? Vector2.zero;
            Transform parentT = info.Renderer.transform.parent;
            Vector3 chain = parentT != null && parentT != info.Go.transform
                ? parentT.localPosition
                : Vector3.zero;
            // 像素对齐（同单位）：center 奇宽带 .5px，随换帧翻转 → snap 整像素
            float offX = Mathf.Round(frame.imagePos.x + center.x) / 100f;
            float offY = Mathf.Round(frame.imagePos.y + center.y) / 100f;
            info.Renderer.transform.localPosition = new Vector3(offX - chain.x, -offY - chain.y, 0f);

            // 帧级效果：LINEARDODGE 加法混合 + RGBA 染色（LSAnimOverlayUtil 共享实现）
            LSAnimOverlayUtil.ApplyFrameEffects(info.Renderer, info.OriginalMaterial, frame, res);
        }
    }
}
