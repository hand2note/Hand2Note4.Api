using System.Text;

namespace Hand2Note4.Api;

public static class StringsHelper {
    
    internal static string
    AggregateToString<T>(this IEnumerable<T> items, string? separator = null) {
        var result = new StringBuilder();
        foreach (var item in items) {
            if (result.Length != 0 && separator != null)
                result.Append(separator);
            result.Append(item);
        }
        return result.ToString();
    }
    
    internal static string 
    Quoted(this object @object) => $"\"{@object.ToString()}\"";
    
    public static string 
    Join(this IEnumerable<string> strings, string separator) {
        var result = string.Empty;
        foreach(var item in strings) {
            if (result != string.Empty)
                result += separator;
            result += item;
        }
        return result;
    }
    
    public static Dictionary<string, string> CamelToPascal = new();
    
    public static string 
    CamelToPascalCase(this string @string) {
        if (CamelToPascal.TryGetValue(@string, out var result))
            return result;
        lock(CamelToPascal) {
            return CamelToPascal[@string] = @string.CamelToPascalCaseSlow();
        }
    }

    public static string 
    CamelToPascalCaseSlow(this string @string) => $"{@string[0]}".ToUpper() + @string[1..];
   
    public static Dictionary<string, string> PascalToCamel = new();

    public static string 
    PascalToCamelCase(this string @string) {
        if (PascalToCamel.TryGetValue(@string, out var result))
            return result;
        lock(PascalToCamel) {
            return PascalToCamel[@string] = @string.PascalToCamelCaseSlow();
        }
    }

    public static string 
    PascalToCamelCaseSlow(this string @string) {
        if (char.IsLower(@string[0]))
            return @string;
        return char.ToLower(@string[0]) + @string[1..];
    }

}