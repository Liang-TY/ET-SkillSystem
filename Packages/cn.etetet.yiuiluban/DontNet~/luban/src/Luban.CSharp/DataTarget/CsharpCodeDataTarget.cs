using System.Reflection;
using System.Text;
using Luban.CodeFormat;
using Luban.CodeFormat.CodeStyles;
using Luban.CodeTarget;
using Luban.CSharp.CodeTarget;
using Luban.CSharp.TemplateExtensions;
using Luban.DataTarget;
using Luban.Defs;
using Luban.TemplateExtensions;
using Luban.Tmpl;
using Scriban;
using Scriban.Runtime;
using System.Text.RegularExpressions;

namespace Luban.CSharp.DataTarget;

[DataTarget("cs-code-data")]
public class CsharpCodeDataTarget : DataTargetBase
{
    private static string CodeTargetName => typeof(CsharpCodeCodeTarget).GetCustomAttribute<CodeTargetAttribute>()!.Name;

    protected virtual ICodeStyle CodeStyle => _codeStyle ??= CreateConfigurableCodeStyle();

    private ICodeStyle _codeStyle;

    protected override string DefaultOutputFileExt => "cs";

    private ICodeStyle CreateConfigurableCodeStyle()
    {
        var baseStyle = GenerationContext.Current.GetCodeStyle(CodeTargetName) ?? CodeFormatManager.Ins.CsharpDefaultCodeStyle;

        var env = EnvManager.Current;
        string namingKey = BuiltinOptionNames.NamingConvention;
        return new OverlayCodeStyle(baseStyle,
            env.GetOptionOrDefault($"{namingKey}.{CodeTargetName}", "namespace", true, ""),
            env.GetOptionOrDefault($"{namingKey}.{CodeTargetName}", "type", true, ""),
            env.GetOptionOrDefault($"{namingKey}.{CodeTargetName}", "method", true, ""),
            env.GetOptionOrDefault($"{namingKey}.{CodeTargetName}", "property", true, ""),
            env.GetOptionOrDefault($"{namingKey}.{CodeTargetName}", "field", true, ""),
            env.GetOptionOrDefault($"{namingKey}.{CodeTargetName}", "enumItem", true, "")
        );
    }

    protected TemplateContext CreateTemplateContext(Template template)
    {
        var ctx = new TemplateContext
        {
            LoopLimit = 0,
            NewLine = "\n",
        };
        ctx.PushGlobal(new ContextTemplateExtension());
        ctx.PushGlobal(new TypeTemplateExtension());
        OnCreateTemplateContext(ctx);
        return ctx;
    }

    protected virtual void OnCreateTemplateContext(TemplateContext ctx)
    {
        ctx.PushGlobal(new CsharpCodeTemplateExtension());
    }

    public void GenerateCodeData(DefTable table, List<Record> records, StringBuilder result)
    {
        var template = GetTemplate();
        var tplCtx = CreateTemplateContext(template);
        var extraEnvs = new ScriptObject
        {
            { "__name", table.Name },
            { "__namespace", table.Namespace },
            { "__namespace_with_top_module", table.NamespaceWithTopModule },
            { "__full_name_with_top_module", table.FullNameWithTopModule },
            { "__table", table },
            { "__this", table },
            { "__key_type", table.KeyTType },
            { "__value_type", table.ValueTType },
            { "__code_style", CodeStyle },
            { "__records", records },
        };
        tplCtx.PushGlobal(extraEnvs);
        result.Append(CommonFileHeaders.AUTO_GENERATE_C_LIKE);
        result.Append(template.Render(tplCtx));
    }

    protected virtual Template GetTemplate()
    {
        if (TemplateManager.Ins.TryGetTemplate($"{CodeTargetName}/tabledata", out var template))
        {
            return template;
        }
        throw new Exception("template:tabledata not found");
    }

    public override OutputFile ExportTable(DefTable table, List<Record> records)
    {
        var result = new StringBuilder();
        GenerateCodeData(table, records, result);
        string content = result.ToString();
        string outputFileName = GetOutputFileName(table, content);
        return CreateOutputFile($"{outputFileName}.{OutputFileExt}", content);
    }

    private static string GetOutputFileName(DefTable table, string content)
    {
        Match factoryMatch = Regex.Match(content, @"class\s+([A-Za-z0-9_]+)\s*:\s*IConfigFactory");
        if (factoryMatch.Success)
        {
            return factoryMatch.Groups[1].Value;
        }

        return table.OutputDataFile;
    }
}
