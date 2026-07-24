using UnityEditor;

namespace ET
{
    /// <summary>
    /// MenuItemExecute 命令处理器
    /// 执行 Unity 菜单项，如 "File/Save Project"
    /// </summary>
    public static class UBridgeMenuItemExecuteHandler
    {
        public static string Handle(string payloadJson)
        {
            MenuItemExecuteRequest request = UBridgeJsonHelper.FromJson<MenuItemExecuteRequest>(payloadJson);
            MenuItemExecuteResponse response = MenuItemExecuteResponse.Create();

            string menuPath = request?.MenuPath ?? "";
            response.MenuPath = menuPath;

            if (string.IsNullOrWhiteSpace(menuPath))
            {
                response.Error = UBridgeErrorCode.InvalidCommandLine;
                response.Message = "MenuPath is required";
                return UBridgeJsonHelper.ToJson(response);
            }

            response.Executed = EditorApplication.ExecuteMenuItem(menuPath);
            if (!response.Executed)
            {
                response.Error = UBridgeErrorCode.HandlerFail;
                response.Message = $"menu item not found or failed: {menuPath}";
            }

            return UBridgeJsonHelper.ToJson(response);
        }
    }
}