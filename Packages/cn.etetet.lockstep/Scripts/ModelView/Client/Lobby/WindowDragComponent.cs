using UnityEngine;

namespace ET.Client
{
    /// <summary>
    /// 弹窗拖动（用户决策：设置/角色信息/活动/背包可拖，商城与主界面不可拖）。
    /// 用法：窗口容器的 ClickDown 事件调 WindowDragHelper.Begin；本组件 Update 逐帧移动，松手结束。
    /// （框架 UIEventBindDrag 为无参同步事件拿不到 delta，故用 Input 轮询方案。）
    /// </summary>
    [ComponentOf(typeof(Scene))]
    public class WindowDragComponent: Entity, IAwake, IUpdate, IDestroy
    {
        public RectTransform DraggingWindow;
        public Vector2 StartMouse;
        public Vector2 StartWindowPos;
        public bool Dragging;
    }
}
