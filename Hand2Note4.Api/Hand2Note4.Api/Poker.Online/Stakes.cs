namespace Hand2Note4.Api;

[BinarySerializable]
public readonly struct Stakes  {
    [Tag(1)] public double Ante { get;}
    [Tag(2)] public IBlinds Blinds { get;}
    //BB Ante that is posted only by a big blind. This often happens in live poker games
    [Tag(3)] public double BigBlindAnte {get;}
    public Stakes(double ante, IBlinds blinds, double bigBlindAnte) {
        Ante = ante;
        Blinds = blinds;
        BigBlindAnte = bigBlindAnte;
    }

    public Stakes(double ante = 0,
        double smallBlind = 0,
        double bigBlind = 0,
        double buttonBlind = 0,
        double smallBetAmount = 0,
        double bigBetAmount = 0) {
        Ante = ante;
        if (smallBetAmount.IsGreaterZero() || bigBetAmount.IsGreaterZero())
            Blinds = new FixedLimitBlinds(smallBlind, smallBetAmount, bigBetAmount);
        else if (buttonBlind.IsGreaterZero())
            Blinds = new ShortDeckBlinds(buttonBlind);
        else
            Blinds = new StandardBlinds(smallBlind, bigBlind);
    }
}

/// <summary>
/// Represents the rules of betting alive blinds in a hand.
/// </summary>
[TypeTag(1, typeof(FixedLimitBlinds))]
[TypeTag(2, typeof(ShortDeckBlinds))]
[TypeTag(3, typeof(StandardBlinds))]
public interface
IBlinds { }

public interface IHasSmallBlind {double SmallBlind {get;}}

[BinarySerializable]
public class
StandardBlinds : IBlinds, IHasSmallBlind{
    [Tag(1)] public double SmallBlind { get; private set;}
    [Tag(2)] public double BigBlind { get; private set;}
    public StandardBlinds(double smallBlind, double bigBlind) {
        SmallBlind = smallBlind.VerifyArgumentPositive(nameof(smallBlind));
        BigBlind = bigBlind.VerifyArgumentPositive(nameof(bigBlind));
        if (bigBlind.IsLess(smallBlind))
            throw new ArgumentException("Small blind can't be greater or equal a big blind");
    }
    public override int GetHashCode() => HashCode.Combine(SmallBlind, BigBlind);
}

[BinarySerializable]
public class
FixedLimitBlinds : IBlinds, IHasSmallBlind {
    [Tag(1)] public double SmallBlind { get;}
    [Tag(2)] public double SmallBet { get;}
    [Tag(3)] public double BigBet { get;}
    public FixedLimitBlinds(double smallBlind, double smallBet, double bigBet) {
        SmallBlind = smallBlind.VerifyArgumentPositive(nameof(smallBlind));
        SmallBet = smallBet.VerifyArgumentPositive(nameof(smallBet));
        BigBet = bigBet.VerifyArgumentPositive(nameof(bigBet));
        if (smallBlind.IsGreaterOrEqual(SmallBet)) throw new ArgumentException("Small blind can't be greater or equal small bet");
        if (smallBet.IsGreaterOrEqual(bigBet)) throw new ArgumentException("Small bet can't be greater or equal big bet");
    }
    public FixedLimitBlinds() { }
    public double GetMinBet(Streets street) => street <= Streets.Flop ? SmallBet : BigBet;
}

[BinarySerializable]
public class
ShortDeckBlinds : IBlinds {
    [Tag(1)] public double ButtonBlind { get;}
    public ShortDeckBlinds(double buttonBlind) => ButtonBlind = buttonBlind.VerifyArgumentPositive(nameof(buttonBlind));
    public ShortDeckBlinds() { }
}