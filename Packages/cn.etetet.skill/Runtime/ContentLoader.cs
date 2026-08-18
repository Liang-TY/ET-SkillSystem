using System;
using System.Collections.Generic;
using System.Reflection;

namespace ET
{
    /// <summary>内容 ID 特性接口——[SkillId]/[BuffId]/[ActionId] 实现，ContentLoader 泛型扫描用。</summary>
    public interface IContentIdAttribute
    {
        int Id { get; }
    }

    /// <summary>
    /// 内容注册器泛型（技能/Buff/Action 同构复用，以后 equipment/monster 内容同样接）：
    /// 反射扫描 TAttr 特性类 → 无状态单例缓存 + 守门员（非 const 实例字段拒绝注册——回滚安全机器强制）。
    /// 静态存储按封闭类型隔离（各内容类型各一份字典）。进程内两端加载同一份 .bytes，帧同步安全。
    /// </summary>
    public static class ContentLoader<TAttr, TBase>
        where TAttr : Attribute, IContentIdAttribute
        where TBase : class
    {
        [StaticField]
        private static readonly Dictionary<int, TBase> items = new();

        public static void RegisterAssembly(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                TAttr attr = type.GetCustomAttribute<TAttr>(false);
                if (attr == null) continue;
                if (!typeof(TBase).IsAssignableFrom(type))
                {
                    Log.Error($"[Content] {type.FullName} 标了 [{typeof(TAttr).Name}] 但没继承 {typeof(TBase).Name}，跳过");
                    continue;
                }

                // 守门员：内容类必须无实例字段（static readonly 允许——只读配置数组放这）
                FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public
                                                    | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (fields.Length > 0)
                {
                    Log.Error($"[Content] {type.FullName} 有实例字段 {fields[0].Name}——" +
                              $"状态必须存实体（回滚安全），拒绝注册");
                    continue;
                }

                items[attr.Id] = (TBase)Activator.CreateInstance(type);
                Log.Info($"[Content] 注册 {typeof(TBase).Name} {attr.Id}: {type.Name}");
            }
        }

        public static TBase Get(int id)
        {
            items.TryGetValue(id, out TBase item);
            return item;
        }
    }
}
