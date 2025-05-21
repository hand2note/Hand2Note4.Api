using System.Diagnostics;
namespace Hand2Note4.Api;

public static class 
TypeDefinitionFunctions {

    public static Dictionary<Type, ITypeDefinition> TypeDefinitions = new();

    public static ITypeDefinition
    GetTypeDefinition(this Type type) {
        if (TypeDefinitions.TryGetValue(type, out var result))
            return result;
        lock(TypeDefinitions)
            TypeDefinitions[type] = result = type.GetTypeDefinitionSlow();
        return result;
    }

    private static ITypeDefinition
    GetTypeDefinitionSlow(this Type type) {
        if (TypeDefinitions.TryGetValue(type, out var result))
            return result;
        if (type.IsDictionaryType() || type.IsImmutableDictionaryType()) {
            var (keyType, valueType) = type.GetDictionaryGenericArguments();
            return new DictionaryTypeDefinition(
                Type: type, 
                KeyValueDefinition: new ObjectTypeDefinition(
                    Type: (keyType, valueType).GetType(),
                    Properties: ImmutableDictionary<int, ITypeDefinition>.Empty
                        .Add(1, keyType.GetTypeDefinition())
                        .Add(2, valueType.GetTypeDefinition())));
        }
        
        if (type.IsCollection())
            return new CollectionTypeDefinition(
                Type: type, 
                ArgumentTypeDefinition:  type.GetCollectionArgumentType().GetTypeDefinition());

        if (type == typeof(string))
            return StringTypeDefinition.Instance;

        if (type.IsInterface)
            return new InterfaceTypeDefinition(
                Type: type, 
                SubTypes: type.GetInterfaceSubTypes().ToImmutableDictionary(item => item.tag, item => item.type));

        if (type.TryGetConvertedTypeDefinition(out var converterTypeDefinition))
            return converterTypeDefinition;

        if (type.IsNullableType(out var nullableValueType))
            return new NullableTypeDefinition(ValueTypeDefinitiion: nullableValueType.GetTypeDefinition());

        if (type.IsPrimitive || type.IsEnum || type == typeof(DateTime) || type == typeof(TimeSpan))
            return new NumericTypeDefinition(type);

        return new ObjectTypeDefinition(
            Type: type,
            Properties: type.GetSerializableProperies().ToImmutableDictionary(
                memberId => memberId.tag,
                memberId => memberId.memberInfo.GetPropertyType().GetTypeDefinition()));
    }

    public static bool 
    TryGetConvertedTypeDefinition(this Type type, out ConvertedTypeDefinition result) {
        if (type.TryGetSerializationConverter(out var converterFunction)) {
            var convertedValueType = converterFunction.ReturnType;
            result = new ConvertedTypeDefinition(ConvertedValueTypeDefiniton: convertedValueType.GetTypeDefinition());
            return true;
        }
        result = default;
        return false;
    }

    public static IEnumerable<(int tag, ObjectTypeDefinition definition)> 
    GetInterfaceSubTypeDefinitions(this Type type) =>
        type.GetInterfaceSubTypes().Select(tagType => (tagType.tag, tagType.type.GetTypeDefinition().VerifyType<ObjectTypeDefinition>()));
            
    public static IEnumerable<(int tag, Type type)> 
    GetInterfaceSubTypes(this Type type) =>
        type.GetCustomAttributes<TypeTagAttribute>()
            .DistinctBy(attribute => attribute.Tag)
            .Select(attribute => (attribute.Tag, attribute.Type));

    public static string
    GetNameWithGenerics(this Type type) {
        if (!type.IsGenericType) return type.Name;
        var genericTypeName = type.GetGenericTypeDefinition().Name;
        genericTypeName = genericTypeName[..genericTypeName.IndexOf('`')];
        var genericArgs = type.GetGenericArguments().Select(GetNameWithGenerics).Join(separator: ", ");
        return $"{genericTypeName}<{genericArgs}>";
    }
}