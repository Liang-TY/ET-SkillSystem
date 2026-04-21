using System;
using TrueSync;
using UnityEngine;

namespace ET.Client
{
    [EntitySystemOf(typeof(LSUnitView))]
    [LSEntitySystemOf(typeof(LSUnitView))]
    [FriendOf(typeof(LSUnitView))]
    public static partial class LSUnitViewSystem
    {
        [EntitySystem]
        private static void Awake(this LSUnitView self, GameObject go)
        {
            self.GameObject = go;
            self.Transform = go.transform;

        }

        [LSEntitySystem]
        private static void LSRollback(this LSUnitView self)
        {
            //LSUnit unit = self.GetUnit();
            //self.Transform.position = unit.Position.ToVector();
            //self.Transform.rotation = unit.Rotation.ToQuaternion();
            //self.t = 0;
            //self.totalTime = 0;
        }

        [EntitySystem]
        private static void Update(this LSUnitView self)
        {
            LSUnit unit = self.GetUnit();

            // 3D 逻辑坐标转 2D 屏幕坐标
            Vector3 logicPos = unit.Position.ToVector();
            const float depthRatio = 0.6f; // 纵深转屏幕纵向的比例，越大越有纵深感
            Vector3 screenPos = new Vector3(
                logicPos.x,
                logicPos.z * depthRatio + logicPos.y,
                0
            );

            const float speed = 6f;
            if (screenPos != self.Position)
            {
                float distance = (screenPos - self.Position).magnitude;
                self.totalTime = distance / speed;
                self.t = 0;
                self.Position = screenPos;
            }

            // 朝向：根据逻辑层的 Forward.x 翻转精灵
            TSVector forward = unit.Forward;
            bool shouldFaceRight = forward.x >= FP.Zero;
            if (shouldFaceRight != self.FaceRight)
            {
                self.FaceRight = shouldFaceRight;
                Vector3 scale = self.Transform.localScale;
                scale.x = shouldFaceRight ? 1 : -1;
                self.Transform.localScale = scale;
            }

            // 动画
            LSInput input = unit.GetComponent<LSInputComponent>().LSInput;
            bool isMoving = input.V != TSVector2.zero || input.VY != FP.Zero;
            // TODO: 改成你的 2D 帧动画逻辑
            self.GetComponent<LSAnimatorComponent>().SetFloatValue("Speed", isMoving ? speed : 0);

            // Lerp 插值平滑移动
            self.t += Time.deltaTime;
            float lerpT = self.totalTime > 0 ? Mathf.Min(self.t / self.totalTime, 1f) : 1f;
            self.Transform.position = Vector3.Lerp(self.Transform.position, self.Position, lerpT);

            // 排序：Z 越大越远，sortingOrder 越小越先画（在后面）
            if (self.SpriteRenderer != null)
            {
                self.SpriteRenderer.sortingOrder = -(int)(logicPos.z * 100);
            }
        }

        private static LSUnit GetUnit(this LSUnitView self)
        {
            LSUnit unit = self.Unit;
            if (unit != null)
            {
                return unit;
            }

            self.Unit = (self.IScene as Room).LSWorld.GetComponent<LSUnitComponent>().GetChild<LSUnit>(self.Id);
            return self.Unit;
        }
    }
}