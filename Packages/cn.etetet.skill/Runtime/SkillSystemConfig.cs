namespace ET
{
    /// <summary>
    /// 技能系统配置（全局开关/参数——视图表现/调试/系统级行为）。
    /// 值由 SkillContentLoader 从 Bundles/AnimRes/skillconfig.json 加载（改配置零编译：改 json → YooAsset 重收集 → Play）。
    /// 视图/框架经 ET.Skill 编译期引用读取；内容 DLL 也可读（回滚安全——静态值不进快照，两端口径一致由"两端加载同一份 json"保证）。
    /// 以后配置项多了 → luban 表化（已记档的 luban 专题收编）。
    /// </summary>
    public static class SkillSystemConfig
    {
        /// <summary>受击闪白（受击硬直进入瞬间 sprite 白色高亮闪 150ms）</summary>
        [StaticField]
        public static bool HitFlashEnabled;

        /// <summary>屏震（命中时触发——顿帧类表现，暂不实现逻辑，开关先行）</summary>
        [StaticField]
        public static bool ScreenShakeEnabled;

        /// <summary>调试绘制判定框（阶段2 计划的 Gizmo——暂不实现，开关先行）</summary>
        [StaticField]
        public static bool DebugDrawHitbox;
    }
}
