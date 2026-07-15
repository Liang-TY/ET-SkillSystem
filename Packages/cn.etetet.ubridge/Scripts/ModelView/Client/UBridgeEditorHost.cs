using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ET
{
    /// <summary>
    /// UnityBridge Editor 主机：轮询 + 分发
    /// 通过 [InitializeOnLoad] 自动启动，每200ms检查请求目录
    /// </summary>
    [InitializeOnLoad]
    public static class UBridgeEditorHost
    {
#if !UBRIDGE_CLI
        [StaticField]
#endif
        private static readonly Dictionary<string, Func<string, string>> s_Handlers = new();
#if !UBRIDGE_CLI
        [StaticField]
#endif
        private static double s_LastPollTime;
        [StaticField]
        private static bool s_Initialized;

        static UBridgeEditorHost()
        {
            EditorApplication.update += OnUpdate;
        }

        private static void EnsureInitialized()
        {
            if (s_Initialized) return;

            // 初始化文件存储（不需要 MongoRegister，直接可用）
            string root = UBridgePathHelper.ResolveRoot();
            UBridgeFileStore.Initialize(root);

            // 注册命令处理器
            RegisterHandler("ConsoleGetLogs", UBridgeConsoleGetLogsHandler.Handle);
            RegisterHandler("ScreenshotCapture", UBridgeScreenshotCaptureHandler.Handle);
            RegisterHandler("Ping", UBridgePingHandler.Handle);
            RegisterHandler("MenuItemExecute", UBridgeMenuItemExecuteHandler.Handle);
            // Scene
            RegisterHandler("SceneGetHierarchy", UBridgeSceneGetHierarchyHandler.Handle);
            RegisterHandler("SceneGetActive", UBridgeSceneGetActiveHandler.Handle);
            RegisterHandler("SceneLoad", UBridgeSceneLoadHandler.Handle);
            RegisterHandler("SceneSave", UBridgeSceneSaveHandler.Handle);
            RegisterHandler("SceneNew", UBridgeSceneNewHandler.Handle);
            // Selection
            RegisterHandler("SelectionGet", UBridgeSelectionGetHandler.Handle);
            RegisterHandler("SelectionSet", UBridgeSelectionSetHandler.Handle);
            RegisterHandler("SelectionAdd", UBridgeSelectionAddHandler.Handle);
            RegisterHandler("SelectionRemove", UBridgeSelectionRemoveHandler.Handle);
            RegisterHandler("SelectionClear", UBridgeSelectionClearHandler.Handle);
            // Asset
            RegisterHandler("AssetSearch", UBridgeAssetSearchHandler.Handle);
            RegisterHandler("AssetFind", UBridgeAssetFindHandler.Handle);
            RegisterHandler("AssetGetPath", UBridgeAssetGetPathHandler.Handle);
            RegisterHandler("AssetLoad", UBridgeAssetLoadHandler.Handle);
            RegisterHandler("AssetReadText", UBridgeAssetReadTextHandler.Handle);
            // GameObject
            RegisterHandler("GameObjectCreate", UBridgeGameObjectCreateHandler.Handle);
            RegisterHandler("GameObjectDestroy", UBridgeGameObjectDestroyHandler.Handle);
            RegisterHandler("GameObjectFind", UBridgeGameObjectFindHandler.Handle);
            RegisterHandler("GameObjectGetInfo", UBridgeGameObjectGetInfoHandler.Handle);
            RegisterHandler("GameObjectRename", UBridgeGameObjectRenameHandler.Handle);
            RegisterHandler("GameObjectDuplicate", UBridgeGameObjectDuplicateHandler.Handle);
            RegisterHandler("GameObjectSetActive", UBridgeGameObjectSetActiveHandler.Handle);
            // Transform
            RegisterHandler("TransformGet", UBridgeTransformGetHandler.Handle);
            RegisterHandler("TransformSetPosition", UBridgeTransformSetPositionHandler.Handle);
            RegisterHandler("TransformSetRotation", UBridgeTransformSetRotationHandler.Handle);
            RegisterHandler("TransformSetScale", UBridgeTransformSetScaleHandler.Handle);
            RegisterHandler("TransformSetParent", UBridgeTransformSetParentHandler.Handle);
            RegisterHandler("TransformSetSiblingIndex", UBridgeTransformSetSiblingIndexHandler.Handle);
            RegisterHandler("TransformLookAt", UBridgeTransformLookAtHandler.Handle);
            RegisterHandler("TransformReset", UBridgeTransformResetHandler.Handle);

            s_Initialized = true;
            Debug.Log($"[UBridge] 已启动，监听: {UBridgeFileStore.Root}");
        }

        public static void RegisterHandler(string command, Func<string, string> handler)
        {
            s_Handlers[command] = handler;
        }

        private static void OnUpdate()
        {
            EnsureInitialized();

            // 限频：200ms
            double now = EditorApplication.timeSinceStartup;
            if (now - s_LastPollTime < 0.2) return;
            s_LastPollTime = now;

            var (rpcId, content) = UBridgeFileStore.TryTakeNextRequest();
            if (content == null) return;

            string responseJson;
            try
            {
                // 解析请求信封
                UBridgeRequestEnvelope envelope = UBridgeJsonHelper.FromJson<UBridgeRequestEnvelope>(content);
                if (envelope == null || string.IsNullOrEmpty(envelope.Command))
                {
                    responseJson = UBridgeJsonHelper.ToJson(new UBridgeResponseEnvelope
                    {
                        RpcId = envelope?.RpcId ?? "",
                        Error = UBridgeErrorCode.InvalidCommandLine,
                        Message = "请求信封无效：缺少 Command 字段"
                    });
                    UBridgeFileStore.WriteResponse(rpcId, responseJson);
                    return;
                }

                // 查找 Handler
                if (!s_Handlers.TryGetValue(envelope.Command, out var handler))
                {
                    responseJson = UBridgeJsonHelper.ToJson(new UBridgeResponseEnvelope
                    {
                        RpcId = envelope.RpcId,
                        Error = UBridgeErrorCode.CommandNotFound,
                        Message = $"未知命令: {envelope.Command}"
                    });
                    UBridgeFileStore.WriteResponse(rpcId, responseJson);
                    return;
                }

                // 执行 Handler
                string payloadJson = handler(envelope.PayloadJson ?? "");

                responseJson = UBridgeJsonHelper.ToJson(new UBridgeResponseEnvelope
                {
                    RpcId = envelope.RpcId,
                    Error = UBridgeErrorCode.Success,
                    Message = "",
                    PayloadJson = payloadJson
                });
            }
            catch (Exception ex)
            {
                responseJson = UBridgeJsonHelper.ToJson(new UBridgeResponseEnvelope
                {
                    RpcId = rpcId,
                    Error = UBridgeErrorCode.HandlerError,
                    Message = ex.ToString()
                });
            }

            UBridgeFileStore.WriteResponse(rpcId, responseJson);
        }
    }
}