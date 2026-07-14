using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEditor;

namespace ET
{
    /// <summary>
    /// ScreenshotCapture 命令处理器
    /// 截取 Game View 画面，保存到 Temp/UnityBridge/Screenshots/
    /// </summary>
    public static class UBridgeScreenshotCaptureHandler
    {
        public static string Handle(string payloadJson)
        {
            ScreenshotCaptureRequest request = UBridgeJsonHelper.FromJson<ScreenshotCaptureRequest>(payloadJson);

            // 检查 PlayMode
            if (!EditorApplication.isPlaying)
            {
                bool allowEditMode = request?.AllowEditMode ?? false;
                if (!allowEditMode)
                {
                    ScreenshotCaptureResponse errorResponse = ScreenshotCaptureResponse.Create();
                    errorResponse.Error = UBridgeErrorCode.NotInPlayMode;
                    errorResponse.Message = "不在 PlayMode 中。设置 AllowEditMode=true 允许 EditMode 截图";
                    errorResponse.Captured = false;
                    return UBridgeJsonHelper.ToJson(errorResponse);
                }
            }

            string format = (request?.Format ?? "png").ToLowerInvariant();
            if (format != "png" && format != "jpg" && format != "jpeg")
                format = "png";

            int quality = Math.Clamp(request?.Quality ?? 85, 1, 100);

            Texture2D texture = null;
            try
            {
                texture = CaptureGameView();
                if (texture == null)
                {
                    ScreenshotCaptureResponse errorResponse = ScreenshotCaptureResponse.Create();
                    errorResponse.Error = UBridgeErrorCode.HandlerFail;
                    errorResponse.Message = "截图失败：无法获取 Game View 纹理";
                    errorResponse.Captured = false;
                    return UBridgeJsonHelper.ToJson(errorResponse);
                }

                // 翻转（如果平台需要）
                if (SystemInfo.graphicsUVStartsAtTop)
                {
                    Texture2D flipped = FlipTexture(texture);
                    UnityEngine.Object.DestroyImmediate(texture);
                    texture = flipped;
                }

                // 编码
                byte[] bytes;
                string ext, mediaType;
                if (format == "png")
                {
                    bytes = texture.EncodeToPNG();
                    ext = "png";
                    mediaType = "image/png";
                }
                else
                {
                    bytes = texture.EncodeToJPG(quality);
                    ext = "jpg";
                    mediaType = "image/jpeg";
                }

                // 保存
                string dir = Path.Combine(Application.dataPath, "../Temp/UnityBridge/Screenshots");
                Directory.CreateDirectory(dir);
                string fileName = $"game_{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.{ext}";
                string fullPath = Path.Combine(dir, fileName);
                File.WriteAllBytes(fullPath, bytes);

                BridgeScreenshotInfo info = BridgeScreenshotInfo.Create();
                info.Path = fullPath;
                info.FileName = fileName;
                info.Width = texture.width;
                info.Height = texture.height;
                info.FileSize = bytes.Length;
                info.MediaType = mediaType;

                ScreenshotCaptureResponse response = ScreenshotCaptureResponse.Create();
                response.Captured = true;
                response.Target = "game";
                response.Screenshot = info;

                return UBridgeJsonHelper.ToJson(response);
            }
            finally
            {
                if (texture != null)
                    UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        private static Texture2D CaptureGameView()
        {
            // 方式1：反射获取 GameView 的 RenderTexture
            Type gameViewType = Type.GetType("UnityEditor.GameView, UnityEditor");
            if (gameViewType != null)
            {
                EditorWindow[] windows = (EditorWindow[])Resources.FindObjectsOfTypeAll(gameViewType);
                if (windows.Length > 0)
                {
                    FieldInfo rtField = gameViewType.GetField("m_RenderTexture",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                    if (rtField != null)
                    {
                        RenderTexture rt = rtField.GetValue(windows[0]) as RenderTexture;
                        if (rt != null)
                        {
                            RenderTexture backup = RenderTexture.active;
                            RenderTexture.active = rt;

                            Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
                            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                            tex.Apply();

                            RenderTexture.active = backup;
                            return tex;
                        }
                    }
                }
            }

            // 方式2：降级使用 ScreenCapture API
            return ScreenCapture.CaptureScreenshotAsTexture();
        }

        private static Texture2D FlipTexture(Texture2D source)
        {
            int width = source.width;
            int height = source.height;

            RenderTexture temp = RenderTexture.GetTemporary(width, height, 0, source.graphicsFormat);
            Graphics.Blit(source, temp, new Vector2(1, -1), new Vector2(0, 1));

            RenderTexture backup = RenderTexture.active;
            RenderTexture.active = temp;

            Texture2D flipped = new Texture2D(width, height, TextureFormat.RGBA32, false);
            flipped.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            flipped.Apply();

            RenderTexture.active = backup;
            RenderTexture.ReleaseTemporary(temp);
            return flipped;
        }
    }
}