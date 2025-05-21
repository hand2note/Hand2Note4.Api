namespace Hand2Note4.Api;

[BinarySerializable]
public struct PotMap<T> {
    [Tag(1)] public Items10<T> InnerValues;
    [Tag(2)] public int PotNumbers {get;private set;}
    public PotMap(Items10<T> innerValues, int potNumbers) {
        InnerValues = innerValues;
        PotNumbers = potNumbers;
    }
    
    public InlineList<T> Values => PotNumbers.GetBitNumbers().Map(static (potNumber, potMap) => potMap.Get(potNumber), this);
    
    public static PotMap<T> Empty => new();
    public PotMap<T> 
    With(int potNumber, T value) {
        var result = this;
        result.Add(potNumber.VerifyPotNumber(), value);
        return result;
    }
    
    public void 
    Add(int potNumber, T value) {
        InnerValues[potNumber.VerifyPotNumber() - Pot.MainPotNumber] = value;
        PotNumbers = PotNumbers | (1 << (potNumber - Pot.MainPotNumber));
    }
    
    private T Get(int potNumber) => TryGet(potNumber, out var result) ? result : throw new InvalidOperationException($"Failed to get value for the potNumber={potNumber}");
    
    public bool 
    TryGet(int potNumber, out T result) {
        if (Contains(potNumber)) {
            result = InnerValues[potNumber - Pot.MainPotNumber];
            return true;
        }

        result = default;
        return false;
    }
    
    public bool 
    Contains(int potNumber) => PotNumbers.ContainsBit(potNumber);
}

public static class
PotHelper {
    
    public static int 
    VerifyPotNumber(this int potNumber) {
        if (potNumber < Pot.MainPotNumber || potNumber > Pot.MainPotNumber + 8)
            throw new InvalidOperationException($"Pot number ({potNumber.Quoted()}) is out of range") ;
        return potNumber;
    }
}
