using UnityEngine;

namespace ET.Client
{
    /// <summary>相机边界钳制 + 像素对齐（战斗/城镇共用）。</summary>
    public static class CameraClampHelper
    {
        /// <summary>
        /// 相机跟随钳制：target（角色脚底 + 垂直偏移）→ 钳到地图视觉边界内 → snap 到 1/100 单位。
        /// mapOriginX = 碰撞组件 OriginX（负半宽）；mapOriginZ = OriginZ（正半高）；地图以原点为中心对称。
        /// 地图比视野窄/矮时对应轴居中（不滚动）。
        /// </summary>
        public static Vector3 ClampToMap(Vector3 target, float mapOriginX, float mapOriginZ, Camera cam)
        {
            float halfW = cam.orthographicSize * cam.aspect;   // 横向半视野
            float halfH = cam.orthographicSize;                // 纵向半视野

            // 地图视觉边界（地面贴图以原点为中心对称）
            float mapLeft = mapOriginX, mapRight = -mapOriginX;
            float mapBottom = -mapOriginZ, mapTop = mapOriginZ;

            float camX = (mapRight - mapLeft <= 2f * halfW)
                ? (mapLeft + mapRight) / 2f
                : Mathf.Clamp(target.x, mapLeft + halfW, mapRight - halfW);
            float camY = (mapTop - mapBottom <= 2f * halfH)
                ? (mapBottom + mapTop) / 2f
                : Mathf.Clamp(target.y, mapBottom + halfH, mapTop - halfH);

            // snap 到 1/100 单位（1 个 DNF 像素；任意整数 ppu 下都是整数屏幕像素），保留相机 z
            return new Vector3(
                Mathf.Round(camX * 100f) / 100f,
                Mathf.Round(camY * 100f) / 100f,
                cam.transform.position.z);
        }
    }
}
