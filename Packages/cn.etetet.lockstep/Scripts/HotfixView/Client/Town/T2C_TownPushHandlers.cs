using TrueSync;
using UnityEngine;

namespace ET.Client
{
    /// <summary>有玩家进城镇（TownScene 广播）：创建远端角色视图。
    /// 放 HotfixView：多人组件是 ModelView 实体（ET.Hotfix 不引用 ET.ModelView）；消息分派扫全程序集不挑家。</summary>
    [MessageHandler(SceneType.LockStep)]
    public class T2C_PlayerEnterTownHandler : MessageHandler<Scene, T2C_PlayerEnterTown>
    {
        protected override async ETTask Run(Scene root, T2C_PlayerEnterTown message)
        {
            TSVector p = message.Position;
            Vector3 display = new((float)p.x, (float)(p.z + p.y), 0f);   // 屏幕映射同款
            root.GetComponent<Room>()?.GetComponent<TownRemotePlayerManagerComponent>()
                ?.CreateRemote(message.PlayerId, display);
            await ETTask.CompletedTask;
        }
    }

    /// <summary>有玩家离开城镇（TownScene 广播）：移除远端角色视图</summary>
    [MessageHandler(SceneType.LockStep)]
    public class T2C_PlayerLeaveTownHandler : MessageHandler<Scene, T2C_PlayerLeaveTown>
    {
        protected override async ETTask Run(Scene root, T2C_PlayerLeaveTown message)
        {
            root.GetComponent<Room>()?.GetComponent<TownRemotePlayerManagerComponent>()
                ?.RemoveRemote(message.PlayerId);
            await ETTask.CompletedTask;
        }
    }

    /// <summary>远端玩家位置包（TownScene 转发）：更新插值目标</summary>
    [MessageHandler(SceneType.LockStep)]
    public class T2C_PositionBroadcastHandler : MessageHandler<Scene, T2C_PositionBroadcast>
    {
        protected override async ETTask Run(Scene root, T2C_PositionBroadcast message)
        {
            TSVector p = message.Position;
            Vector3 display = new((float)p.x, (float)(p.z + p.y), 0f);
            TSVector f = message.Forward;
            Vector3 forward = new((float)f.x, 0f, 0f);
            root.GetComponent<Room>()?.GetComponent<TownRemotePlayerManagerComponent>()
                ?.UpdateRemote(message.PlayerId, display, forward, message.IsMoving);
            await ETTask.CompletedTask;
        }
    }
}
