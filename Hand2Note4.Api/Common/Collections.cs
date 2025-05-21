using System.Collections.Immutable;

namespace Hand2Note4.Api;
internal static class Collections {

    public static int
    MaxOrDefault(this IEnumerable<int> items) =>
        items.MaxOrDefault(x => x);
    
    public static int
    MaxOrDefault<T>(this IEnumerable<T> items, Func<T, int> selector) {
        var result = int.MinValue;
        var hasItems = false;
        foreach (var item in items) {
            var value = selector(item);
            if (value > result)
                result = value;
            if (!hasItems)
                hasItems = true;
        }

        return hasItems ? result : 0;
    }

    public static IEnumerable<T>
    WithMaxValue<T>(this IEnumerable<T> items, Func<T, IComparable> selector) {
        var max = items.Select(selector).Max();
        return items.Where(x => selector(x).Equals(max));
    }
    
    public static ImmutableList<TOut>
    MapToImmutableList<TIn, TOut>(this IEnumerable<TIn> enumerable, Func<TIn, TOut> convert) =>
        ImmutableList.CreateRange(enumerable.Select(convert));
    
    public static ImmutableList<TOut>
    MapToImmutableList<TOut>(this IEnumerable enumerable, Func<object, TOut> convert) {
        var result = ImmutableList.CreateBuilder<TOut>();
        foreach(var item in enumerable) {
            result.Add(convert(item));
        }
        return result.ToImmutableList();
    }
    
    public static List<TOut>
    MapToList<TIn, TOut>(this IEnumerable<TIn> enumerable, Func<TIn, TOut> convert) =>
        enumerable.Select(convert).ToList();

    public static void 
    ForEach<T>(this IEnumerable<T> items, Action<T> action) {
        foreach(var item in items)
            action(item);
    }
        
    public static IEnumerable<(int index, T value)>
    WithIndex<T>(this IEnumerable<T> items) {
        int index = 0;
        foreach (var item in items) {
            yield return (index, item);
            index++;
        }
    }
    
    public static void
    VerifyDistinct<T>(this IEnumerable<T> items, string? message = null) {
        if (items.Distinct().Count() != items.Count())
            throw new InvalidOperationException(message ?? "Expecting collection with distinct element");
    }
    
    public static bool
    TryGet<T>(this IEnumerable<T> items, Func<T, bool> predicate, out T? result) {
        foreach (var item in items) {
            if (!predicate(item)) continue;
            result = item;
            return true;
        }
        result = default;
        return false;
    }

}
