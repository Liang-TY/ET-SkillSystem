namespace ET
{
    public static partial class ErrorCode
    {
        public const int ERR_Success = 0;

        // 1-11004 是SocketError请看SocketError定义
        //-----------------------------------
        // 100000-109999是Core层的错误

        // 110000以下的错误请看ErrorCore.cs

        // 这里配置逻辑层的错误码
        // 110000 - 200000是抛异常的错误
        // 200001以上不抛异常

        /// <summary>
        /// 参数异常
        /// </summary>
        public const int ERR_Cast_ArgsError = 200101;

        /// <summary>
        /// 释放者为空
        /// </summary>
        public const int ERR_Cast_CasterIsNull = 200102;

        /// <summary>
        /// 没有目标
        /// </summary>
        public const int ERR_Cast_TargetIsNull = 200103;
    }
}