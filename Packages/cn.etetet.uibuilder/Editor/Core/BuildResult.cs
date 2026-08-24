using System.Collections.Generic;

namespace ET.UIBuilder
{
    /// <summary>
    /// 一次构建的结构化结果。S3：spec/构建/代码生成错误；
    /// S5 将扩展编译错误（file/line/message）与预览路径。
    /// </summary>
    public class BuildResult
    {
        public bool Ok;
        public string SpecPath;
        public string PrefabPath;
        public readonly List<string> GeneratedFiles = new List<string>();
        public readonly List<string> Warnings = new List<string>();
        public readonly List<string> Errors = new List<string>();
    }
}
