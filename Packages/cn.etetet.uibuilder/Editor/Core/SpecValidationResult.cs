using System.Collections.Generic;
using System.Text;

namespace ET.UIBuilder
{
    /// <summary>
    /// 校验结果：收集全部问题后一次性返回（不遇错即弃），供 AI 一轮修完。
    /// </summary>
    public class SpecValidationResult
    {
        public string SpecPath;
        public readonly List<SpecError> Issues = new List<SpecError>();

        public int ErrorCount { get; private set; }
        public int WarningCount { get; private set; }
        public bool Ok => ErrorCount == 0;

        public void Add(ESpecSeverity severity, string code, string path, string message)
        {
            Issues.Add(new SpecError { Severity = severity, Code = code, Path = path, Message = message });
            if (severity == ESpecSeverity.Error)
                ErrorCount++;
            else
                WarningCount++;
        }

        public void Error(string code, string path, string message)
        {
            Add(ESpecSeverity.Error, code, path, message);
        }

        public void Warn(string code, string path, string message)
        {
            Add(ESpecSeverity.Warning, code, path, message);
        }

        /// <summary>多行汇总文本（console / BuildResult 用）</summary>
        public string Format()
        {
            var sb = new StringBuilder();
            sb.Append("spec: ").Append(SpecPath)
              .Append(" => ").Append(Ok ? "OK" : "INVALID")
              .Append($" ({ErrorCount} errors, {WarningCount} warnings)");
            sb.AppendLine();
            foreach (SpecError issue in Issues)
            {
                sb.Append("  ").AppendLine(issue.ToString());
            }

            return sb.ToString();
        }
    }
}
