using System.Globalization;
using System.Text;
using Luban.CSharp.TypeVisitors;
using Luban.Datas;
using Luban.DataVisitors;
using Luban.Defs;
using Luban.Types;
using Luban.Utils;

namespace Luban.CSharp.DataVisitors;

public class ToCsharpCodeLiteralVisitor : IDataFuncVisitor<string>
{
    public static ToCsharpCodeLiteralVisitor Ins { get; } = new();

    public string Accept(DBool type)
    {
        return type.Value ? "true" : "false";
    }

    public string Accept(DByte type)
    {
        return type.Value.ToString(CultureInfo.InvariantCulture);
    }

    public string Accept(DShort type)
    {
        return type.Value.ToString(CultureInfo.InvariantCulture);
    }

    public string Accept(DInt type)
    {
        return type.Value.ToString(CultureInfo.InvariantCulture);
    }

    public string Accept(DLong type)
    {
        return type.Value.ToString(CultureInfo.InvariantCulture);
    }

    public string Accept(DFloat type)
    {
        if (float.IsNaN(type.Value))
        {
            return "float.NaN";
        }
        if (float.IsPositiveInfinity(type.Value))
        {
            return "float.PositiveInfinity";
        }
        if (float.IsNegativeInfinity(type.Value))
        {
            return "float.NegativeInfinity";
        }
        return $"{type.Value.ToString("R", CultureInfo.InvariantCulture)}f";
    }

    public string Accept(DDouble type)
    {
        if (double.IsNaN(type.Value))
        {
            return "double.NaN";
        }
        if (double.IsPositiveInfinity(type.Value))
        {
            return "double.PositiveInfinity";
        }
        if (double.IsNegativeInfinity(type.Value))
        {
            return "double.NegativeInfinity";
        }
        return type.Value.ToString("R", CultureInfo.InvariantCulture);
    }

    public string Accept(DDateTime type)
    {
        return type.UnixTimeOfCurrentContext().ToString(CultureInfo.InvariantCulture);
    }

    public string Accept(DString type)
    {
        return $"@\"{type.Value.Replace("\"", "\"\"")}\"";
    }

    public string Accept(DBean type)
    {
        var sb = new StringBuilder();
        sb.Append("new ").Append(type.ImplType.FullNameWithTopModule).Append('(');

        bool hasPreviousArg = false;
        for (int i = 0; i < type.Fields.Count; i++)
        {
            var defField = (DefField)type.ImplType.HierarchyFields[i];
            if (!defField.NeedExport())
            {
                continue;
            }

            if (hasPreviousArg)
            {
                sb.Append(", ");
            }
            else
            {
                hasPreviousArg = true;
            }

            var fieldData = type.Fields[i];
            sb.Append(fieldData == null ? "default" : fieldData.Apply(this));
        }

        sb.Append(')');
        return sb.ToString();
    }

    private static string GetCollectionTypeName(TType type)
    {
        return type switch
        {
            TArray array => $"{array.ElementType.Apply(DeclaringTypeNameVisitor.Ins)}[]",
            TList list => $"{ConstStrings.ListTypeName}<{list.ElementType.Apply(DeclaringTypeNameVisitor.Ins)}>",
            TSet set => $"{ConstStrings.HashSetTypeName}<{set.ElementType.Apply(DeclaringTypeNameVisitor.Ins)}>",
            TMap map => $"{ConstStrings.HashMapTypeName}<{map.KeyType.Apply(DeclaringTypeNameVisitor.Ins)}, {map.ValueType.Apply(DeclaringTypeNameVisitor.Ins)}>",
            _ => type.Apply(UnderlyingDeclaringTypeNameVisitor.Ins),
        };
    }

    private void AppendCollection(IEnumerable<DType> datas, string typeName, StringBuilder sb)
    {
        sb.Append("new ").Append(typeName).Append(" {");

        bool hasPreviousItem = false;
        foreach (var data in datas)
        {
            if (hasPreviousItem)
            {
                sb.Append(", ");
            }
            else
            {
                sb.Append(' ');
                hasPreviousItem = true;
            }
            sb.Append(data.Apply(this));
        }

        sb.Append(" }");
    }

    public string Accept(DArray type)
    {
        var sb = new StringBuilder();
        AppendCollection(type.Datas, GetCollectionTypeName(type.Type), sb);
        return sb.ToString();
    }

    public string Accept(DList type)
    {
        var sb = new StringBuilder();
        AppendCollection(type.Datas, GetCollectionTypeName(type.Type), sb);
        return sb.ToString();
    }

    public string Accept(DSet type)
    {
        var sb = new StringBuilder();
        AppendCollection(type.Datas, GetCollectionTypeName(type.Type), sb);
        return sb.ToString();
    }

    public string Accept(DMap type)
    {
        var sb = new StringBuilder();
        sb.Append("new ").Append(GetCollectionTypeName(type.Type)).Append(" {");

        bool hasPreviousItem = false;
        foreach (var entry in type.DataMap)
        {
            if (hasPreviousItem)
            {
                sb.Append(", ");
            }
            else
            {
                sb.Append(' ');
                hasPreviousItem = true;
            }
            sb.Append("[ ").Append(entry.Key.Apply(this)).Append(" ] = ").Append(entry.Value.Apply(this));
        }

        sb.Append(" }");
        return sb.ToString();
    }

    public string Accept(DEnum type)
    {
        if (string.IsNullOrEmpty(type.StrValue) || string.Equals(type.StrValue, "0", StringComparison.Ordinal))
        {
            return "default";
        }

        string enumTypeName = type.Type.Apply(UnderlyingDeclaringTypeNameVisitor.Ins);
        string enumSep = type.Type.GetTagOrDefault("sep", "|");
        string[] enumTokens = type.StrValue.Split(enumSep[0]);

        var sb = new StringBuilder();
        for (int i = 0; i < enumTokens.Length; i++)
        {
            string token = enumTokens[i].Trim();
            var enumItem = type.Type.DefEnum.Items.FirstOrDefault(item =>
                item.Name == token || item.Value == token || item.Alias == token);
            if (enumItem == null)
            {
                throw new Exception($"enum item '{token}' not found in {type.Type.DefEnum.FullName}");
            }

            if (i > 0)
            {
                sb.Append(" | ");
            }
            sb.Append(enumTypeName).Append('.').Append(enumItem.Name);
        }
        return sb.ToString();
    }
}
