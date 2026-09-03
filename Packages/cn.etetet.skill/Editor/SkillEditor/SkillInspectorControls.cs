using System;
using System.Linq;
using UnityEngine.UIElements;

namespace ET.Editor
{
    /// <summary>Inspector 列表区块：标题 + 计数 + 添加/删除按钮 + 条目容器（每次刷新重建）。</summary>
    internal sealed class SkillInspectorListBlock
    {
        private readonly VisualElement root;
        private readonly Label countLabel;
        private readonly VisualElement items;

        public VisualElement Root => root;

        public SkillInspectorListBlock(
            string title,
            int count,
            Action addItem,
            Action<int> removeItem,
            Action<int> duplicateItem = null)
        {
            root = new VisualElement();
            root.style.marginTop = 8;
            root.style.borderTopWidth = 1;
            root.style.borderTopColor = new UnityEngine.Color(0.25f, 0.27f, 0.3f);

            VisualElement header = new();
            header.style.flexDirection = FlexDirection.Row;
            header.style.alignItems = Align.Center;
            Label titleLabel = new(title) { style = { unityFontStyleAndWeight = UnityEngine.FontStyle.Bold } };
            countLabel = new Label($"({count})") { style = { marginLeft = 4, color = new UnityEngine.Color(0.6f, 0.65f, 0.7f) } };
            Button addButton = new(addItem) { text = "+" };
            addButton.style.marginLeft = 8;
            header.Add(titleLabel);
            header.Add(countLabel);
            header.Add(addButton);
            root.Add(header);

            items = new VisualElement { style = { paddingLeft = 10 } };
            root.Add(items);
            RemoveItem = removeItem;
            DuplicateItem = duplicateItem;
        }

        public Action<int> RemoveItem { get; }
        public Action<int> DuplicateItem { get; }

        public VisualElement ItemsContainer => items;

        public VisualElement AddItemRow(string header, int index, bool selected, Action select)
        {
            VisualElement row = new();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginTop = 2;
            if (selected) row.style.backgroundColor = new UnityEngine.Color(0.18f, 0.28f, 0.4f);

            Button headerButton = new(select) { text = header, style = { flexGrow = 1, unityTextAlign = TextAnchor.MiddleLeft } };
            row.Add(headerButton);

            if (DuplicateItem != null)
            {
                Button duplicate = new(() => DuplicateItem(index)) { text = "复制" };
                duplicate.style.marginRight = 2;
                row.Add(duplicate);
            }
            Button remove = new(() => RemoveItem(index)) { text = "删" };
            row.Add(remove);

            items.Add(row);
            return row;
        }
    }

    /// <summary>Inspector 控件工厂：每个控件绑定一条命令路径，失焦/isDelayed 提交一次命令。</summary>
    internal static class SkillInspectorFields
    {
        public static IntegerField Int(
            string label,
            int value,
            bool delayed,
            Func<int> getCurrent,
            Action<int> commit,
            bool readOnly = false)
        {
            IntegerField field = new(label) { isDelayed = delayed, value = value, isReadOnly = readOnly };
            field.RegisterValueChangedCallback(_ =>
            {
                if (field.value != getCurrent()) commit(field.value);
            });
            return field;
        }

        public static Toggle Bool(string label, bool value, Func<bool> getCurrent, Action<bool> commit)
        {
            Toggle field = new(label) { value = value };
            field.RegisterValueChangedCallback(_ =>
            {
                if (field.value != getCurrent()) commit(field.value);
            });
            return field;
        }

        public static TextField Text(
            string label,
            string value,
            Func<string> getCurrent,
            Action<string> commit)
        {
            TextField field = new(label) { value = value ?? string.Empty };
            string original = getCurrent();
            field.RegisterCallback<FocusInEvent>(_ => original = getCurrent());
            field.RegisterCallback<FocusOutEvent>(_ =>
            {
                if (!string.Equals(field.value, original, StringComparison.Ordinal)) commit(field.value);
            });
            return field;
        }

        public static EnumField Enum<T>(string label, string value, Action<string> commit) where T : struct, System.Enum
        {
            T parsed = System.Enum.TryParse(value, true, out T result) ? result : System.Enum.GetValues(typeof(T)).Cast<T>().First();
            EnumField field = new(label, parsed);
            field.RegisterValueChangedCallback(_ => commit(field.value.ToString()));
            return field;
        }

        public static Label Hint(string text)
        {
            Label label = new(text);
            label.style.color = new UnityEngine.Color(0.6f, 0.65f, 0.7f);
            return label;
        }
    }
}
