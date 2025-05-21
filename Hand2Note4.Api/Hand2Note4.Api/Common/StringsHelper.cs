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
}