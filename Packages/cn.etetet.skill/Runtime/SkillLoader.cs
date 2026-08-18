using System;
using System.Collections.Generic;
using System.Reflection;

namespace ET
{
    /// <summary>
    /// 技能逻辑注册表/工厂。SkillContentLoader 在 Assembly.Load 内容 DLL 后调 RegisterAssembly：
    /// 反射扫描 [SkillId] 特性类 → 无状态单例缓存（全单位共享，不进实体不序列化，零运行时分配）。
    /// 静态注册表进程内一致（帧同步安全：两端加载同一份 .bytes）。
    /// </summary>
    public static class SkillLoader
    {
        [StaticField]
        private static readonly Dictionary<int, SkillLogic> logics = new();

        /// <summary>
        /// 注册一个程序集内的全部技能（幂等，重复 Id 后者覆盖）。
        /// 守门员：技能类存在非 const 实例字段 → 拒绝注册——无状态纪律机器强制（回滚安全）。
        /// </summary>
        public static void RegisterAssembly(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                SkillIdAttribute attr = type.GetCustomAttribute<SkillIdAttribute>(false);
                if (attr == null) continue;
                if (!typeof(SkillLogic).IsAssignableFrom(type))
                {
                    Log.Error($"[Skill] {type.FullName} 标了 [SkillId] 但没继承 SkillLogic，跳过");
                    continue;
                }

                FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public
                                                    | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                if (fields.Length > 0)
                {
                    Log.Error($"[Skill] {type.FullName} 有实例字段 {fields[0].Name}——" +
                              $"技能状态必须存 LSCast 实体（回滚安全），拒绝注册");
                    continue;
                }

                logics[attr.Id] = (SkillLogic)Activator.CreateInstance(type);
                Log.Info($"[Skill] 注册技能 {attr.Id}: {type.Name}");
            }
        }

        /// <summary>取技能逻辑单例；未注册返回 null</summary>
        public static SkillLogic Get(int skillId)
        {
            logics.TryGetValue(skillId, out SkillLogic logic);
            return logic;
        }
    }
}
