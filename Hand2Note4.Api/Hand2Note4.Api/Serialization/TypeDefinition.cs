namespace Hand2Note4.Api;

public interface ITypeDefinition {}

public record  
ObjectTypeDefinition(
    Type Type, 
    ImmutableDictionary<int, ITypeDefinition> Properties) : ITypeDefinition {

    public ITypeDefinition GetPropertyType(int tag) => Properties.TryGetValue(tag, out var result) ? result : throw new InvalidOperationException($"Property with tag {tag} not found");
    public bool TryGetPropertyType(int tag, out ITypeDefinition result) => Properties.TryGetValue(tag, out result);
}

public record NumericTypeDefinition(Type Type) : ITypeDefinition;
public record StringTypeDefinition: ITypeDefinition { public static StringTypeDefinition Instance = new();}
public record CollectionTypeDefinition(Type Type, ITypeDefinition ArgumentTypeDefinition) : ITypeDefinition;
public record DictionaryTypeDefinition(Type Type, ObjectTypeDefinition KeyValueDefinition): ITypeDefinition;
public record NullableTypeDefinition(ITypeDefinition ValueTypeDefinitiion) : ITypeDefinition;
public record ConvertedTypeDefinition(ITypeDefinition ConvertedValueTypeDefiniton): ITypeDefinition;

public record 
InterfaceTypeDefinition(
    Type Type, 
    ImmutableDictionary<int, Type> SubTypes) : ITypeDefinition {

    public bool 
    TryGetSubTypeDefinition(int tag, out ObjectTypeDefinition result) {
        if (SubTypes.TryGetValue(tag, out var resultType)) {
            result = resultType.GetTypeDefinition().VerifyType<ObjectTypeDefinition>();
            return true;
        }
        result = default;
        return false;
    }
}
