namespace ET
{
    /// <summary>
    /// lockstep 包的 NumericType 常量（仅移动/施法禁止，供 LSInputComponentSystem 用）。
    /// ⚠️ 值必须与 cn.etetet.skill 包的 NumericType.cs 保持一致。
    /// lockstep 包不引用 skill 包（skill 引用 lockstep），所以各持一份。
    /// </summary>
    public static class NumericType
    {
        public const int ForbidMove = 1005;     // >0 表示禁止移动
        public const int ForbidSkill = 1006;    // >0 表示禁止施法
    }
}
