namespace ET
{
    /// <summary>
    /// 数值 key 常量。定义在 skill 包（框架层），GameContent 项目使用。
    /// lockstep 包也有一份同名文件（仅 ForbidMove/ForbidSkill），值必须一致。
    /// </summary>
    public static class NumericType
    {
        // 每个 final key 是 10 的倍数，子属性 key = final * 10 + 1~5
        public const int Speed = 1000;
        public const int SpeedBase = 10001;
        public const int SpeedAdd = 10002;
        public const int SpeedPct = 10003;
        public const int SpeedFinalAdd = 10004;
        public const int SpeedFinalPct = 10005;

        public const int Hp = 1001;
        public const int HpBase = 10011;

        public const int MaxHp = 1002;
        public const int MaxHpBase = 10021;
        public const int MaxHpAdd = 10022;
        public const int MaxHpPct = 10023;
        public const int MaxHpFinalAdd = 10024;
        public const int MaxHpFinalPct = 10025;

        public const int Attack = 1003;
        public const int AttackBase = 10031;
        public const int AttackAdd = 10032;
        public const int AttackPct = 10033;

        public const int Defense = 1004;
        public const int DefenseBase = 10041;

        // 独立标记（非五层公式，直接用 final key）
        public const int ForbidMove = 1005;     // >0 表示禁止移动
        public const int ForbidSkill = 1006;    // >0 表示禁止施法
    }
}
