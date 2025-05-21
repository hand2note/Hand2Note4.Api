namespace Hand2Note4.Api;

public enum 
WireType {
    VarInt = 0,
    VarNegativeInt = 1,
    Fixed32 = 2,
    Fixed64 = 3,
    LengthDelimited = 4,
    Default = 5
}

public static class 
Serialization{

    /// <summary>
    /// 536_870_911 is 29 bits max value
    /// </summary>
    private const int MaxEncodedFieldNumber = 536_870_911;
    public static long MaxLongAsVarInt = (1L << 7 * 7) - 1;
    public static int MaxIntAsVarInt = (1 << 3 * 7) - 1;
    
    internal static readonly UTF8Encoding UTF8 = new();
    
    public static void
    BinarySerializeWithLengthPrefix<T>(this T @object, Stream stream) {
        var writer = BinarySerializerWriter.ThreadShared(stream);
        writer.WriteWithLengthPrefix(@object, (writer, @object) => writer.WriteSerializableValueWithoutHeader(@object.GetSerializableValue(typeof(T)), tag: 0));
        writer.Flush();
    }
    
    public static byte[]
    BinarySerializeToBytes<T>(this T @object) => 
        @object.BinarySerializeToBytes(type: typeof(T));

    public static byte[]
    BinarySerializeToBytes<T>(this T @object, MemoryStream bufferStream) => 
        @object.BinarySerializeToBytes(type: typeof(T), bufferStream);

    public static byte[]
    BinarySerializeToBytes(this object @object, Type type) =>
        @object.BinarySerializeToBytes(type, bufferStream: new MemoryStream());

    public static byte[]
    BinarySerializeToBytes(this object @object, Type type, MemoryStream bufferStream) {
        if (bufferStream.Position != 0)
            bufferStream.SetLength(0);
        var writer = BinarySerializerWriter.ThreadShared(bufferStream);
        writer.WriteSerializableValueWithoutHeader(@object.GetSerializableValue(type), tag: 0).Flush();
        writer.Flush();
        return bufferStream.ToArray();
    }

    public static void
    BinarySerialize<T>(this T @object, Stream stream) {
        var writer = BinarySerializerWriter.ThreadShared(stream);
        writer.Serialize(@object);
        writer.Flush();
    }

    public static BinarySerializerWriter
    Serialize<T>(this BinarySerializerWriter stream, T @object) => 
        stream.WriteSerializableValueWithoutHeader(@object.GetSerializableValue(), tag: 0).Flush();
        
    //doesn't write any header (wireType, tag, or length)
    //the tag parameter is required to write dictionary and collections
    public static BinarySerializerWriter
    WriteSerializableValueWithoutHeader(this BinarySerializerWriter writer, ISerializableValue value, int tag) =>
        value switch {
            ObjectSerializableValue objectValue => writer.WriteObjectWithoutHeader(objectValue),
            CollectionSerializableValue collectionValue => writer.WriteCollection(collectionValue, tag),
            DictionarySerializableValue dictionaryValue => writer.WriteDictionary(dictionaryValue, tag),
            StringSerializableValue stringValue => writer.WriteString(stringValue.Value),
            ULongSerializableValue ulongValue => writer.WriteUlong(ulongValue.Value),
            DoubleSerializableValue doubleValue => writer.WriteDouble(doubleValue.Value),
            FloatSerializableValue doubleValue => writer.WriteFloat(doubleValue.Value),
            LongSerializableValue longValue => writer.WriteLong(longValue.Value),
            DefaultSerializableValue => writer,
            _ => throw new NotImplementedException(value.GetType().Name)
        };
    
    public static BinarySerializerWriter
    WriteSerializableValueWithHeader(this BinarySerializerWriter writer, int tag, ISerializableValue value) {
        //We write collections and dictionaries without tag, wiretype and length, 
        //becase write each item, key or value with its header
        if (value is CollectionSerializableValue or DictionarySerializableValue)
            return writer.WriteSerializableValueWithoutHeader(value, tag);

        var wireType = value.GetWireType();
        writer.WriteTagWireType(tag, wireType);
        if (value is StringSerializableValue stringValue)
            return writer.WriteString(stringValue.Value);

        if (wireType == WireType.LengthDelimited)
            return writer.WriteWithLengthPrefix(value, (writer, value) => writer.WriteSerializableValueWithoutHeader(value, tag));
        return writer.WriteSerializableValueWithoutHeader(value, tag);
    }

    private static BinarySerializerWriter
    WriteObjectWithoutHeader(this BinarySerializerWriter writer, ObjectSerializableValue @object) {
        //type tag is a part of interface object payload
        if (@object.TypeTag != 0)
            writer.WriteVarInt(@object.TypeTag);
        foreach (var (tag, value) in @object.Properties) 
            writer.WriteSerializableValueWithHeader(tag, value);
        
        return writer;
    }
  
    /// <summary>
    /// Long can be written either as varint, int, or long depending of it's value
    /// </summary>
    public static WireType 
    GetWireType(this long value) {
        if (value == long.MinValue)
            return WireType.Fixed64;
        
        var (varIntValue, varIntType) = value >= 0  
            ? (value, WireType.VarInt)
            : (value * -1, WireType.VarNegativeInt);

        if (varIntValue <= MaxIntAsVarInt) 
            return varIntType;
        
        if (varIntValue <= int.MaxValue) 
            return WireType.Fixed32;
        
        if (varIntValue <= MaxLongAsVarInt) 
            return varIntType;
        
        return WireType.Fixed64;
    }

    public static WireType GetWireType(this byte value) => ((long)value).GetWireType();
    public static WireType GetWireType(this int value) => ((long)value).GetWireType();
    public static WireType GetWireType(this short value) => ((long)value).GetWireType();
    public static WireType GetWireType(this uint value) => ((long)value).GetWireType();

    public static BinarySerializerWriter 
    WriteLongWithHeader(this BinarySerializerWriter writer, long value, int tag) {
        var wireType = value.GetWireType();
        writer.WriteTagWireType(tag, wireType);
        writer.WriteLong(value, wireType);
        return writer;
    }

    public static BinarySerializerWriter
    WriteDoubleWithHeader(this BinarySerializerWriter writer, double value, int tag) => 
        writer.WriteTagWireType(tag, WireType.Fixed64).WriteDouble(value);

    public static BinarySerializerWriter
    WriteFloatWithHeader(this BinarySerializerWriter writer, float value, int tag) => 
        writer.WriteTagWireType(tag, WireType.Fixed32).WriteFloat(value);

    public static BinarySerializerWriter
    WriteBoolWithHeader(this BinarySerializerWriter writer, bool value, int tag) => 
        writer.WriteLongWithHeader((long)(value ? 1 : 0), tag);

    public static BinarySerializerWriter
    WriteLong(this BinarySerializerWriter writer, long value) => 
        writer.WriteLong(value, wireType: value.GetWireType());

    public static BinarySerializerWriter
    WriteLong(this BinarySerializerWriter writer, long value, WireType wireType) {
        #if DEBUG 
        if (value.GetWireType() != wireType)
            throw new InvalidOperationException($"{value} should have wire type {value.GetWireType()} but was {wireType}");
        #endif
        return wireType switch {
            WireType.VarInt => writer.WriteVarInt(value),
            WireType.VarNegativeInt => writer.WriteVarNegativeInt(value),
            WireType.Fixed32 => writer.WriteInt32((int)value),
            WireType.Fixed64 => writer.WriteInt64(value),
            WireType.LengthDelimited => throw new InvalidOperationException("Number can't be length delimited"),
            _ => throw new NotImplementedException(wireType.ToString())
        };
    }

    public static BinarySerializerWriter
    WriteVarNegativeInt(this BinarySerializerWriter writer, long value) => 
        writer.WriteVarInt(value * -1);

    public static BinarySerializerWriter 
    WriteCollection(this BinarySerializerWriter writer, CollectionSerializableValue collectionValue, int tag) {
        foreach (var value in collectionValue.Values) 
            writer.WriteSerializableValueWithHeader(value: value, tag: tag);
        
        return writer;
    }
    
    public static BinarySerializerWriter 
    WriteDictionary(this BinarySerializerWriter writer, DictionarySerializableValue dictionary, int tag) {
        foreach (var keyValue in dictionary.KeyValues) 
            writer
                .WriteSerializableValueWithHeader(tag, value: keyValue);
        
        return writer;
    }

    public static BinarySerializerWriter
    WriteTagWireType(this BinarySerializerWriter writer, int tag, WireType wireType) => 
        writer.WriteVarInt(tag.EncodeTagWireType(wireType));

    public static BinarySerializerWriter
    WritePlainBytesTagWireType(this BinarySerializerWriter writer, int tag) => 
        writer.WriteTagWireType(tag, WireType.Default);

    public static int
    EncodeTagWireType(this int tag, WireType wireType) {
        if (tag is < 0 or > MaxEncodedFieldNumber) 
            throw new ArgumentOutOfRangeException(nameof(tag), "Field number must be between 0 and 536,870,911 (inclusive).");
        
        var typeCode = (int)wireType;

        if (typeCode is < 0 or > 7) 
            throw new ArgumentOutOfRangeException(nameof(typeCode), "Invalid wire type.");
        
        return (tag << 3) | typeCode;
    }
    
    public static BinarySerializerWriter
    WriteString(this BinarySerializerWriter writer, string @string, int tag) {
        writer.WriteTagWireType(tag, WireType.LengthDelimited);
        writer.WriteString(@string);
        return writer;
    }

    public static BinarySerializerWriter
    WriteString(this BinarySerializerWriter writer, string @string) {
        var length = UTF8.GetByteCount(@string); 
        return writer
            .WriteVarInt(length)
            .WriteString(@string, expectedBytes: length);
    }

    public static BinarySerializerWriter
    WriteWithLengthPrefix<T>(this BinarySerializerWriter writer, T value, Action<BinarySerializerWriter, T> writeValue, int tag) => 
        writer
            .WriteTagWireType(tag, WireType.LengthDelimited)
            .WriteWithLengthPrefix(value, writeValue);

    public static WireType
    GetWireType(this ISerializableValue value) => 
        value switch {
            LongSerializableValue longValue => longValue.Value.GetWireType(),
            ULongSerializableValue or DoubleSerializableValue => WireType.Fixed64,
            FloatSerializableValue => WireType.Fixed32,
            ObjectSerializableValue or CollectionSerializableValue or DictionarySerializableValue or StringSerializableValue => WireType.LengthDelimited,
            DefaultSerializableValue => WireType.Default,
            _ => throw new NotImplementedException(value.GetType().Name)
        };

    [BinarySerializationConverter]
    public static GuidSerializable
    GetGuidSerializable(this Guid guid) {
        var bytes = guid.ToByteArray();
        var higherBits = BitConverter.ToInt64(bytes, 0);
        var lowerBits = BitConverter.ToInt64(bytes, 8);
        return new GuidSerializable(higherBits, lowerBits);
    }

    [BinaryDeserializationConverter]
    public static Guid 
    GetGuid(this GuidSerializable bytes) {
        var guidBytes = new byte[16];
        BitConverter.GetBytes(bytes.HigherBits).CopyTo(guidBytes, 0);
        BitConverter.GetBytes(bytes.LowerBits).CopyTo(guidBytes, 8);
        return new Guid(guidBytes); 
    }

    public static void
    WriteDateTime(this BinarySerializerWriter writer, DateTime value) {
        writer.WriteLongWithHeader(value.Ticks, 1);
    }

    public static void
    WriteTimeSpan(this BinarySerializerWriter writer, TimeSpan value) {
        writer.WriteLongWithHeader(value.Ticks, 1);
    }
    
    public static void
    WriteGuid(this BinarySerializerWriter writer, Guid value) {
        var guidSerializable = value.GetGuidSerializable();
        if (guidSerializable.HigherBits != 0)
            writer.WriteLongWithHeader(guidSerializable.HigherBits, tag: 1);
        if (guidSerializable.LowerBits != 0)
            writer.WriteLongWithHeader(guidSerializable.LowerBits, tag: 2);
    }

    public static int
    GetVarIntSize(this int value) => 
        GetVarIntSize((long)value);

    public static int 
    GetVarIntSize(this long value) {
        //fast check for small value 
        if (value <= 0b_0111_1111)
            return 1;

        value >>= 7;
        var result = 2;
        while ((value >>= 7) != 0) result++; 

        return result; 
    }
}
