using System.Diagnostics;
using BinaryPrimitives = System.Buffers.Binary.BinaryPrimitives;

namespace Hand2Note4.Api;

public static class 
BinarySerializerWriterFunctions {

    internal static readonly UTF8Encoding UTF8 = new UTF8Encoding();
   
    public static BinarySerializerWriter
    WriteVarInt(this BinarySerializerWriter writer, long value) {
        if (value < 0) 
            if (value == long.MinValue)
                throw new InvalidOperationException("Number is too large, use fixed64 instead)");
            else
                value *= -1;

        while (true) {
            var byteValue = (byte)(value & 0b_0111_1111);
            value >>= 7;

            if (value != 0)
                byteValue |= 0b_1000_0000; 

            writer.WriteByte(byteValue);

            if (value == 0)
                break;
        }

        return writer;
    }
        
    public static BinarySerializerWriter 
    WriteString(this BinarySerializerWriter writer, string value, int expectedBytes) {
        var actualBytes = UTF8.GetBytes(value, writer.GetNextSpan(length: expectedBytes));
        writer.AdvancePosition(actualBytes);
        Debug.Assert(expectedBytes == actualBytes);
        return writer;
    }
    
    public static BinarySerializerWriter
    WriteUint32(this BinarySerializerWriter writer, uint value) {
        BinaryPrimitives.WriteUInt32LittleEndian(writer.GetNextSpan(length: 4), value);     
        return writer.AdvancePosition(4);
    }

    public static BinarySerializerWriter
    WriteInt32(this BinarySerializerWriter writer, int value) {
        BinaryPrimitives.WriteInt32LittleEndian(writer.GetNextSpan(length: 4), value);     
        return writer.AdvancePosition(4);
    }

    public static BinarySerializerWriter
    WriteUlong(this BinarySerializerWriter writer, ulong value) {
        BinaryPrimitives.WriteUInt64LittleEndian(writer.GetNextSpan(length: 8), value);
        return writer.AdvancePosition(8);
        
    }

    public static BinarySerializerWriter
    WriteInt64(this BinarySerializerWriter writer, long value) {
        BinaryPrimitives.WriteInt64LittleEndian(writer.GetNextSpan(length: 8), value);
        return writer.AdvancePosition(8);
    }

    public static BinarySerializerWriter
    WriteFloat(this BinarySerializerWriter writer, float value) {
        BinaryPrimitives.WriteSingleLittleEndian(writer.GetNextSpan(length: 4), value);     
        writer.AdvancePosition(4);
        return writer;
        
    }

    public static BinarySerializerWriter
    WriteDouble(this BinarySerializerWriter writer, double value) {
        BinaryPrimitives.WriteDoubleLittleEndian(writer.GetNextSpan(length: 8), value);
        writer.AdvancePosition(8);
        return writer;
    }

    public static BinarySerializerWriter
    WriteWithLengthPrefix<T>(this BinarySerializerWriter writer, T value, Action<BinarySerializerWriter, T> writeValue) {
        var initialPosition = writer.Position;
        //1 byte for the length
        var itemStartPosition = initialPosition + 1;
        writer.AdvancePosition();
        writeValue(writer, value);
       
        // by default we only reserved one byte;
        // if the prefix turns out to need more than this then
        // we need to shuffle the existing data
        var length = writer.Position - itemStartPosition;
        if (length < 0)
            throw new InvalidOperationException("Object with empty properties should not be serialized");

        var lengthSize = length.GetVarIntSize();
        if (lengthSize == 1) 
            writer.Buffer[initialPosition] = (byte)(length & 0b_0111_1111);
        else {
            writer.AllocateSpace(lengthSize);
            Array.Copy(
                sourceArray: writer.Buffer, 
                sourceIndex: itemStartPosition, 
                destinationArray: writer.Buffer, 
                destinationIndex: initialPosition + lengthSize, 
                length: length);
            //moving backwards and writing the length
            writer
                .MoveTo(initialPosition)
                .WriteVarInt(length)
                .MoveTo(initialPosition + lengthSize + length);
        }

        return writer;
    }
}
