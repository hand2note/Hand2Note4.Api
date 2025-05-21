namespace Hand2Note4.Api;
    
public interface IHasPotNumber { int PotNumber {get;}}
/// <summary>
/// Represents a single pot in a bank. Main pot is a <see cref="Pot"/> as well as each side pot is a separate <see cref="Pot"/>
/// </summary>
public class 
Pot: IHasPotNumber {
    public const int MainPotNumber = 1;
    
    public double PotSize {get; private set;}
    /// <summary>
    /// Main pot has always a zero <see cref="PotNumber"/>
    /// </summary>
    public int PotNumber {get; private set;}
    public SeatNumberFlags Pretenders {get; private set;}
    //Sum of all invested money into the pot may not necessary be equal the total pot size because
    //the rake may be excluded from the pot size.
    public SeatMap<double> Investors {get; private set;}

    public static Pot
    Create(double potSize, int potNumber, SeatNumberFlags pretenders, SeatMap<double> investors) {
        var result = ObjectPool<Pot>.ThreadShared.RentObject();
        result.PotSize = potSize.VerifyArgumentNotNegative(nameof(potSize));
        result.PotNumber = potNumber;
        result.Pretenders = pretenders;
        result.Investors = investors;
        return result;
    }

    public static Pot[] CreateArray(int size) => ArrayPool<Pot>.ThreadShared.GetArray(size);

    public Pot (){}

    public bool IsSidePot => PotNumber != MainPotNumber;
    public bool IsMainPot => PotNumber == MainPotNumber;

    public override string ToString() => $"Pot #{PotNumber} {PotSize}";
}


[BinarySerializable]
public struct 
BankRake {
    [Tag(1)] public PotMap<double> PotRakes {get; private set;}
    public BankRake(PotMap<double> potRakes) {
        PotRakes = potRakes;   
        foreach(var rake in potRakes.Values)
            if (rake.IsZero())
                throw new ArgumentException($"Pots with zero rakes should be removed from the {nameof(BankRake)} object");
            else if (rake.IsLessZero())
                throw new ArgumentException("Rake can't be negative");
    }

    public static BankRake
    Create(PotMap<double> potRakes) => new BankRake(potRakes);
}
