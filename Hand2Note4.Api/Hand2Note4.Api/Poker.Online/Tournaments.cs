namespace Hand2Note4.Api;

[BinarySerializable]
public sealed record 
Tournament {
    [Tag(1)] public Rooms Room {get;}
    [Tag(2)] public long TournamentId {get; }
    //In Freerolls the  BuyInCurrency can be None if it's unknow. However, 
    //if user wons the freeroll then we can know the currency from the hand history.
    [Tag(3)] public Currencies BuyInCurrency {get;init; }
    /// <summary>
    /// With rake included
    /// </summary>
    [Tag(4)] public double BuyInWithRake {get; }
    [Tag(5)] public double Rake {get; }
    [Tag(6)] public double KnockoutBounty {get;}
    [Tag(7)] public string? UniqueTournamentName {get; }
    [Tag(8)] public bool? IsMtt {get;}
    [Tag(9)] public TournamentSpeedTypes SpeedType {get;}
    [Tag(10)] public TournamentTypes Type { get;}
    public Tournament(Rooms room, long tournamentId, double buyInWithRake, double rake,  Currencies buyInCurrency, string? uniqueTournamentName, double knockoutBounty = 0, TournamentTypes type = TournamentTypes.Default, bool? isMtt = null, TournamentSpeedTypes speedType = TournamentSpeedTypes.Normal) {
        Room = room;
        BuyInWithRake = buyInWithRake.VerifyArgumentNotNegative(nameof(buyInWithRake));
        Rake = rake.VerifyArgumentNotNegative(nameof(rake));
        KnockoutBounty = knockoutBounty.VerifyArgumentNotNegative(nameof(knockoutBounty));
        BuyInCurrency = buyInCurrency;
        UniqueTournamentName = uniqueTournamentName;
        Type = type;
        TournamentId = tournamentId;
        IsMtt = isMtt;
        SpeedType = speedType;
    }
}

[Flags]
public enum 
TournamentSpeedTypes {
    Normal = 0, 
    Turbo = 1,
    SuperTurbo = 1 << 1
}

[Flags]
public enum TournamentTypes {
    Default = 0,
    Freeroll = 1,
    Shootout = 1 << 1,
    SpinNGo = 1 << 2,
    DoubleOrNothing = 1 << 3,
    Knockout = 1 << 4,
    Blast = 1 << 5,
    WildTwister = 1 << 6
}

