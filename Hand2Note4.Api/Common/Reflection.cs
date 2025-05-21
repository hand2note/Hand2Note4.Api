using System.Reflection;
using System.Runtime.CompilerServices;

namespace Hand2Note4.Api;
internal static class Reflection {

     public static Dictionary<Type, List<(int tag, MemberInfo memberInfO)>> 
    SerializableProperties = new();

    public static Dictionary<Type, Type>
    CollectionTypeArguments = new();

    public static List<(int tag, MemberInfo memberInfo)> 
    GetSerializableProperies(this Type type) {
        if (SerializableProperties.TryGetValue(type, out var result))
            return result;

        lock(SerializableProperties) 
            return SerializableProperties[type] = type.GetSerializableProperiesSlow();
    }

    public static List<(int tag, MemberInfo memberInfo)> 
    GetSerializableProperiesSlow(this Type type) {
        var result = new List<(int tag, MemberInfo memberInfo)>();
        foreach(var property in GetPropertyMembers(type)) 
            if (property.TryGetPropertyTag(out var tag))
                 result.Add((tag, property));

        // We serialize all properties if no tag is specified. This is done for IPC support. 
        if (result.Count == 0)
            result = type.GetProperties().Where(property => property.CanWrite && !property.IsIndexer()).WithIndex().MapToList(indexProperty => (tag: indexProperty.index + 1,  memberInfo: (MemberInfo)indexProperty.value));
        
        result.Select(item => item.tag).VerifyDistinct(message: $"{type.Name} contains duplicate tag properties");
        return result;
    }

    public static bool
    IsIndexer(this PropertyInfo propertyInfo) => propertyInfo.GetIndexParameters().Length > 0;

    public static IEnumerable<MemberInfo>
    GetPropertyMembers(this Type type) {
        foreach(var property in type.GetProperties().Concat(type.GetProperties(BindingFlags.NonPublic)))
            yield return property;

        foreach (var field in type.GetFields().Concat(type.GetFields(BindingFlags.NonPublic)))
            yield return field;
    }

    public static object?
    GetPropertyValue(this MemberInfo member, object @object) {
        if (member is PropertyInfo propertyInfo)
            return propertyInfo.GetValue(@object);
        if (member is FieldInfo fieldInfo)
            return fieldInfo.GetValue(@object);
        throw new NotSupportedException();
    }

    public static bool 
    HasGenericArguments(this Type type) => type.GetGenericArguments().Length != 0;

    public static Type 
    GetSingleGenericArgument(this Type type) => 
        type.TryGetSingleGenericArgument(out var result) ? result : throw new InvalidOperationException($"Failed to get single generic argument of type {type.Name}");

    public static bool 
    TryGetSingleGenericArgument(this Type type, out Type result) {
        var genericArguments = type.GetGenericArguments();
        if (genericArguments == null || genericArguments.Length != 1) {
            result = default;
            return false;
        }
        result = genericArguments[0];
        return true;
    }

    public static (Type keyType, Type valueType)
    GetDictionaryGenericArguments(this Type type) {
        var genericArguments = type.GetGenericArguments();
        if (genericArguments == null || genericArguments.Length != 2) {
            throw new InvalidOperationException($"Failed to {nameof(GetDictionaryGenericArguments)} of type {type.Name.Quoted()}");
        }
        return (keyType: genericArguments[0], valueType: genericArguments[1]);
    }

    public static object
    CreateListOfType(this IList<object?> values, Type listType) {
        if (listType.IsArray) 
            return listType.GetCollectionArgumentType().CreateArray(elements: values);

        if (listType.IsImmutableListType()) 
            return CreateImmutableCollection(values, listType);

        if (listType.IsImmutableHashSetType()) 
            return 
                CreateImmutableCollection(values, listType);

        if (listType.IsImmutableArrayType()) 
            return 
                CreateImmutableCollection(values, listType);

        if (listType.IsListType()) {
            var resultType = typeof(List<>).MakeGenericType(listType.GetCollectionArgumentType());
            var result = (IList)Activator.CreateInstance(resultType);
            foreach(var value in values)
                result.Add(value);
            return result;
        }

        if (listType.IsSortedSetType()) 
            return values.GetGenericCollectionOfType(typeof(SortedSet<>), argumentType: listType.GetCollectionArgumentType());

        if (listType.IsHashSetType())
            return values.GetGenericCollectionOfType(typeof(HashSet<>), argumentType: listType.GetCollectionArgumentType());

        throw new NotImplementedException($"List of type {listType.Name.Quoted()} is not implemented");

        object 
        CreateImmutableCollection(IEnumerable<object> values, Type collectionType) {
            var result = collectionType.GetField("Empty")!.GetValue(null);
            var add = collectionType.GetMethod("Add")!;
            foreach (var value in values)
                result = add.Invoke(result, new object[] { value });
            return result!;
        }
    }

