namespace Hand2Note4.Api;

public class
BinarySerializerWriter {
    public static ThreadLocal<BinarySerializerWriter> _threadShared = new ThreadLocal<BinarySerializerWriter>(
        () => new BinarySerializerWriter(
            buffer: new byte[1024 * 1024], 
            destination: null));

    private Stream Destination;
    public byte[] Buffer;
    public int Position {get; private set;}
    public BinarySerializerWriter(byte[] buffer, Stream destination) {
        Destination = destination;
        Buffer = buffer;
    }
    
    public static BinarySerializerWriter 
    ThreadShared(Stream stream) {
        _threadShared.Value.Destination = stream;
        _threadShared.Value.Position = 0;
        return _threadShared.Value;
    }

    public int BytesLeft => Buffer.Length - Position;

    public Span<byte> 
    GetNextSpan(int length) {
        AllocateSpace(size: length);
        return Buffer.AsSpan().Slice(Position, length);
    }

    public BinarySerializerWriter
    AllocateSpace(int size) {
        if (BytesLeft < size) {
            if (Position + size > Array.MaxLength) 
                throw new InvalidOperationException($"Can't allocate so much space ({size:N0} bytes).");
            
            var newBuffer = new byte[Math.Max(Position + size, Buffer.Length <= Array.MaxLength / 2 ? Buffer.Length * 2 : Array.MaxLength)];
            if (Position > 0)
                Array.Copy(
                    sourceArray: Buffer, 
                    sourceIndex: 0, 
                    destinationArray: newBuffer, 
                    destinationIndex: 0, 
                    length: Position);
            Buffer = newBuffer;
        }
        return this;
    }

    public BinarySerializerWriter 
    AdvancePosition(int offset = 1) {
        AllocateSpace(offset);
        Position += offset;
        return this;
    }

    public BinarySerializerWriter
    Flush() {
        if (Position != 0) {
            Destination.Write(Buffer, 0, Position);
            Position = 0;
        }
        return this;
    }

    public void
    WriteByte(byte @byte) {
        AllocateSpace(1);
        Buffer[Position] = @byte;
        AdvancePosition();
    }

    public BinarySerializerWriter 
    MoveTo(int position) {
        position.VerifyNotNegative(nameof(position));
        Position = position;
        return this;
    }

    public void 
    Skip(int count) {
        Position += count;
    }

    public unsafe void 
    WritePlainBytes<T>(T value) {
        var size = sizeof(T);
        AllocateSpace(size);
        fixed(byte* pointer = &Buffer[Position]) {
            ((T*)pointer)[0] = value;
            Skip(size);
        }
    }

    public void
    WriteBytes(Span<byte> bytes) {
        AllocateSpace(bytes.Length);
        bytes.CopyTo(GetNextSpan(length: bytes.Length));
        Skip(bytes.Length);
    }

}
