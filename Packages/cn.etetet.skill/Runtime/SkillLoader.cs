using System.Reflection;

namespace ET
{
    /// <summary>技能注册表薄壳（泛型 ContentLoader 的封闭——调用方 API 不变）。</summary>
    public static class SkillLoader
    {
        public static void RegisterAssembly(Assembly assembly)
            => ContentLoader<SkillIdAttribute, SkillLogic>.RegisterAssembly(assembly);

        public static SkillLogic Get(int skillId)
            => ContentLoader<SkillIdAttribute, SkillLogic>.Get(skillId);
    }
}
