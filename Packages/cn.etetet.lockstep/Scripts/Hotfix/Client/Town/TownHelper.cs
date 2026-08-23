using TrueSync;

namespace ET.Client
{
    /// <summary>
    /// 城镇入口（03 文档 §1.1）：RPC（服务器记成员+切 Gate 路由）→ 场景切换。
    /// 登录直进与战斗回城走同一条路（回城 spawnPosition = 记住的城镇位置）。
    /// </summary>
    public static partial class TownHelper
    {
        public static async ETTask EnterTown(Scene root, TSVector spawnPosition)
        {
            T2C_EnterTownConfirm response = await root.GetComponent<ClientSenderComponent>().Call(C2G_EnterTown.Create())
                as T2C_EnterTownConfirm;
            if (response == null || response.Error != ErrorCode.ERR_Success)
            {
                Log.Error($"[Town] 进城镇失败：{(response == null ? "响应为空" : response.Message)}");
                return;
            }

            TownMemory.PendingMembers = response.Members;   // 已有成员（阶段D 远端渲染，进城完成后清）
            await TownSceneChangeHelper.SceneChangeToTown(root, spawnPosition);
        }
    }
}
