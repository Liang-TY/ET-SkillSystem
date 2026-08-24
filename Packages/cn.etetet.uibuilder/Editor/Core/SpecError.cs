namespace ET.UIBuilder
{
    public enum ESpecSeverity
    {
        Error,
        Warning,
    }

    /// <summary>
    /// 一条校验问题：severity + 定位路径 + 编码 + 消息。
    /// Path 形如 panel.layer / nodes[1].children[0].props.fontSize。
    /// </summary>
    public class SpecError
    {
        public ESpecSeverity Severity;
        public string Code;
        public string Path;
        public string Message;

        public override string ToString()
        {
            string tag = Severity == ESpecSeverity.Error ? "ERROR" : "WARN ";
            return $"[{tag}][{Code}] {Path}: {Message}";
        }
    }
}
