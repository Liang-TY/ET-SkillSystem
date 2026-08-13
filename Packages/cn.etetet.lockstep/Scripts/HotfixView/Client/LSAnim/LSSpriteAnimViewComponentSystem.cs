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
            if (Time.frameCount % 60 == 0)
                Log.Warning($"[LSAnimView] Update 跑了 SR={(self.SpriteRenderer==null?"NULL":"OK")}");
            if (self.SpriteRenderer == null) return;
            LSUnitView view = self.GetParent<LSUnitView>();
            LSUnit unit = view.Unit;
            LSAnimComponent anim = unit?.GetComponent<LSAnimComponent>();
            if (Time.frameCount % 60 == 0)
                Log.Warning($"[LSAnimView] view.Unit={(unit==null?"NULL":"OK")} anim={(anim==null?"NULL":"OK")}");
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
            // 无条件诊断（确认 anim 值；hasDbg 只在编辑器查）
            if (Time.frameCount % 60 == 0)
            {
#if UNITY_EDITOR
                bool hasDbg = view.GameObject.GetComponentInChildren<LSUnitViewDebug>() != null;
#else
                bool hasDbg = false;
#endif
                Log.Warning($"[LSAnimView] go={view.GameObject.name} id={view.GameObject.GetInstanceID()} hasDbg={hasDbg} AnimId={anim.AnimId} FrameIndex={anim.FrameIndex} IsLoop={anim.IsLoop}");
            }

            // 只有帧真的变了才碰渲染器
            if (anim.AnimId == self.LastAnimId && anim.FrameIndex == self.LastFrameIndex) return;

            AnimFrameData frame = anim.GetCurrentFrame();
            LSAnimResComponent res = self.Room()?.GetComponent<LSAnimResComponent>();
            Sprite sprite = res?.GetSprite(frame.image.index);
            if (sprite == null) return;

            self.SpriteRenderer.sprite = sprite;
            // imagePos 是像素（100ppu）→ 除以 100 转 Unity 单位
            self.SpriteRenderer.transform.localPosition =
                new Vector3(frame.imagePos.x / 100f, frame.imagePos.y / 100f, 0f);

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
