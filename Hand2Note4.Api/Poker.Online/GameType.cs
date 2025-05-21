namespace Hand2Note4.Api;

[BinarySerializable]
public class 
PokerGameType {
    [Tag(1)] public PokerGames PokerGame {get; private set; }
    [Tag(2)] public BetLimits BetLimits {get; private set; }
    [Tag(3)] public SessionTypes SessionType {get; private set;}
    [Tag(4)] public bool IsZoom {get; private set;}
    [Tag(5)] public bool IsHomeGame {get; private set;}
    [Tag(6)] public double CapAmount {get; private set;}
    //In Bomb Pots everyone posts antes, no blinds, and action starts on Flop
    [Tag(7)] public bool IsBombPot {get; private set;}
    //When all players automatically go all-in preflop
    [Tag(8)] public bool IsAutoAllin {get; private set;}

    public PokerGameType(PokerGames pokerGame, BetLimits betLimits, SessionTypes sessionType, bool isZoom, bool isHomeGame, double capAmount, bool isBombPot, bool isAutoAllin) {
        PokerGame = pokerGame;
        BetLimits = betLimits;
        SessionType = sessionType;
        IsZoom = isZoom;
        IsHomeGame = isHomeGame;
        CapAmount = capAmount;
        IsBombPot = isBombPot;
        IsAutoAllin = isAutoAllin;
    }

    public PokerGameType(PokerGames pokerGame, BetLimits betLimits, SessionTypes sessionType) {
        PokerGame = pokerGame;
        BetLimits = betLimits;
        SessionType = sessionType;
    }

    public PokerGameType(PokerGames pokerGame, BetLimits betLimits, SessionTypes sessionType, bool isZoom) {
        PokerGame = pokerGame;
        BetLimits = betLimits;
        SessionType = sessionType;
        IsZoom = isZoom;
    }
    
    public bool IsCash => SessionType == SessionTypes.Cash;
    public bool IsTournament => !IsCash;
}

[Flags]
public enum 
BetLimits: byte {
    NoLimit = 1,
    PotLimit = 1 << 1,
    Limit = 1 << 2,
    CapNoLimit = 1 << 3,
    CapPotLimit = 1 << 4,
    All = NoLimit | PotLimit | Limit | CapNoLimit | CapPotLimit,
    AllExceptLimit = All ^ Limit,
    NoLimitPotLimit = NoLimit | PotLimit
}
