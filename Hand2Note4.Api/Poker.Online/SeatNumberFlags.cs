namespace Hand2Note4.Api;

[BinarySerializable]
public readonly struct 
SeatNumberFlags: IEquatable<SeatNumberFlags> {
    [Tag(1)] public int Flags {get;}
    public SeatNumberFlags(int flags) => Flags = flags;

    public static readonly SeatNumberFlags AllSeatNumbers = new(flags: (1 << 10) - 1);

    public static SeatNumberFlags Empty = new(); 
    
    public SeatNumberFlags Add(int seatNumber) {
        return new SeatNumberFlags(Flags | (1 << (seatNumber - 1)));
    }
    public bool Contains(int seatNumber) => (Flags & (1 << (seatNumber - 1))) != 0;
    public SeatNumberFlags Add(SeatNumberFlags other) => new (Flags | other.Flags);
    public SeatNumberFlags Remove(SeatNumberFlags other) => new (Flags & ~(other.Flags));
    public SeatNumberFlags Remove(int seatNumber) => new (Flags & ~(1 << (seatNumber - 1)));

    public static bool operator ==(SeatNumberFlags seatNumbers, SeatNumberFlags other) => seatNumbers.Flags == other.Flags;
    public static bool operator !=(SeatNumberFlags seatNumbers, SeatNumberFlags other) => seatNumbers.Flags != other.Flags;

    public struct Enumerator {
        public SeatNumberFlags _flags;    
        public Enumerator(SeatNumberFlags flags) => _flags = flags;

        public int Current => _currentSeatNumber;
        private int _currentSeatNumber; 

        public bool 
        MoveNext() {
            _currentSeatNumber++;
            if (_currentSeatNumber > 10) return false;
            if (_flags.Contains(_currentSeatNumber))
                return true;
            return MoveNext();
        }
    }
    
    public List<int> AsList {
        get {
            var result = new List<int>();
            foreach(var seatNumber in this)
               result.Add(seatNumber);
            return result;
        }
    }
    public readonly Enumerator GetEnumerator() => new Enumerator(this);
    public override string ToString() => $"Seats #{AsList.AggregateToString(",")}";
    public override bool Equals(object? @object) => @object is SeatNumberFlags flags && flags.Flags == Flags;
    public override int GetHashCode() => HashCode.Combine(Flags);
    public bool Equals(SeatNumberFlags other) => Flags == other.Flags;
} 

