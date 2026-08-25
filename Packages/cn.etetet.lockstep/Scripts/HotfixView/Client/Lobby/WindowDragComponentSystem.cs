using UnityEngine;

namespace ET.Client
{
    [EntitySystemOf(typeof(WindowDragComponent))]
    [FriendOf(typeof(WindowDragComponent))]
    public static partial class WindowDragComponentSystem
    {
        [EntitySystem]
        private static void Awake(this WindowDragComponent self)
        {
        }

        [EntitySystem]
        private static void Destroy(this WindowDragComponent self)
        {
            self.Dragging = false;
            self.DraggingWindow = null;
        }

        [EntitySystem]
        private static void Update(this WindowDragComponent self)
        {
            if (!self.Dragging || self.DraggingWindow == null) return;

            if (!Input.GetMouseButton(0))   // 松手结束
            {
                self.Dragging = false;
                return;
            }

            // 屏幕像素 → 画布单位换算（ScaleWithScreenSize 下两坐标系不同）
            Canvas canvas = self.DraggingWindow.GetComponentInParent<Canvas>();
            float scale = canvas != null ? canvas.scaleFactor : 1f;
            if (scale <= 0f) scale = 1f;

            Vector2 delta = (Vector2)Input.mousePosition - self.StartMouse;
            self.DraggingWindow.anchoredPosition = self.StartWindowPos + delta / scale;
        }
    }

    [FriendOf(typeof(WindowDragComponent))]
    public static class WindowDragHelper
    {
        /// <summary>开始拖动指定窗口（挂在 root scene 的 WindowDragComponent 上，按需创建）</summary>
        public static void Begin(Scene root, RectTransform window)
        {
            if (root == null || window == null) return;

            WindowDragComponent drag = root.GetComponent<WindowDragComponent>();
            if (drag == null)
                drag = root.AddComponent<WindowDragComponent>();

            drag.DraggingWindow = window;
            drag.StartMouse = Input.mousePosition;
            drag.StartWindowPos = window.anchoredPosition;
            drag.Dragging = true;
        }
    }
}
