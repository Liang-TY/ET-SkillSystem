using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace ET
{
    /// <summary>
    /// 生成 AlibabaPuHuiTi 动态 TMP SDF 字体资产（运行时补烘）。
    /// 预烘 preload-chars.txt 常用字，运行时遇到图集外字符从 ttf 动态补进图集（无 641 字集上限）。
    /// 重新生成会保持原 guid，已引用此字体的 prefab / TMP Settings 不脱链。
    ///
    /// 跑法：编辑器菜单 ET > Gen Alibaba SDF；或 batchmode -executeMethod ET.GenAlibabaSdf.Generate
    /// 依赖：ttf 的 includeFontData 必须为 1（否则运行时无字体数据可补）。
    /// </summary>
    public static class GenAlibabaSdf
    {
        const string TtfPath   = "Packages/cn.etetet.lockstep/Assets/GameRes/Fonts/AlibabaPuHuiTi-3-55-Regular.ttf";
        const string CharsPath = "Packages/cn.etetet.lockstep/Assets/GameRes/Fonts/preload-chars.txt";
        const string OutPath   = "Assets/TextMesh Pro/Resources/Fonts & Materials/AlibabaPuHuiTi-3-55-Regular SDF.asset";

        const int SamplingPointSize = 48;
        const int Padding           = 6;
        const int AtlasWidth        = 2048;
        const int AtlasHeight       = 2048;

        [MenuItem("ET/Gen Alibaba SDF")]
        public static void Generate()
        {
            // 1. 读字集：去掉控制符保留空格，去重
            string chars = new string(File.ReadAllText(CharsPath).Where(c => !char.IsControl(c)).Distinct().ToArray());

            // 2. 加载源字体
            Font font = AssetDatabase.LoadAssetAtPath<Font>(TtfPath);
            if (font == null)
            {
                Debug.LogError("[GenAlibabaSdf] 找不到源字体: " + TtfPath);
                return;
            }

            // 3. 记录旧 guid，删旧资产后重建（生成完写回 guid，保持引用不断链）
            string oldGuid = AssetDatabase.AssetPathToGUID(OutPath);
            if (!string.IsNullOrEmpty(oldGuid))
                AssetDatabase.DeleteAsset(OutPath);

            // 4. Dynamic 建资产：sourceFontFile 指向 ttf，运行时遇到图集外字符自动补烘
            TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(
                font, SamplingPointSize, Padding,
                GlyphRenderMode.SDFAA, AtlasWidth, AtlasHeight,
                AtlasPopulationMode.Dynamic, enableMultiAtlasSupport: true);
            if (fontAsset == null)
            {
                Debug.LogError("[GenAlibabaSdf] CreateFontAsset 失败");
                return;
            }

            // 5. 落盘主资产 + 图集纹理/材质子资产
            AssetDatabase.CreateAsset(fontAsset, OutPath);
            AssetDatabase.AddObjectToAsset(fontAsset.atlasTextures[0], fontAsset);
            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);

            // 6. 预烘常用字（减少首帧运行时补烘）
            bool ok = fontAsset.TryAddCharacters(chars, out string missing);
            if (!string.IsNullOrEmpty(missing))
                Debug.LogWarning("[GenAlibabaSdf] 预烘缺字(" + missing.Length + "): " + missing);

            EditorUtility.SetDirty(fontAsset);
            AssetDatabase.SaveAssets();

            // 7. 写回旧 guid
            if (!string.IsNullOrEmpty(oldGuid))
            {
                string metaPath = OutPath + ".meta";
                if (File.Exists(metaPath))
                {
                    string meta = File.ReadAllText(metaPath);
                    meta = Regex.Replace(meta, "guid: [0-9a-f]+", "guid: " + oldGuid);
                    File.WriteAllText(metaPath, meta);
                }
            }

            // 8. 重新导入 + 刷新
            AssetDatabase.ImportAsset(OutPath);
            AssetDatabase.Refresh();

            // 9. 报告
            Debug.Log("[GenAlibabaSdf] 完成: 模式=Dynamic(运行时补烘) 预烘=" + chars.Length
                      + " 成功=" + (ok ? "是" : "否")
                      + " 缺字=" + (string.IsNullOrEmpty(missing) ? "无" : missing)
                      + " glyph=" + fontAsset.glyphTable.Count
                      + " -> " + OutPath);
        }
    }
}