    public static object
    GetGenericCollectionOfType(this IEnumerable<object> values, Type collectionType, Type argumentType) {
        var resultType = collectionType.MakeGenericType(argumentType);
        var result = Activator.CreateInstance(resultType);
        var addMethod = resultType.GetMethods().First(method => method.Name == "Add");
        foreach(var value in values)
            addMethod.Invoke(result, new object[] { value });
        return result;
    }
    
    public static object 
    GetDictionaryOfType(this IEnumerable<(object key, object value)> keyValues, Type dictionaryType) {
        if (dictionaryType.IsImmutableDictionaryType())
            return keyValues.CreateImmutableDictionaryOfType(dictionaryType);
        var dictionary = (IDictionary)dictionaryType.CreateEmptyDictionary();
        foreach(var (key, value) in keyValues) {
            dictionary.Add(key, value);
        }
        return dictionary;
    }

    public static object 
    CreateImmutableDictionaryOfType(this IEnumerable<(object key, object value)> values, Type dictionaryType) {
        if (!dictionaryType.IsImmutableDictionaryType())
            throw new InvalidOperationException($"Expecting an immutable dictionary type but was {dictionaryType.Name}");

        var (keyType, valueType) = dictionaryType.GetDictionaryGenericArguments();
        var resultType = typeof(ImmutableDictionary<,>).MakeGenericType(keyType, valueType);
        var result = resultType.GetField("Empty")?.GetValue(null) ?? throw new InvalidOperationException($"type {resultType.Name} doesn't have field 'Empty'");

        var add = resultType.GetMethod("Add");
       
        foreach(var (key, value) in values)
            result = add?.Invoke(result, new[] {key, value});

        if (result == null)
            throw new InvalidOperationException($"Can't create dictionary of type {dictionaryType}");

        return result;
    }

    public static object
    ToEnumValue(this int value, Type enumType) => Enum.ToObject(enumType, value);
    
    public static object 
    CreateEmptyImmutableCollection(this Type collectionType, Type argumentType) {
        var resultType = collectionType.MakeGenericType(argumentType);
        return resultType.GetField("Empty").GetValue(null)!;
    }

    public static object 
    CreateEmptyCollection(this Type collectionType, Type argumentType) {
        var resultType = collectionType.MakeGenericType(argumentType);
        return Activator.CreateInstance(resultType)!;
    }

    public static object 
    CreateArray(this Type elementType, IList<object> elements) {
        var array = Array.CreateInstance(elementType, elements.Count);
        for (int i = 0; i < elements.Count; i++) 
            array.SetValue(Convert.ChangeType(elements[i], elementType), i);
        return array;
    }

    public static object
    CreateArray(this Type elementType, int size) =>
        Array.CreateInstance(elementType, size);

    public static object 
    CreateEmptyDictionary(this Type dictionaryType) {
        var (keyType, valueType) = dictionaryType.GetDictionaryGenericArguments();
        var resultType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
        return Activator.CreateInstance(resultType)!;
    }

    public static object 
    CreateEmptyImmutableDictionary(this Type dictionaryType) {
        var (keyType, valueType) = dictionaryType.GetDictionaryGenericArguments();
         var resultType = typeof(ImmutableDictionary<,>).MakeGenericType(keyType, valueType);
        return resultType.GetField("Empty").GetValue(null)!;
    }

    public static bool 
    IsListType(this Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);

