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
    public static partial class LSBulletViewComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LSBulletViewComponent self)
        {
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
                // 根 GO 锚 = 弹正下方的地面点（DNF 投射物锚语义，imagePos 摆位才与单位同公式）；
                // 碰撞盒中心的 y（HalfExtents.y）只用于逻辑，视觉贴地
                info.Go.transform.position = new Vector3((float)bullet.Position.x, 0f, (float)bullet.Position.z);
                AdvanceFrame(info, res, Time.deltaTime);
            }
        }

        private static void CreateView(LSBulletViewComponent self, LSBullet bullet)
        {
            BulletDefinition def = BulletLoader.Get(bullet.ConfigId);
            if (def == null || def.ViewAnimId == AnimId.None) return;

            GlobalComponent globalComponent = self.Root().GetComponent<GlobalComponent>();
            // 两级结构（同单位 Unit2D）：根 GO 定位，子 GO 挂 SpriteRenderer 做 imagePos 摆位
            // ——renderer 若直接挂在根上，设 localPosition 会覆盖世界定位（弹聚集在原点的 bug）
            GameObject go = new("Bullet");
            go.transform.SetParent(globalComponent.Unit, false);
            GameObject visual = new("Visual");
            visual.transform.SetParent(go.transform, false);
            SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = 10;   // 单位之上

            self.Bullets[bullet.Id] = new BulletViewInfo
            {
                Go = go,
                Renderer = renderer,
                AnimId = def.ViewAnimId,
                FrameIndex = 0,
                Timer = 0,
                FaceRight = bullet.Direction.x >= 0,
            };
        }

        private static void RemoveView(LSBulletViewComponent self, long bulletId)
        {
            if (!self.Bullets.TryGetValue(bulletId, out BulletViewInfo info)) return;
            if (info.Go != null) UnityEngine.Object.Destroy(info.Go);
            self.Bullets.Remove(bulletId);
        }

        /// <summary>渲染时间自推帧：按 clip 的 delay 换帧，播完停末帧（非循环——波扩散后保持）；
        /// 摆位同单位：imagePos+锚点修正，面左镜像（绕弹心）</summary>
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

            Vector2 off = res?.GetFrameOffset(frame.image.path, frame.image.index) ?? Vector2.zero;
            info.Renderer.transform.localPosition =
                new Vector3((frame.imagePos.x + off.x) / 100f, (frame.imagePos.y + off.y) / 100f, 0f);
            info.Renderer.transform.localScale = info.FaceRight
                ? Vector3.one
                : new Vector3(-1, 1, 1);   // 面左：绕弹心镜像
        }
    }
}
