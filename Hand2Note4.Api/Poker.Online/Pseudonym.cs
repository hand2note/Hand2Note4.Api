namespace Hand2Note4.Api;

[TypeTag(1, typeof(NoPseudonym))]
[TypeTag(2, typeof(Pseudonym))]
public interface IPseudonym { }

[BinarySerializable]
public class NoPseudonym : IPseudonym, IEquatable<NoPseudonym> { 
    public static readonly NoPseudonym Instance = new();
    
    [BinaryDeserializationConstructor] public static NoPseudonym Create() => Instance;

    public bool Equals(NoPseudonym? other) => true;
    public override bool Equals(object? obj) {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((NoPseudonym)obj);
    }

    public override int GetHashCode() {
        return 1;
    }
}
[BinarySerializable]
public readonly struct Pseudonym  : IPseudonym {
    [Tag(1)]
    public string Value { get;}
    public Pseudonym(string value) => Value = value;
}