    public static bool 
    IsSortedSetType(this Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(SortedSet<>);

    public static bool 
    IsHashSetType(this Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(HashSet<>);

    public static bool 
    IsImmutableListType(this Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ImmutableList<>);

    public static bool 
    IsImmutableHashSetType(this Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ImmutableHashSet<>);
    
    public static bool 
    IsImmutableArrayType(this Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ImmutableArray<>);
    
    public static bool 
    IsDictionaryType(this Type type) => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
    
    public static bool 
    IsImmutableDictionaryType(this Type type) => type.IsGenericType &&  type.GetGenericTypeDefinition() == typeof(ImmutableDictionary<,>);

    public static Type 
    GetPropertyType(this MemberInfo member) {
        if (member is PropertyInfo propertyInfo) return propertyInfo.PropertyType;
        if (member is FieldInfo fieldInfo) return fieldInfo.FieldType;
        throw new InvalidOperationException($"member {member} is not a property");
    }

    public static bool 
    TryGetAttribute<T>(this MemberInfo member, out T result) where T: Attribute{
        var attribute = member.GetCustomAttribute<T>();
        result = attribute;
        return attribute != null;
    }

    public static bool 
    TryGetAttribute<T>(this object @object, out T result)
    where T: Attribute {
        var attributes = @object.GetCustomAttributes<T>();
        if (attributes.Length == 0){
            result = default;
            return false;
        }
        result = (T)attributes[0];
        return true;
    }

    public static bool 
    ContainsAttribute<T>(this object @object) where T: Attribute  => @object.GetCustomAttributes<T>().Length > 0;

    private static object[]
    GetCustomAttributes<T>(this object @object) => 
        @object is PropertyInfo propertyInfo 
            ? propertyInfo.GetCustomAttributes(typeof(T), true)
            : @object is ParameterInfo parameterInfo 
                ? parameterInfo.GetCustomAttributes(typeof(T), true)
                : @object.GetType().GetCustomAttributes(typeof(T), true);
    
    internal static object
    GetDefaultValue(this ParameterInfo parameter) =>
        parameter.ParameterType.GetDefaultValue();
        

    internal static Dictionary<Type, object> DefaultValues {get;} = new();

    internal static object 
    GetDefaultValue(this Type type) {
        if (DefaultValues.TryGetValue(type, out var result))
            return result;
        lock(DefaultValues)
            return DefaultValues[type] = type.GetDefaultValueSlow();
    }

    internal static object 
    GetDefaultValueSlow(this Type type) {
        if (type == typeof(string)) 
            return null;

        if (type == typeof(bool)) 
            return false;

        if (type == typeof(double)) 
            return 0d;

        if (type == typeof(int))
            return 0;
                
        if (type == typeof(long)) 
            return 0L;

        if (type == typeof(decimal)) 
            return 0.0;

        if (type.IsEnum) 
            return 0.ToEnumValue(enumType: type);
        
        if (type.IsImmutableListType()) 
            return typeof(ImmutableList<>).CreateEmptyImmutableCollection(argumentType: type.GetSingleGenericArgument());

        if (type.IsListType()) 
            return typeof(List<>).CreateEmptyCollection(argumentType: type.GetSingleGenericArgument());

        if (type.IsSortedSetType())
            return typeof(SortedSet<>).CreateEmptyCollection(argumentType: type.GetSingleGenericArgument());

        if (type.IsImmutableDictionaryType()) 
            return type.CreateEmptyImmutableDictionary();

        if (type.IsImmutableArrayType()) 
            return typeof(ImmutableArray<>).CreateEmptyImmutableCollection(argumentType: type.GetSingleGenericArgument());

        if (type.IsDictionaryType()) 
            return type.CreateEmptyDictionary();

        if (type.IsHashSetType())
            return typeof(HashSet<>).CreateEmptyCollection(argumentType: type.GetSingleGenericArgument());

        if (type.IsImmutableHashSetType())
            return typeof(ImmutableHashSet<>).CreateEmptyImmutableCollection(argumentType: type.GetSingleGenericArgument());

        if (type.IsArray)
            return type.GetElementType().CreateArray(size: 0);

        if (type == typeof(DateTime))
            return DateTime.MinValue;

        if (type == typeof(TimeSpan))
            return TimeSpan.Zero;
        
        if (type == typeof(Guid))
            return Guid.Empty;

        if (type.TryGetEmptyConstructor(out var emptyConstructor))
            return emptyConstructor.Invoke([]);
        return null;
    }

    public static bool
    TryGetEmptyConstructor(this Type type, out ConstructorInfo result) => 
        type.GetConstructors().TryGet(constructor => constructor.GetParameters().Length == 0, out result);

    public static bool 
    TryGetMainConstructor(this Type type, out ConstructorInfo result) {
        result = default;
        var constructors = type.GetConstructors().Where(constructor => constructor.GetParameters().Length > 0).ToList();
        if (constructors.Count == 0) return false;

        result = constructors.WithMaxValue(constructor => constructor.GetParameters().Length).First();
        return true;
    }

    public static bool 
    IsCollection(this Type type) {
        if (type == typeof(string))
            return false;
        if (type.IsDictionaryType() || type.IsImmutableDictionaryType())
            return false;
        if (typeof(ICollection).IsAssignableFrom(type)) 
            return true;
        foreach(var interfaceType in type.GetInterfaces())
            if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(ICollection<>))
                return true;
        return false;
    }
        
    public static bool
    IsStruct(this Type type) =>
        type is { IsValueType: true, IsPrimitive: false };
        
    public static bool 
    IsTupleType(this Type type) {
        if (!type.IsGenericType)
            return false;
        
        if (type.GetGenericTypeDefinition() == typeof(ValueTuple<>)) 
            return true;
        
        if (type.FullName != null && type.FullName.StartsWith("System.ValueTuple`", StringComparison.Ordinal)) 
            return true;
        
        return false;
    }
    
    public static MethodInfo
    GetValueTupleConstructor(this Type[] types) {
        var methodName = types.Length switch {
            2 => nameof(GetValueTupleTwo),
            3 => nameof(GetValueTupleThree),
            _ => throw new InvalidOperationException()
        };
        var method = typeof(Reflection).GetMethod(methodName, BindingFlags.Public | BindingFlags.Static);
        if (method == null) throw new InvalidOperationException($"Method {methodName} not found");
        return method.MakeGenericMethod(types);
    }
    
    public static (T1, T2)
    GetValueTupleTwo<T1, T2>(T1 value1, T2 value2) => (value1, value2);
    
    public static (T1, T2, T3)
    GetValueTupleThree<T1, T2, T3>(T1 value1, T2 value2, T3 value3) => (value1, value2, value3);
    
    public static bool 
    IsRecordType(this Type type) {
        // Check for compiler-generated methods specific to records
        var methods = type.GetMethods();
        return methods.Any(method => method.Name.StartsWith("<Clone>$")) &&
            type.GetProperty("EqualityContract", BindingFlags.Instance | BindingFlags.NonPublic) != null;
    }

    public static bool 
    TryGetMatchingParameter(this ConstructorInfo constructor, MemberInfo member, out ParameterInfo result ) {
        foreach(var parameter in constructor.GetParameters()) {
            if (parameter.Name is null) 
                throw new InvalidOperationException("Parameters with null names aren't expected");

            if (parameter.Name.CamelToPascalCase() == member.Name.CamelToPascalCase() || 
                parameter.Name.PascalToCamelCase() == member.Name.PascalToCamelCase()) {
                result = parameter;
                return true;
            }
        }
        result = default;
        return false;
    }


    public static Type 
    GetCollectionArgumentType(this Type type) {
        if (CollectionTypeArguments.TryGetValue(type, out var result))
            return result;
        lock(CollectionTypeArguments)
            return CollectionTypeArguments[type] = type.GetCollectionArgumentTypeSlow();
    }

    public static Type 
    GetCollectionArgumentTypeSlow(this Type type) => 
        type.TryGetCollectionArgumentType(out var result) ? result : throw new InvalidOperationException($"Failed to {nameof(GetCollectionArgumentType)} from {type.Name}");

    public static bool 
    TryGetCollectionArgumentType(this Type type, out Type result) {
        if (type.IsArray) {
            result =  type.GetElementType();
            return true;
        }
        
        if (!type.HasGenericArguments())
            throw new NotImplementedException($"Non-generic collections aren't implemented yet ({type.Name})");
        return type.TryGetSingleGenericArgument(out result);
    }
    public static Dictionary<Type, bool> IsAnonymousTypeCache = new();

    public static bool 
    IsAnonymousType(this Type type) {
        if (IsAnonymousTypeCache.TryGetValue(type, out var result))
            return result;
        lock(IsAnonymousTypeCache)
            return IsAnonymousTypeCache[type] = type.IsAnonymousTypeSlow();
    }

    public static bool 
    IsAnonymousTypeSlow(this Type type) {
        var hasCompilerGeneratedAttribute = type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Length > 0;
        var nameContainsAnonymousType = type.FullName.Contains("AnonymousType");
        return hasCompilerGeneratedAttribute && nameContainsAnonymousType;
    }

    public static bool
    ContructorParameterMatchesPropertyName(this string parameterName, string propertyName, Type declaringType) =>
        parameterName.CamelToPascalCase().Equals(propertyName.CamelToPascalCase()) ||
        parameterName.PascalToCamelCase().Equals(propertyName.PascalToCamelCase()) ||
        //in tuples constuctor parametres are named value1, value2 but properties are named as Item1, Item2,
        declaringType.IsTupleType() && (parameterName.Replace("value", "Item").Equals(propertyName));

    public static IEnumerable<MethodInfo> 
    GetAllStaticFunctions(this Assembly assembly) {
        return assembly.GetTypes()
            .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic))
            .Where(method => method.IsStatic);
    }

    public static bool
    ContainsAttribute<T>(this MethodInfo method) where T : Attribute => 
        method.GetCustomAttributes(typeof(T), false).Length > 0;

    public static bool
    IsGenericInstanceOf(this Type type, Type genericType) => 
        type.IsGenericType && type.Name == genericType.Name;

    public static bool
    HasAttribute<T>(this MemberInfo member) where T : Attribute => 
        member.CustomAttributes.Any(attributeData => attributeData.AttributeType == typeof(T));

    public static bool 
    IsNullableType(this Type type) => 
        type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);

    public static bool 
    IsNullableType(this Type type, out Type nullableValueType) {
        if (type.IsNullableType()) {
            nullableValueType = type.GetGenericArguments()[0];
            return true;
        }
        nullableValueType = null;
        return false;
    }

    public static void 
    SetPropertyValue(this MemberInfo member, object @object, object? value) {
        if (member is PropertyInfo property) {
            var setMethod = property.GetSetMethod(true);
            if (setMethod == null) 
                if (property.TryGetBackingField(out var backingField))
                    backingField.SetValue(@object, value);
                else 
                     throw new InvalidOperationException($"Property {@object.GetType()}.{property.Name} has neigther a setter nor a backing field");
            else
                setMethod.Invoke(@object, new[] {value});
        }
        else 
            member.VerifyType<FieldInfo>().SetValue(@object, value);
    }

    public static bool 
    TryGetBackingField(this PropertyInfo propertyInfo, out FieldInfo result) {
        result = propertyInfo.DeclaringType.GetField($"<{propertyInfo.Name}>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);    
        return result != null;
    }


    public static IEnumerable<ConstructorInfo>
    GetConstructorsIncludingProtected(this Type type) {
        var constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        foreach(var constructor in constructors ) {
            if (!constructor.IsRecordOriginalConstructor())
                yield return constructor;;
        }
    }

    public static bool 
    IsRecordOriginalConstructor(this ConstructorInfo constructor) {
        if (constructor.DeclaringType.IsRecordType()) {
            var parameters = constructor.GetParameters();
            if (parameters.Length == 1 && parameters[0].ParameterType == constructor.DeclaringType)
                return true;
        }
        return false;
    }

    public static bool
    IsSystemType(this Type type) => type.Namespace?.StartsWith("System") ?? false;

    public static bool
    HasAttribute<T>(this Type type) where T : Attribute => 
        Attribute.IsDefined(type, typeof(T));
        
    public static FieldInfo
    GetPrivateFieldOrThrow(this Type type, string fieldName) =>
        type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new InvalidOperationException($"{typeof(MemoryStream)} doesn't have field {fieldName}");
    
    public static bool
    IsInlineArray(this Type type) => 
        type.HasAttribute<InlineArrayAttribute>();
    
    public static string
    GetNameWithGenerics(this Type type) {
        if (!type.IsGenericType) return type.Name;
        var genericTypeName = type.GetGenericTypeDefinition().Name;
        genericTypeName = genericTypeName[..genericTypeName.IndexOf('`')];
        var genericArgs = type.GetGenericArguments().Select(GetNameWithGenerics).AggregateToString(separator: ", ");
        return $"{genericTypeName}<{genericArgs}>";
    }

    public static ConstructorInfo
    GetParameterlessConstructor(this Type type) =>
        type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .FirstOrDefault(constructor => constructor.GetParameters().Length == 0)
        ?? throw new InvalidOperationException($"Failed to get constructor of type {type.Name.Quoted()}");

    public static bool
    TryGetAttribute<T>(this Type objectType, out T result) where T : Attribute {
        var customAttribute = objectType.GetCustomAttributes(typeof(T), true);
        if (customAttribute.Length == 0) {
            result = default;
            return false;
        }

        result = (T)customAttribute[0];
        return true;
    }
}
