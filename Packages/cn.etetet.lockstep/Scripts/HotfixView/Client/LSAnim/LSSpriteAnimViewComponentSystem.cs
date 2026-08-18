using UnityEngine;

namespace ET.Client
{
    [EntitySystemOf(typeof(LSSpriteAnimViewComponent))]
    [LSEntitySystemOf(typeof(LSSpriteAnimViewComponent))]
    [FriendOf(typeof(LSSpriteAnimViewComponent))]
    [FriendOf(typeof(LSUnitView))]
    [FriendOf(typeof(LSAnimComponent))]
    public static partial class LSSpriteAnimViewComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LSSpriteAnimViewComponent self)
        {
            self.SpriteRenderer = self.GetParent<LSUnitView>().SpriteRenderer;
        }

        [EntitySystem]
        private static void Update(this LSSpriteAnimViewComponent self)
        {
            if (self.SpriteRenderer == null) return;
            LSUnitView view = self.GetParent<LSUnitView>();
            LSUnit unit = view.Unit;
            LSAnimComponent anim = unit?.GetComponent<LSAnimComponent>();
            if (anim == null) return;

#if UNITY_EDITOR
            // 调试镜像：每帧把逻辑状态推到 LSUnitViewDebug（编辑器运行时 Inspector 可见）
            LSUnitViewDebug dbg = view.GameObject.GetComponentInChildren<LSUnitViewDebug>();
            if (dbg != null)
            {
                dbg.AnimId = anim.AnimId;
                dbg.FrameIndex = anim.FrameIndex;
                dbg.FrameTick = (float)anim.FrameTick;
                dbg.Speed = (float)anim.Speed;
                dbg.IsLoop = anim.IsLoop;
                dbg.IsFinished = anim.IsFinished;
                dbg.FaceRight = view.FaceRight;
                dbg.SpriteName = self.SpriteRenderer.sprite ? self.SpriteRenderer.sprite.name : "null";
            }
#endif

            // 只有帧真的变了才碰渲染器
            if (anim.AnimId == self.LastAnimId && anim.FrameIndex == self.LastFrameIndex) return;

            AnimFrameData frame = anim.GetCurrentFrame();
            LSAnimResComponent res = self.Room()?.GetComponent<LSAnimResComponent>();
            Sprite sprite = res?.GetSprite(frame.image.path, frame.image.index);
            if (sprite == null) return;

            self.SpriteRenderer.sprite = sprite;
            // §2.1 绝对摆位公式：renderer local = 内容真实中心 − prefab 中间层偏移（运行时自标定）
            //   真实中心(相对锚点) = ((imagePos.x+X+宽/2)/100, -(imagePos.y+Y+高/2)/100)——DNF y 下正要翻转
            //   中间层偏移 = renderer.parent 世界位 − 根 GO 世界位（prefab 里烤的补偿常数，直读直用不硬编码）
            Vector2 center = res?.GetFrameCenter(frame.image.path, frame.image.index) ?? Vector2.zero;
            Transform parentT = self.SpriteRenderer.transform.parent;
            Vector3 chain = parentT != null ? parentT.position - view.GameObject.transform.position : Vector3.zero;
            self.SpriteRenderer.transform.localPosition = new Vector3(
                (frame.imagePos.x + center.x) / 100f - chain.x,
                -(frame.imagePos.y + center.y) / 100f - chain.y,
                0f);

            self.LastAnimId = anim.AnimId;
            self.LastFrameIndex = anim.FrameIndex;
        }

        [LSEntitySystem]
        private static void LSRollback(this LSSpriteAnimViewComponent self)
        {
            // 逻辑快照已恢复 AnimId/FrameIndex；重置缓存强制下次 Update 重新同步
            self.LastAnimId = -1;
            self.LastFrameIndex = -1;
        }
    }
}
