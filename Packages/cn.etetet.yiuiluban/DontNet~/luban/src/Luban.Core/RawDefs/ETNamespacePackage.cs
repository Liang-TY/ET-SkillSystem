namespace Luban;

public static class ETNamespacePackage
{
    private const string m_Prefix = "cn.etetet.";

    public static (string namespaceValue, string package) Get(string value)
    {
        var namespaceValue = value;
        var package        = "";
        if (value.StartsWith(m_Prefix, StringComparison.Ordinal))
        {
            var split = value.Split(".");
            if (split.Length == 3)
            {
                package        = split[2];
                namespaceValue = value.Replace($"{m_Prefix}{package}", "");
            }
            else if (split.Length >= 4)
            {
                package        = split[2];
                namespaceValue = value.Replace($"{m_Prefix}{package}.", "");
            }
        }

        return (namespaceValue, package);
    }
}
