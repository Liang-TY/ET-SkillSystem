namespace ET
{
    [EntitySystemOf(typeof(LSNumericComponent))]
    public static partial class LSNumericComponentSystem
    {
        [EntitySystem]
        private static void Awake(this LSNumericComponent self)
        {
            self.NumericDic ??= new System.Collections.Generic.Dictionary<int, FP>();
        }
    }
}
