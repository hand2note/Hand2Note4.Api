using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Hand2Note4.Api;

#if DEBUG || WASM || MACOS
// Ensure Items10 serialization produces identical bytes in both DEBUG and RELEASE modes.
// This allows debugging of databases created in either mode. Otherwise, we would need two database for DEBUG and RELEASE mode.
[BinarySerializable, BinarySerializeAsCollection]
public struct Items10<T> {
    public T Item0 {get;private set;}
    public T Item1 {get;private set;}
    public T Item2 {get;private set;}
    public T Item3 {get;private set;}
    public T Item4 {get;private set;}
    public T Item5 {get;private set;}
    public T Item6 {get;private set;}
    public T Item7 {get;private set;}
    public T Item8 {get;private set;}
    public T Item9 {get;private set;}
    public Items10(
        T item0 = default,
        T item1 = default,
        T item2 = default,
        T item3 = default,
        T item4 = default, 
        T item5 = default,
        T item6 = default,
        T item7 = default,
        T item8 = default,
        T item9 = default) {

        Item0 = item0;
        Item1 = item1;
        Item2 = item2;
        Item3 = item3;
        Item4 = item4;
        Item5 = item5;
        Item6 = item6;
        Item7 = item7;
        Item8 = item8;
        Item9 = item9;
    }

    public T this[int index] {
        set {
            switch(index) {
                case 0: Item0 = value; break;
                case 1: Item1 = value; break;
                case 2: Item2 = value; break;
                case 3: Item3 = value; break;
                case 4: Item4 = value; break;
                case 5: Item5 = value; break;
                case 6: Item6 = value; break;
                case 7: Item7 = value; break;
                case 8: Item8 = value; break;
                case 9: Item9 = value; break;
                default: throw new ArgumentOutOfRangeException($"Index was {index}");
            }
        } 
        get {
            switch(index) {
                case 0: return Item0;
                case 1: return Item1;
                case 2: return Item2;
                case 3: return Item3;
                case 4: return Item4;
                case 5: return Item5;
                case 6: return Item6;
                case 7: return Item7;
                case 8: return Item8;
                case 9: return Item9;
                default: throw new ArgumentOutOfRangeException($"Index was {index}");
            }
        }
    }

    // Custom Enumerator to support foreach loop wihout implementing IEnumerable
    public struct Enumerator : IEnumerator{
        private readonly Items10<T> _items;
        private int _index;

        public Enumerator(Items10<T> items) {
            _items = items;
            _index = -1;
        }
        
        public bool 
        MoveNext() {
            _index = _index + 1;
            return _index < 10;
        }

        public void Reset() {
            throw new NotImplementedException();
        }

        public T Current => _items[_index];

        object IEnumerator.Current => Current;
    }
    public readonly Enumerator GetEnumerator() => new Enumerator(this);
}
#else 
[InlineArray(10)]
public struct Items10<T> {
    private T __item0;
}
#endif

#if DEBUG || WASM || MACOS
// Ensure Items10 serialization produces identical bytes in both DEBUG and RELEASE modes.
// This allows debugging of databases created in either mode. Otherwise, we would need two database for DEBUG and RELEASE mode.
[BinarySerializable, BinarySerializeAsCollection]
public struct Items4<T> {
    public T Items0 {get; private set;}
    public T Items1 {get; private set;}
    public T Items2 {get; private set;}
    public T Items3 {get; private set;}
    public Items4(T items0 = default, T items1 = default, T items2 = default, T items3 = default) {
        Items0 = items0;
        Items1 = items1;
        Items2 = items2;
        Items3 = items3;
    }

    public T this[int index] {
        set {
            switch(index) {
                case 0: Items0 = value; break;
                case 1: Items1 = value; break;
                case 2: Items2 = value; break;
                case 3: Items3 = value; break;
                throw new ArgumentOutOfRangeException($"Index was {index}");
            }
        }
        get {
            switch(index) {
                case 0: return Items0;
                case 1: return Items1;
                case 2: return Items2;
                case 3: return Items3;
                default: throw new ArgumentOutOfRangeException($"Index was {index}");
            }
        }
    }

    // Custom Enumerator to support foreach loop wihout implementing IEnumerable
    public struct Enumerator : IEnumerator{
        private readonly Items4<T> _items;
        private int _index;

        public Enumerator(Items4<T> items) {
            _items = items;
            _index = -1;
        }
        
        public bool 
        MoveNext() {
            _index = _index + 1;
            return _index < 4;
        }

        public void Reset() {
            throw new NotImplementedException();
        }

        public T Current => _items[_index];

        object IEnumerator.Current => Current;
    }
    public readonly Enumerator GetEnumerator() => new Enumerator(this);
}
#else 
[InlineArray(4)]
public struct Items4<T> {
    private T __item1;  
}
#endif

