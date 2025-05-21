namespace Hand2Note4.Api;

public interface ISerializableValue {}

public record 
ObjectSerializableValue(int TypeTag, Dictionary<int, ISerializableValue> Properties) : ISerializableValue {
    public bool 
    TryGetPropertyValue(int tag, out ISerializableValue result) {
        return Properties.TryGetValue(tag, out result);
    } 
}

public record CollectionSerializableValue(ImmutableList<ISerializableValue> Values) : ISerializableValue;
public record DictionarySerializableValue(ImmutableList<ObjectSerializableValue> KeyValues): ISerializableValue;
public record StringSerializableValue(string Value) : ISerializableValue;
public record LongSerializableValue(long Value) : ISerializableValue;
public record ULongSerializableValue(ulong Value) : ISerializableValue;
public record DoubleSerializableValue(double Value) : ISerializableValue;
public record FloatSerializableValue(float Value) : ISerializableValue;
/// <summary>
/// Is used to save default values in collections
/// </summary>
public record DefaultSerializableValue : ISerializableValue{
    public static DefaultSerializableValue Instance => new DefaultSerializableValue();
};

public class 
TagAttribute:Attribute{
    public int Value {get;}
    public TagAttribute(int value) {
        Value = value;
        if (value <= 0) 
            throw new ArgumentException("Tag must be positive");
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = false)]
public class 
TypeTagAttribute:Attribute{
    public int Tag {get;}
    public Type Type {get;}
    public TypeTagAttribute(int tag, Type type) {
        Tag = tag;
        Type = type;
    }
}

public class BinarySerializableAttribute: Attribute {}
public class BinaryDeserializationConstructorAttribute : Attribute { }

//now this doesn't work with generators
public class BinarySerializationConverterAttribute : Attribute {}
public class BinaryDeserializationConverterAttribute : Attribute {}

public class GenerateCollectionSerializerAttribute : Attribute {}
public class BinarySerializeAsCollectionAttribute : Attribute {}

public class SerializeAsPlainBytesAttribute : Attribute {}

[BinarySerializable]
public struct GuidSerializable {
    [Tag(1)]public long HigherBits {get;}
    [Tag(2)]public long LowerBits {get;}
    public GuidSerializable(long higherBits, long lowerBits) {
        HigherBits = higherBits;
        LowerBits = lowerBits;
    }
}
