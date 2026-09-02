using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace ET
{
    /// <summary>
    /// 一次性工具：用 AlibabaPuHuiTi ttf + preload-chars.txt 静态烘焙 TMP SDF 字体资产。
    /// 静态烘焙 = 编辑期把字集全部光栅化进图集，构建/运行时不再动态补字；
    /// preload-chars.txt 即唯一真源字集，运行时超出该字集会缺字（需改字集后重跑本工具）。
    ///
    /// 跑法（本机 Unity CLI 不在 PATH，需全路径）：
    ///   unity -batchmode -quit -projectPath <工程> -executeMethod ET.GenAlibabaSdf.Generate -logFile gen_sdf.log
    /// </summary>
    public static class GenAlibabaSdf
    {
        const string TtfPath   = "Packages/cn.etetet.lockstep/Assets/GameRes/Fonts/AlibabaPuHuiTi-3-55-Regular.ttf";
        const string CharsPath = "Packages/cn.etetet.lockstep/Assets/GameRes/Fonts/preload-chars.txt";
        const string OutPath   = "Packages/cn.etetet.lockstep/Assets/GameRes/Fonts/AlibabaPuHuiTi-3-55-Regular SDF.asset";

        // 采样字号须 ≥ UI 最大字号（此处 48 覆盖常规 UI 及伤害飘字）；padding 保证 SDF 抗锯齿采样
        const int SamplingPointSize = 48;
        const int Padding           = 6;
        const int AtlasWidth        = 2048;
        const int AtlasHeight       = 2048;

        [MenuItem("ET/Gen Alibaba SDF")]
        public static void Generate()
        {
            // 1. 读字集：去掉换行/制表等控制符，保留空格(U+0020)与全角空格，去重
            string raw   = File.ReadAllText(CharsPath);
            string chars = new string(raw.Where(c => !char.IsControl(c)).Distinct().ToArray());

            Debug.Log($"[GenAlibabaSdf] 字集规模={chars.Length}");

            // 2. 加载源字体
            Font font = AssetDatabase.LoadAssetAtPath<Font>(TtfPath);
            if (font == null)
            {
                Debug.LogError("[GenAlibabaSdf] 找不到源字体: " + TtfPath);
                return;
            }

            // 3. 删除旧资产，保证可重复跑
            string old = AssetDatabase.AssetPathToGUID(OutPath);
            if (!string.IsNullOrEmpty(old))
                AssetDatabase.DeleteAsset(OutPath);

            // 4. 先以 Dynamic 建资产（Static 下 TryAddCharacters 会拒绝；烘焙完再切 Static）
            TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(
                font, SamplingPointSize, Padding,
                GlyphRenderMode.SDFAA, AtlasWidth, AtlasHeight,
                AtlasPopulationMode.Dynamic, enableMultiAtlasSupport: true);

            if (fontAsset == null)
            {
                Debug.LogError("[GenAlibabaSdf] CreateFontAsset 失败");
                return;
            }

            // 5. 落盘主资产，并把图集纹理、材质挂为子资产
            AssetDatabase.CreateAsset(fontAsset, OutPath);
            AssetDatabase.AddObjectToAsset(fontAsset.atlasTextures[0], fontAsset);
            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);

            // 6. 烘焙字集（此时 Dynamic，真正光栅化打包进图集）
            bool ok = fontAsset.TryAddCharacters(chars, out string missing);
            if (!string.IsNullOrEmpty(missing))
                Debug.LogWarning("[GenAlibabaSdf] 缺字(" + missing.Length + "): " + missing);

            // 7. 切静态：构建/运行时清空 sourceFontFile，字集固定为已烘焙内容
            fontAsset.atlasPopulationMode = AtlasPopulationMode.Static;

            // 8. 落盘 + 重新导入
            EditorUtility.SetDirty(fontAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(OutPath);
            AssetDatabase.Refresh();

            // 9. 报告
            Debug.Log("[GenAlibabaSdf] 完成: 字集=" + chars.Length
                      + " 烘焙成功=" + (ok ? "是" : "否")
                      + " 缺字=" + (string.IsNullOrEmpty(missing) ? "无" : missing)
                      + " glyph=" + fontAsset.glyphTable.Count
                      + " 图集=" + AtlasWidth + "x" + AtlasHeight
                      + " -> " + OutPath);
        }
    }
}