public class
InlineListConverter<T> : JsonConverter<InlineList<T>> {
    
    public override InlineList<T>
    Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
        var list = new InlineList<T>();

        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Expected a JSON array for InlineList.");

        reader.Read();

        while (reader.TokenType != JsonTokenType.EndArray) {
            var item = JsonSerializer.Deserialize<T>(ref reader, options);
            list.Add(item);
            reader.Read();
        }

        return list;
    }

    public override void
    Write(Utf8JsonWriter writer, InlineList<T> value, JsonSerializerOptions options) {
        writer.WriteStartArray();

        foreach (var item in value) {
            if (item != null)
                JsonSerializer.Serialize(writer, item, options); // Serialize non-null items
            else
                writer.WriteNullValue();
        }

        writer.WriteEndArray();
    }
}

/*
 * A list of max 10 items that is allocated on the stack
 * Supports for each loop wihtout implementing IEnumerable.
 * We don't implement IEnumerable to avoid accidental boxing and allocation on the heap
 */
[StructLayout(LayoutKind.Sequential)]
[DebuggerDisplay("{DebugView}")]//this doesn't work at the moment, but maybe in the future MS fix
[BinarySerializable]
public struct 
InlineList<T> : IEquatable<InlineList<T>> {
    [Tag(1)] public Items10<T> Items;
    [Tag(2)] public int Count {get;private set;}
    public InlineList(Items10<T> items, int count) {
        Items = items;
        Count = count;
    }
    public InlineList(int initialSize) {
        if (initialSize > 10) throw new InvalidOperationException($"Maximimum seats length can be 10 but was {initialSize}");
        Count = initialSize;
    }

    //todo: rename to Count and delete
    public int Length => Count;
    [JsonIgnore] public T First => Count > 0 ? this[0] : throw new InvalidOperationException("The list is empty");
    public static InlineList<T> Empty => new();
    [JsonIgnore] public bool IsNotEmpty => Count != 0;
    [JsonIgnore] public bool IsEmpty => Count == 0;

    [JsonIgnore]    
    public T this[int index] {
        get => Items[index < Count ? index : throw new InvalidOperationException($"Index ({index}) was beyond the list bounds (count={Count})")];
        set => Items[index < Count ? index : throw new InvalidOperationException($"Index ({index}) was beyond the list bounds (count={Count})")] = value;
    }

    public InlineList<T> 
    With(T seat) {
        var result = this;
        result.Items[Count] = seat;
        result.Count++;
        return result;
    }

    public void
    Add(T seat) {
        Items[Count] = seat;
        Count++;
    }

    public void 
    AddRange(IEnumerable<T> items) {
        foreach(var item in items)
            Add(item);
    }

    [JsonIgnore]
    public T 
    Last { 
        get {
            if (Count == 0) throw new InvalidOperationException("List is empty");
            return Items[Count - 1];
        }
    }

    [JsonIgnore]
    public T 
    BeforeLast {
        get {
            if (Count <= 1) throw new InvalidOperationException("List doesn't contain the before last element");
            return Items[Count - 2];
        }
    }

    public void 
    Clear() => Count = 0;

    public static InlineList<T>
    CreateFilledWithDefaultValues(int count = 10) {
        var result = new InlineList<T>();
        for (int i = 0; i < count; i++) 
            result.Add(default);
        return result;
    }

    // Custom Enumerator to support foreach loop wihout implementing IEnumerable
    public struct Enumerator : IEnumerator{
        private readonly InlineList<T> _list;
        private int _index;

        public Enumerator(InlineList<T> list) {
            _list = list;
            _index = -1;
        }
        
        public bool 
        MoveNext() {
            _index = _index + 1;
            return _index < _list.Count;
        }

        public void Reset() {
            throw new NotImplementedException();
        }

        public T Current => _list[_index];

        object IEnumerator.Current => Current;
    }
    public readonly Enumerator GetEnumerator() => new Enumerator(this);

    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public Span<T> AsSpan() => MemoryMarshal.CreateSpan<T>(ref Unsafe.As<Items10<T>, T>(ref Items), Count);
    public static implicit operator Span<T>(InlineList<T> list) => list.AsSpan();

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public List<T> DebugView => this.GetDebugView();
    public unsafe List<T> GetDebugView()  {
       var result = new List<T>();
        foreach(var item in AsSpan())
            result.Add(item);
        return result;
    }

    public bool 
    Equals(InlineList<T> other) {
        if (other.Count != Count) return false;
        if (Count == 0) return true;
        var span = AsSpan();
        var otherSpan = other.AsSpan();
        for (int i = 0; i < span.Length; i++) 
            if (!span[i].Equals(otherSpan[i]))
                return false;
        return true;
    }
}

public static class 
InlineListHelper {
    
    public static InlineList<T>
    ToInlineList<T>(this IEnumerable<T> items) {
        var result = new InlineList<T>(); 
        result.AddRange(items);
        return result;
    }
    
    public static InlineList<TOut>
    Map<TIn, TArgument, TOut>(this in InlineList<TIn> items, Func<TIn, TArgument, TOut> selector, TArgument argument) {
        var result = new InlineList<TOut>(); 
        foreach(var item in items) 
            result.Add(selector(item, argument));
        return result;
    }
}
