using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ET.UIBuilder
{
    /// <summary>
    /// 控件工厂：type → 未挂父级的 GameObject。
    /// 三种来源：
    ///   1. 模板 prefab 实例化（text/image/button/tmp/loop_scroll，YIUI/LoopScroll 官方模板，查找目录沿用 ubridge 机制）
    ///   2. Unity DefaultControls（input/toggle/slider/dropdown/scroll_view，配内置皮肤图保证无贴图时代可预览）
    ///   3. 代码直建（node/block）
    /// 新增类型 = SpecSchema 注册属性表 + 此处注册构造函数。
    /// </summary>
    public static class ControlFactory
    {
        private static readonly string[] TemplateDirs =
        {
            "Packages/cn.etetet.yiuiLoopScrollRectAsync/Editor/TemplatePrefabs",
            "Packages/cn.etetet.yiuiframework/Editor/TemplatePrefabs/YIUI",
        };

        private static readonly Dictionary<string, Func<NodeSpec, GameObject>> Creators =
            new Dictionary<string, Func<NodeSpec, GameObject>>
            {
                { "node", _ => CreateNode() },
                { "text", _ => InstantiateTemplate("YIUIText_NoRaycast") },
                { "tmp", _ => InstantiateTemplate("YIUIText (TMP)") },
                { "image", _ => InstantiateTemplate("YIUIImage_NoRaycast") },
                { "button", _ => InstantiateTemplate("YIUIButton") },
                { "input", _ => FixupFonts(DefaultControls.CreateInputField(StdRes())) },
                { "toggle", _ => FixupFonts(DefaultControls.CreateToggle(StdRes())) },
                { "slider", _ => FixupFonts(DefaultControls.CreateSlider(StdRes())) },
                { "dropdown", _ => FixupFonts(DefaultControls.CreateDropdown(StdRes())) },
                { "scroll_view", _ => FixupFonts(DefaultControls.CreateScrollView(StdRes())) },
                { "loop_scroll_v", _ => InstantiateTemplate("LoopScrollVertical") },
                { "loop_scroll_h", _ => InstantiateTemplate("LoopScrollHorizontal") },
                { "prefab", InstantiatePrefab },
                { "block", _ => CreateBlock() },
            };

        public static GameObject Create(NodeSpec node)
        {
            if (!Creators.TryGetValue(node.Type, out Func<NodeSpec, GameObject> creator))
                throw new InvalidOperationException($"未知控件类型 '{node.Type}'（应已被 SpecLoader 拦截）");

            GameObject go = creator(node);
            go.name = node.Name;
            go.layer = LayerMask.NameToLayer("UI");
            return go;
        }

        /// <summary>是否为容器类控件（子节点直接堆叠其下）</summary>
        public static bool IsContainer(string type)
        {
            return type == "node" || type == "block";
        }

        private static GameObject CreateNode()
        {
            var go = new GameObject("node");
            go.AddComponent<RectTransform>();
            go.AddComponent<CanvasRenderer>();
            return go;
        }

        private static GameObject CreateBlock()
        {
            var go = new GameObject("block");
            go.AddComponent<RectTransform>();
            go.AddComponent<CanvasRenderer>();
            go.AddComponent<UIBlock>();
            return go;
        }

        /// <summary>模板实例化（Object.Instantiate 脱离引用：面板子节点是普通对象，不保留模板嵌套）</summary>
        private static GameObject InstantiateTemplate(string templateName)
        {
            foreach (string dir in TemplateDirs)
            {
                GameObject template = AssetDatabase.LoadAssetAtPath<GameObject>($"{dir}/{templateName}.prefab");
                if (template != null)
                    return UnityEngine.Object.Instantiate(template);
            }

            throw new InvalidOperationException(
                $"模板未找到: {templateName}.prefab（搜索: {string.Join("; ", TemplateDirs)}）");
        }

        /// <summary>实例化指定 prefab（PrefabUtility 保留嵌套引用：Common 组件等复用物跟随源更新）</summary>
        private static GameObject InstantiatePrefab(NodeSpec node)
        {
            string path = GetStr(node.Props, "path");
            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (source == null)
                throw new InvalidOperationException($"prefab 类型节点 '{node.Name}' 的 path 无法加载: {path}");

            Object instance = PrefabUtility.InstantiatePrefab(source);
            return instance != null ? (GameObject)instance : UnityEngine.Object.Instantiate(source);
        }

        /// <summary>DefaultControls 标准资源（内置 UI 皮肤，保证无贴图时代控件可见可预览）</summary>
        private static DefaultControls.Resources StdRes()
        {
            return new DefaultControls.Resources
            {
                standard = SkinSprite("UISprite.psd"),
                background = SkinSprite("Background.psd"),
                inputField = SkinSprite("InputFieldBackground.psd"),
                knob = SkinSprite("Knob.psd"),
                checkmark = SkinSprite("Checkmark.psd"),
                dropdown = SkinSprite("DropdownArrow.psd"),
                mask = SkinSprite("UIMask.psd"),
            };
        }

        private static Sprite SkinSprite(string name)
        {
            return AssetDatabase.GetBuiltinExtraResource<Sprite>($"UI/Skin/{name}");
        }

        /// <summary>DefaultControls 创建的 Text 默认无字体（Unity 6 移除 Arial），统一补内置 LegacyRuntime</summary>
        private static GameObject FixupFonts(GameObject go)
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            foreach (Text text in go.GetComponentsInChildren<Text>(true))
            {
                if (text.font == null)
                    text.font = font;
            }

            return go;
        }

        private static string GetStr(System.Collections.Generic.Dictionary<string, object> props, string key)
        {
            return props.TryGetValue(key, out object value) ? value?.ToString() : null;
        }
    }
}
