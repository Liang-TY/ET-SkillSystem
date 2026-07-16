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
            // Prefab
            RegisterHandler("PrefabInstantiate", UBridgePrefabInstantiateHandler.Handle);
            RegisterHandler("PrefabSave", UBridgePrefabSaveHandler.Handle);
            RegisterHandler("PrefabApply", UBridgePrefabApplyHandler.Handle);
            RegisterHandler("PrefabUnpack", UBridgePrefabUnpackHandler.Handle);
            RegisterHandler("PrefabGetInfo", UBridgePrefabGetInfoHandler.Handle);
            RegisterHandler("PrefabGetHierarchy", UBridgePrefabGetHierarchyHandler.Handle);
            // Inspector
            RegisterHandler("InspectorGetComponents", UBridgeInspectorGetComponentsHandler.Handle);
            RegisterHandler("InspectorGetProperties", UBridgeInspectorGetPropertiesHandler.Handle);
            RegisterHandler("InspectorGetProperty", UBridgeInspectorGetPropertyHandler.Handle);
            RegisterHandler("InspectorFindProperty", UBridgeInspectorFindPropertyHandler.Handle);
            RegisterHandler("InspectorSetProperty", UBridgeInspectorSetPropertyHandler.Handle);
            RegisterHandler("InspectorSetProperties", UBridgeInspectorSetPropertiesHandler.Handle);
            RegisterHandler("InspectorAddComponent", UBridgeInspectorAddComponentHandler.Handle);
            RegisterHandler("InspectorRemoveComponent", UBridgeInspectorRemoveComponentHandler.Handle);
            // Editor 控制
            RegisterHandler("Reload", UBridgeReloadHandler.Handle);
            RegisterHandler("EditorUndo", UBridgeEditorUndoHandler.Handle);
            RegisterHandler("EditorRedo", UBridgeEditorRedoHandler.Handle);
            RegisterHandler("EditorPause", UBridgeEditorPauseHandler.Handle);
            RegisterHandler("EditorGetState", UBridgeEditorGetStateHandler.Handle);
            // 延迟命令
            RegisterHandler("Compile", UBridgeCompileHandler.Handle);
            RegisterHandler("Refresh", UBridgeRefreshHandler.Handle);
            RegisterHandler("RegenProject", UBridgeRegenProjectHandler.Handle);
            RegisterHandler("EnterPlay", UBridgeEnterPlayHandler.Handle);
            RegisterHandler("ExitPlay", UBridgeExitPlayHandler.Handle);
            RegisterHandler("HostState", UBridgeQueryHostStateHandler.Handle);
            RegisterHandler("BatchExecute", UBridgeBatchExecuteHandler.Handle);
            RegisterHandler("AssetImport", UBridgeAssetImportHandler.Handle);
            RegisterHandler("AssetRefresh", UBridgeAssetRefreshHandler.Handle);

            s_Initialized = true;
            Debug.Log($"[UBridge] 已启动，监听: {UBridgeFileStore.Root}");
        }

        public static System.Collections.Generic.IReadOnlyDictionary<string, Func<string, string>> GetHandlers() => s_Handlers;
        public static System.Collections.Generic.IEnumerable<string> GetRegisteredCommands() => s_Handlers.Keys;

        public static void RegisterHandler(string command, Func<string, string> handler)
        {
            s_Handlers[command] = handler;
        }

        private static void OnUpdate()
        {
            EnsureInitialized();

            // 优先泵送延迟命令：有 pending 则每帧重入一次 Handler
            if (UBridgeDeferredRuntime.HasPending)
            {
                if (UBridgeDeferredRuntime.IsTimeout()) return;
                string cmd = UBridgeDeferredRuntime.GetPendingCommand();
                string payload = UBridgeDeferredRuntime.GetPendingPayload();
                if (s_Handlers.TryGetValue(cmd, out var h))
                {
                    try
                    {
                        string result = h(payload);
                        UBridgeFileStore.WriteResponse(UBridgeDeferredRuntime.GetPendingRpcId(), result);
                        UBridgeDeferredRuntime.Clear();
                    }
                    catch (DeferredNotReady) { } // 下帧再试
                    catch { UBridgeDeferredRuntime.Clear(); }
                }
                return; // pending 优先，不处理新请求
            }

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
                UBridgeDeferredRuntime.SetRequestRpcId(rpcId);
                Debug.Log($"[UBridge] 执行命令: {envelope.Command} rpcId={rpcId}");
                string payloadJson = handler(envelope.PayloadJson ?? "");
                Debug.Log($"[UBridge] 命令完成: {envelope.Command}");

                responseJson = UBridgeJsonHelper.ToJson(new UBridgeResponseEnvelope
                {
                    RpcId = envelope.RpcId,
                    Error = UBridgeErrorCode.Success,
                    Message = "",
                    PayloadJson = payloadJson
                });
            }
            catch (DeferredStarted)
            {
                Debug.Log($"[UBridge] 延迟命令已启动: rpcId={rpcId}");
                // 延迟命令已启动，立即返回"已接收"响应
                responseJson = UBridgeJsonHelper.ToJson(new UBridgeResponseEnvelope
                {
                    RpcId = rpcId,
                    Error = UBridgeErrorCode.Success,
                    Message = "deferred: command accepted, pending execution",
                    PayloadJson = "{\"_deferred\":true}"
                });
            }
            catch (Exception ex)
            {
                Debug.LogError($"[UBridge] 处理请求异常: {ex.Message}");

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