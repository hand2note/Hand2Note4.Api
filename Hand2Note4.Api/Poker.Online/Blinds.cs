namespace Hand2Note4.Api;

/// <summary>
/// Represents types of blinds posted in turn.
/// Blind is an amount posted as a bet.
/// Amount posted into center of the table is a dead blind.
/// </summary>
[Flags] public enum 
RegularBlindTypes {
    None = 0,
    SmallBlind = 1,
    BigBlind = 1 << 1,
    ButtonBlind = 1 << 2,
    //Player that skipped straddle has BlindType == BlindTypes.None
    FirstStraddle  = 1 << 3,
    SecondStraddle = 1 << 4,
    ThirdStraddle = 1 << 5,
    FourthStraddle = 1 << 6,
    FifthStraddle = 1 << 7,
    SixthStraddle = 1 << 8,
    SeventhStraddle = 1 << 9,
    EighthStraddle = 1 << 10,
    AutoAllin = 1 << 11,
    
    AllStraddles = FirstStraddle | SecondStraddle | ThirdStraddle | FourthStraddle | FifthStraddle | SixthStraddle | SeventhStraddle | EighthStraddle,
}

/// <summary>
/// Represents types of blinds posted to enter the hand out of queue which are "live", i.e. treated as a bet. 
/// Note that SmallBlind can't be posted out of queue because it can be posted only as a dead blind
/// See https://en.wikipedia.org/wiki/Blind_(poker)
/// </summary>
public enum 
LiveMissedBlindType {
    None,
    BigBlind, 
    ButtonBlind,

    //Some poker rooms set a dealer to a small blind poster.
    //And then the small blind acts last on postflop.
    //We may treat it as a posting small blind out of queue and a hand without a small blind.
    //This allow us to process the hand successfully instead of just loosing it.
    //Also, on some rooms, for example Chico, player may post live small blind out of queue
    SmallBlind,

    // Some poker rooms don't support dead blinds. For example, on IPoker player posts both small and big blinds as a live bet.
    BigAndSmallBlind,
    
    FirstStraddle,
}

/// <summary>
/// Represents types of blind posted into center of the table to enter the hand out of queue.
/// </summary>
public enum 
DeadBlindType {
    None,
    SmallBlind,
    BigBlind,//in straddle games big blind may be posted into the center pot
    ButtonBlind,
    SmallAngBigBlinds,
    FirstStraddle,
    SecondStraddle
}

/// <summary>
/// Represents a blind and amount posted by a player in a hand.
/// </summary>
[BinarySerializable]
public readonly struct 
RegularPostedBlind: IEquatable<RegularPostedBlind> {
    [Tag(1)] public RegularBlindTypes Type {get; }
    [Tag(2)] public double Amount {get; }
    public RegularPostedBlind(RegularBlindTypes type, double amount) {
        if (type == RegularBlindTypes.None && amount.IsNotZero())
            throw new ArgumentException($"Invalid posted blind {type}, {amount}");
        Type = type;
        Amount = amount.VerifyArgumentNotNegative(nameof(amount));
    }

    public static RegularPostedBlind None = new(RegularBlindTypes.None, 0);
    public bool IsNone => Type == RegularBlindTypes.None; 
    public bool IsNotNone => Type != RegularBlindTypes.None; 
    public bool Equals(RegularPostedBlind other) => Type == other.Type && Amount.IsEqual(other.Amount);
    public override bool Equals(object? obj) => obj is RegularPostedBlind other && Equals(other);
    public override int GetHashCode() => HashCode.Combine((int)Type, Amount);
    public override string ToString() => $"{Type} {Amount}";
}

/// <summary>
/// Blind out of queue and its amount posted by a player in a hand.
/// </summary>
[BinarySerializable]
public readonly struct 
LiveMissedPostedBlind: IEquatable<LiveMissedPostedBlind> {
    [Tag(1)] public LiveMissedBlindType Type {get; }
    [Tag(2)] public double Amount {get; }
    public LiveMissedPostedBlind(LiveMissedBlindType type, double amount) {
        if (type == LiveMissedBlindType.None && amount.IsNotZero() || type != LiveMissedBlindType.None && amount.IsZero())
            throw new ArgumentException($"Invalid posted blind {type}, {amount}");
        Type = type;
        Amount = amount.VerifyArgumentNotNegative(nameof(amount));
    }

    public static LiveMissedPostedBlind None = new(LiveMissedBlindType.None, 0);
    public bool IsNone => Type == LiveMissedBlindType.None;

    public bool Equals(LiveMissedPostedBlind other) => Type == other.Type && Amount.IsEqual(other.Amount);
    public override bool Equals(object? obj) => obj is LiveMissedPostedBlind other && Equals(other);
    public override int GetHashCode() => HashCode.Combine((int)Type, Amount);
    public override string ToString() => $"{Type} {Amount}";
}

/// <summary>
/// Dead blind and its amount posted by a player in a hand.
/// </summary>
[BinarySerializable]
public readonly struct 
DeadPostedBlind: IEquatable<DeadPostedBlind>{
    [Tag(1)] public DeadBlindType Type {get; }
    [Tag(2)] public double Amount {get; }
    public DeadPostedBlind(DeadBlindType type, double amount) {
        if (type == DeadBlindType.None && amount.IsNotZero() || type != DeadBlindType.None && amount.IsZero())
            throw new ArgumentException($"Invalid posted blind {type}, {amount}");
        Type = type;
        Amount = amount.VerifyArgumentNotNegative(nameof(amount));
    }
    public static DeadPostedBlind None = new(DeadBlindType.None, 0);
    public bool IsNone => Type == DeadBlindType.None;
    public bool IsNotNone => Type != DeadBlindType.None;

    public bool Equals(DeadPostedBlind other) => Type == other.Type && Amount.IsEqual(other.Amount);
    public override bool Equals(object? obj) => obj is DeadPostedBlind other && Equals(other);
    public override int GetHashCode() => HashCode.Combine((int)Type, Amount);
    public override string ToString() => $"{Type} {Amount}";
}

