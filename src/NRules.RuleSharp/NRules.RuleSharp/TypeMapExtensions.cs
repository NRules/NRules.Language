namespace NRules.RuleSharp;

internal static class TypeMapExtensions
{
    public static void AddDefaultAliases(this TypeMap typeMap)
    {
        typeMap.AddAlias("bool", "System.Boolean");
        typeMap.AddAlias("byte", "System.Byte");
        typeMap.AddAlias("sbyte", "System.SByte");
        typeMap.AddAlias("char", "System.Char");
        typeMap.AddAlias("decimal", "System.Decimal");
        typeMap.AddAlias("double", "System.Double");
        typeMap.AddAlias("float", "System.Single");
        typeMap.AddAlias("int", "System.Int32");
        typeMap.AddAlias("uint", "System.UInt32");
        typeMap.AddAlias("long", "System.Int64");
        typeMap.AddAlias("ulong", "System.UInt64");
        typeMap.AddAlias("object", "System.Object");
        typeMap.AddAlias("short", "System.Int16");
        typeMap.AddAlias("ushort", "System.UInt16");
        typeMap.AddAlias("string", "System.String");
    }
}