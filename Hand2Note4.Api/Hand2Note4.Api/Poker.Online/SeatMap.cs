using System.Runtime.InteropServices;

namespace Hand2Note4.Api;
 
public interface IHasSeatNumber { int SeatNumber {get; } }
public interface IHasUsername { string Username {get; } }

[StructLayout(LayoutKind.Sequential)]
[BinarySerializable]
public struct 
SeatMap<T>{
    [Tag(1)] public SeatNumberFlags SeatNumberFlags {get; private set;}
    [Tag(2)] public Items10<T> InnerSeats;
    public SeatMap(Items10<T> innerSeats, SeatNumberFlags seatNumberFlags) {
        InnerSeats = innerSeats;
        SeatNumberFlags = seatNumberFlags;
    }

    public static readonly SeatMap<T> Empty = new();

    public SeatMap<T>
    With(int seatNumber, T value) {
        var result = new SeatMap<T>();
        result.InnerSeats = InnerSeats;
        result.SeatNumberFlags = SeatNumberFlags;
        result.Set(seatNumber, value);
        return result;
    }

    public bool Contains(int seatNumber) => SeatNumberFlags.Contains(seatNumber);
    public T Get(int seatNumber) => TryGet(seatNumber, out var result) ? result : throw new InvalidOperationException($"Seat #{seatNumber} not found");
    public void Set(int seatNumber, T value) => this[seatNumber] = value;

   
    public bool 
    TryGet(int seatNumber, out T result) {
        if (Contains(seatNumber)) {
            result = InnerSeats[seatNumber - 1];
            return true;
        }
        result = default;
        return false;
    }

    public SeatMap<T>
    Remove(int seatNumber) {
        var result = this;
        result.SeatNumberFlags = result.SeatNumberFlags.Remove(seatNumber);
        return result;
    }

    public static implicit operator
    InlineList<T>(SeatMap<T> seats) => seats.Values;

    public InlineList<T>
    Values { get {
        var result = new InlineList<T>();
        for (int seatNumber = 1; seatNumber <= 10; seatNumber++) {
            if (SeatNumberFlags.Contains(seatNumber))
                result.Add(InnerSeats[seatNumber -1]);
            if ((1 << (seatNumber - 1)) >= SeatNumberFlags.Flags)
                return result;
        }
        
        return result;
    }}

    public T this[int seatNumber] {
        get => InnerSeats[seatNumber - 1];
        private set {
            InnerSeats[seatNumber - 1] = value;
            SeatNumberFlags = SeatNumberFlags.Add(seatNumber);
        }
    }
    
    // Custom Enumerator to support foreach loop wihout implementing IEnumerable
    public struct Enumerator {
        private readonly SeatMap<T> _seats;
        private int _currentSeatNumber;

        public Enumerator(SeatMap<T> seats) {
            _seats = seats;
        }
        
        public bool 
        MoveNext() {
            _currentSeatNumber++;
            if (_currentSeatNumber > 10)
                return false;
            if (_seats.Contains(_currentSeatNumber))
                return true;
            return MoveNext();
        }

        public (int seatNumber, T value) Current => (_currentSeatNumber, _seats[_currentSeatNumber]);
    }

    public readonly Enumerator GetEnumerator() => new Enumerator(this);
    
}

