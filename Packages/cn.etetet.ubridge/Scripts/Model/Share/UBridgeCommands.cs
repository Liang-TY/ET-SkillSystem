using System;

namespace ET
{
    /// <summary>
    /// 命令处理器接口：每个命令对应一个实现类
    /// </summary>
    public interface IUBridgeHandler
    {
        /// <summary>
        /// 处理请求，返回响应
        /// </summary>
        IResponse Handle(string requestJson);
    }

    /// <summary>
    /// 请求信封：CLI发送时包装命令类型和参数JSON
    /// </summary>
    public class UBridgeRequestEnvelope : Object
    {
        /// <summary>请求唯一ID，响应会原样返回</summary>
        public string RpcId { get; set; }

        /// <summary>命令类型名，如 "ConsoleGetLogs"</summary>
        public string Command { get; set; }

        /// <summary>命令参数 JSON</summary>
        public string PayloadJson { get; set; }

        /// <summary>超时毫秒数</summary>
        public int TimeoutMs { get; set; } = 15000;
    }

    /// <summary>
    /// 响应信封
    /// </summary>
    public class UBridgeResponseEnvelope : Object
    {
        /// <summary>与请求相同的 RpcId</summary>
        public string RpcId { get; set; }

        /// <summary>错误码，0=成功</summary>
        public int Error { get; set; }

        /// <summary>错误描述</summary>
        public string Message { get; set; }

        /// <summary>命令响应 JSON（具体的 proto 消息序列化结果）</summary>
        public string PayloadJson { get; set; }
    }
}