namespace Hand2Note4.Api;
public static class SerializableValueFunctions{
    
    public static Dictionary<Type, MethodBase?> Constructors {get;} = new();

    public static ISerializableValue
    GetSerializableValue<T>(this T value) => value.GetSerializableValue(declaredType: typeof(T));

    public static ISerializableValue 
    GetSerializableValue(this object value, Type declaredType) {
        #if DEBUG 
        //too slow even for debugging
        //value.VerifyBinarySerializable();
        #endif
        if (value.TryGetConvertedSerializableValue(out var result)) return result;
        if (declaredType.IsNullableType(out var nullableValueType))
            if (value != null)
                return value.GetSerializableValue(nullableValueType);

        if (value.TryGetPrimitiveSerializableValue(out result)) return result;
        if (value.TryGetDictionarySerializableValue(out var dictionaryResult)) return dictionaryResult;
        if (value.TryGetCollectionSerializableValue<ISerializableValue>(out var collectionResult)) return collectionResult;

        return new ObjectSerializableValue(
            TypeTag: value.GetTypeTag(declaredType),
            Properties: value.GetObjectMapProperties().ToDictionary());
    }

    public static IEnumerable<(int tag, ISerializableValue value)>
    GetObjectMapProperties(this object @object) {
        foreach(var (tag, childMember) in @object.GetType().GetSerializableProperies()) {
            if (!childMember.ShouldSerialize(memberParentValue: @object))
                continue;

            var value = childMember.GetPropertyValue(@object).GetSerializableValue(declaredType: childMember.GetPropertyType()); 
            yield return (tag, value);
        }
    }

    public static bool 
    TryGetConvertedSerializableValue(this object @object, out ISerializableValue result) {
        if (@object.GetType().TryGetSerializationConverter(out var converter)) {
            result = converter.Invoke(null,new []{@object }).GetSerializableValue(declaredType: converter.ReturnType);
            return true;
        }

        result = default;
        return false;
    }

    public static bool 
    TryGetPrimitiveSerializableValue(this object @object, out ISerializableValue result) {
        result = @object switch { 
            bool @bool => new LongSerializableValue(Value: @bool ? 1 : 0),
            byte @byte => new  LongSerializableValue(Value: @byte),
            ushort @ushort => new LongSerializableValue(Value: @ushort),
            short @short => new LongSerializableValue(Value: @short),
            int @int => new LongSerializableValue(Value: @int),
            uint @uint => new LongSerializableValue(Value: @uint),
            double @double => new DoubleSerializableValue(Value: @double),
            string @string => new StringSerializableValue(Value: @string),
            long @long => new LongSerializableValue(Value: @long), 
            ulong @long => new ULongSerializableValue(Value: @long),
            float @float => new FloatSerializableValue(Value: @float),
            DateTime dateTime => new LongSerializableValue(Value: dateTime.Ticks),
            TimeSpan timeSpan => new LongSerializableValue(Value: timeSpan.Ticks),
            _ => @object.GetType().IsEnum ? new LongSerializableValue(@object.ConvertEnumToLong()) : null
        };
        return result != null;
    }
    
    public static bool 
    TryGetDictionarySerializableValue(this object @object, out DictionarySerializableValue result) {
        result = default;
        if (@object is not IDictionary dictionary)
            return false;

        var (keyType, valueType) = @object.GetType().GetDictionaryGenericArguments();
        if (keyType.IsCollection())
            throw new NotImplementedException($"Collection key is not supported in type {@object.GetType().Name}");
        /*if (valueType.IsCollection())
            throw new NotImplementedException($"Collection value is not supported in type {@object.GetType().Name}");*/

        result = new DictionarySerializableValue(
            KeyValues: dictionary.Keys.MapToImmutableList(key => (key, dictionary[key]).GetSerializableValue(keyType, valueType)));
        return true;
    }

    public static ObjectSerializableValue
    GetSerializableValue(this (object Key, object Value) keyValuePair, Type keyType, Type valueType) =>
        new ObjectSerializableValue(
            TypeTag: 0,
            Properties: new Dictionary<int, ISerializableValue>() {
                {1, keyValuePair.Key.GetSerializableValue(keyType) },
                {2, keyValuePair.Value.GetSerializableValue(valueType)}
        });

    public static bool 
    TryGetCollectionSerializableValue<T>(this object @object, out CollectionSerializableValue result){
        result = default;
        if (@object is not IEnumerable enumerable || @object.ContainsAttribute<BinarySerializableAttribute>() ||
            !@object.GetType().TryGetCollectionArgumentType(out var argumentType)) 
            return false;

        #if DEBUG
        if (argumentType.IsCollection())
            /*
               this is because we can't write  [[1,2], [3,4]] as
               [tag=1, wireType=VarInt] 1,
               [tag=1, wireType=VarInt] 2,
               [tag=1, wireType=VarInt] 3,
               [tag=1, wireType=VarInt] 4

               Protobuf doesn't support it as well
             */
            throw new NotSupportedException($"Jagged or nested collection aren't supported ({@object.GetType().Name})");
        #endif
        
        // We write struct default values in generated code due to performance considerations, so here we should write too
        var writeDefaultValues = ShouldWriteDefaultValue();
        result = new CollectionSerializableValue(Values: 
            enumerable.MapToImmutableList(item => !writeDefaultValues && item.IsDefaultValue(type: argumentType) 
                ? DefaultSerializableValue.Instance
                : item.GetSerializableValue(declaredType: argumentType)));
        return true;

        bool
        ShouldWriteDefaultValue() {
            if (argumentType.IsTupleType())
                return true;

            if (argumentType.IsEnum)
                return false;
            
            if (argumentType.IsStruct())
                return true;

            return false;
        }
    }

    public static bool 
    ShouldSerialize(this MemberInfo member, object memberParentValue) {
        var memberType = member.GetPropertyType();
        // In Release inline arrays IsDefaultValue doesn't work
        if (memberType.IsInlineArray())
            return true;
        var propertyValue = member.GetPropertyValue(memberParentValue);
        return !propertyValue.IsDefaultValue(type: memberType);
    }

    //Type can be Nullable<int> but memberValue can be 0.
    public static bool 
    IsDefaultValue(this object? memberValue, Type type) {
        if (type.IsNullableType()) return memberValue == null;
        if (memberValue == default) return true;
        if (memberValue is bool @bool) return @bool == false;
        if (memberValue is string @string && @string == null) return true;
        if (memberValue is ICollection collection && collection.Count == 0) return true;
        if (memberValue is int @int && @int == 0) return true;
        if (memberValue is long @long && @long == 0) return true;
        if (memberValue is decimal @decimal && @decimal == 0) return true;
        if (memberValue is double @double) return @double == 0;
        if (memberValue is DateTime dateTime && dateTime == default) return true;
        if (memberValue is TimeSpan timeSpan && timeSpan == default) return true;
        if (memberValue.GetType().IsEnum && memberValue.IsDefaultEnumValue()) return true;
        if (memberValue is Guid guid) return Equals(guid, Guid.Empty);
        if (type.IsStruct() && memberValue.Equals(Activator.CreateInstance(type))) return true;
        return false;
    }

    public static bool 
    IsDefaultEnumValue(this object value) =>
        (Convert.ToInt64(value)) == 0;

    public static StringSerializableValue ToStringValue(this string @string) => new StringSerializableValue(Value: @string);
    public static DoubleSerializableValue ToDoubleValue(this double value) => new DoubleSerializableValue(value);

    public static long
    ConvertEnumToLong(this object value) =>
        Convert.ToInt64(value);

    public static bool 
    TryGetPropertyTag(this MemberInfo property, out int result) {
        if (property.TryGetAttribute<TagAttribute>(out var tagAttribute)) {
            result = tagAttribute.Value;
            return true;
        }

        result = default;
        var declaringType = property.DeclaringType;
        if (declaringType == null)
            return false;

        if (declaringType.IsTupleType() || declaringType.IsAnonymousType()) {
            //tags for tuples are (1, 2, ..)
            result = declaringType.GetPropertyMembers().ToList().IndexOf(property) + 1;
            return true;
        }

        // When using the "braceless" record syntax, attributes placed on members are actually parameter attributes
        if (declaringType.IsRecordType() && declaringType.TryGetMainConstructor(out var constructor)) {
            
            if (!constructor.TryGetMatchingParameter(property, out var constructorParameter))
                return false;

            if (!constructorParameter.TryGetAttribute<TagAttribute>(out tagAttribute))
                return false;
            result = tagAttribute.Value;
            return true;
        }
        return false;
    } 

    internal static object? 
    GetObjectOfType(this ISerializableValue value, Type expectedType) {
        if (value is DefaultSerializableValue)
            return expectedType.IsValueType ? Activator.CreateInstance(expectedType) : null;
        if (expectedType.TryGetDeserializationConverter(out var converter))
            return converter.Invoke(null, new object[]{ value.GetObjectOfType(expectedType: converter.GetParameters()[0].ParameterType)});
        if (expectedType.IsNullableType(out var nullableValueType)) {
            var nullableValue = value.GetObjectOfType(nullableValueType);
            return Activator.CreateInstance(expectedType, nullableValue);
        }
        switch(value) {
            case StringSerializableValue stringValue: 
                if (expectedType == typeof(string) || expectedType == typeof(object))
                    return stringValue.Value;
                break;

            case LongSerializableValue longMapValue:
                if (expectedType == typeof(int) || expectedType == typeof(object)) 
                    return (int)longMapValue.Value;

                if (expectedType == typeof(uint) || expectedType == typeof(object)) 
                    return (uint)longMapValue.Value;
                
                if (expectedType == typeof(long)) 
                    return longMapValue.Value;

                if (expectedType == typeof(bool)) 
                    return longMapValue.Value != 0;

                if (expectedType == typeof(double)) 
                    return (double)longMapValue.Value;

                if (expectedType == typeof(ushort)) 
                    return (ushort)longMapValue.Value;

                if (expectedType == typeof(short)) 
                    return (short)longMapValue.Value;

                if (expectedType == typeof(byte)) 
                    return (byte)longMapValue.Value;

                if (expectedType.IsEnum) 
                    return Enum.ToObject(enumType: expectedType, longMapValue.Value);

                if (expectedType == typeof(DateTime)) 
                    return new DateTime(ticks: longMapValue.Value);

                if (expectedType == typeof(TimeSpan)) 
                    return new TimeSpan(ticks: longMapValue.Value);

                break;
            
            case ULongSerializableValue ulongMapValue:
                if (expectedType == typeof(ulong)) 
                    return (ulong)ulongMapValue.Value;
                break;
                
            case DoubleSerializableValue doubleValue:
                if (expectedType == typeof(double) || expectedType == typeof(object)) 
                    return doubleValue.Value;

                if (expectedType == typeof(decimal) || expectedType == typeof(object)) 
                    return (decimal)doubleValue.Value;

                if (expectedType == typeof(float) || expectedType == typeof(object)) 
                    return (float)doubleValue.Value;
                
                break;

            case ObjectSerializableValue serializableObject:
                return serializableObject.GetObjectOfType(expectedType);

            case CollectionSerializableValue collectionValue:
                return collectionValue.GetCollectionOfType(expectedType);
   
            case DictionarySerializableValue dictionaryValue:
                return dictionaryValue.GetDictionaryOfType(expectedType);
        }
        return false;
    }

    public static object?
    GetObjectOfType(this ObjectSerializableValue serializableObject, Type expectedType) {
        if (expectedType.IsTupleType())
            return serializableObject.GetTupleOfType(expectedType);

        if (expectedType.IsAnonymousType())
            return serializableObject.GetAnonymousObject(expectedType);

        if (expectedType.IsInterface)
            expectedType = expectedType.GetInterfaceSubTypes().TryGet(tagType => tagType.tag == serializableObject.TypeTag, out var expectedSubType) 
                ? expectedSubType.type
                : throw new InvalidOperationException();

        //setting by a constructor
        if (expectedType.TryGetBinaryDeserializationConstructor(out var constructor)) {
            var arguments = constructor
                .GetParameters()
                .GetMethodArguments(declaringType: expectedType, serializableObject).ToArray();

            return constructor.InvokeBinaryDeserializationConstructor(arguments);
        }

        //setting properties one by one
        var properties = serializableObject.Properties;
        if (!expectedType.GetConstructorsIncludingProtected().TryGet(constructor => constructor.GetParameters().Length == 0, out var emptyConstructor))
            throw new InvalidOperationException($"Empty constructor for type {expectedType} not found");
        var result = emptyConstructor.Invoke(null);
        foreach(var (tag, property) in expectedType.GetSerializableProperies()) 
            if (properties.TryGetValue(tag, out var value))
                property.SetPropertyValue(result, value.GetObjectOfType(expectedType: property.GetPropertyType()));
            else 
                property.SetPropertyValue(result, property.GetPropertyType().GetDefaultValue());
        return result;
    }

    public static object
    GetDictionaryOfType(this DictionarySerializableValue dictionaryValue, Type expectedType) {
        var (keyType, valueType) = expectedType.GetDictionaryGenericArguments();
        return dictionaryValue.KeyValues.Select(keyValue => 
            (key: keyValue.Properties[1].GetObjectOfType(keyType),
             value: keyValue.Properties[2].GetObjectOfType(valueType)))
            .GetDictionaryOfType(expectedType);
    }
  
    public static object
    GetCollectionOfType(this CollectionSerializableValue collectionMap, Type expectedType) {
        var argumentType = expectedType.GetCollectionArgumentType();
        return collectionMap.Values.MapToList(itemValue => itemValue.GetObjectOfType(argumentType))
            .CreateListOfType(listType: expectedType);
    }
    
    public static object
    GetTupleOfType(this ObjectSerializableValue serializableObject, Type expectedType) {
        var tupleTypes = expectedType.GetFields().Select(field => field.FieldType).ToArray();
        var method = tupleTypes.GetValueTupleConstructor();
        var arguments = method.GetParameters().GetMethodArguments(expectedType, serializableObject).ToArray();
        return method.Invoke(null, arguments) ?? throw new InvalidOperationException($"Can't create an object from {expectedType.Name}");
    }

    public static object 
    GetAnonymousObject(this ObjectSerializableValue serializableObject, Type expectedType) {
        if (!expectedType.IsAnonymousType())
            throw new ArgumentException($"{expectedType} is not an anonymous type");
        var constructor = expectedType.GetConstructors().First(); 
        var arguments = constructor.GetParameters().GetMethodArguments(expectedType, serializableObject).ToArray();
        return constructor.Invoke(arguments);
    }

    public static IEnumerable<object?>
    GetMethodArguments(this ParameterInfo[] methodParameters, Type declaringType, ObjectSerializableValue serializableObject) {
        var declaredProperties = declaringType.GetSerializableProperies();
        foreach(var parameter in methodParameters){
            if (parameter.Name == null)
                throw new InvalidOperationException("Null parameter names aren't expected");
            else if (declaredProperties.TryGet(property => parameter.Name
                 .ContructorParameterMatchesPropertyName(propertyName: property.memberInfo.Name, declaringType), out var matchingProperty) && 
                serializableObject.TryGetPropertyValue(tag: matchingProperty.tag, out var value))
                yield return value.GetObjectOfType(parameter.ParameterType);
            else if (parameter.IsOptional) 
                yield return null;
            else
                yield return parameter.ParameterType.GetDefaultValue();
        }
    }

    public static int
    GetTypeTag(this object value, Type declaredType) {
        var type = value.GetType();
        if (type == declaredType)
            return 0;
        if (!declaredType.GetCustomAttributes<TypeTagAttribute>().TryGet(attribute => attribute.Type == type, out var tagAttribute) || tagAttribute == null)
            throw new InvalidOperationException($"{value.GetType()} can not be deserialized as {declaredType.Name} without {nameof(TypeTagAttribute)}");
        return tagAttribute.Tag;
    }

    public static CollectionSerializableValue
    ToCollectionSerializableValue(this IEnumerable<ISerializableValue> values) => new CollectionSerializableValue(values.ToImmutableList());

    public static DictionarySerializableValue
    ToDictionarySerializableValue(this IEnumerable<ObjectSerializableValue> keyValues) => new DictionarySerializableValue(keyValues.ToImmutableList());

    //null if no converter declared
    public static Dictionary<Type, MethodInfo?> DeserializationConverters {get;} =  new();
    public static Dictionary<Type, MethodInfo?> SerializationConverters {get;} = new();

    public static bool 
    TryGetSerializationConverter(this Type type, out MethodInfo result) {
        if (SerializationConverters.TryGetValue(type, out result))
            return result != null;
        foreach(var function in type.GetAssemblies().SelectMany(assembly => assembly.GetAllStaticFunctions()))
            if (function.ContainsAttribute<BinarySerializationConverterAttribute>() && 
                function.GetParameters().Length == 1) {
                var parameter = function.GetParameters()[0];
                if (parameter.ParameterType == type) {
                    result = function;
                    break;
                }

                if (type.IsGenericInstanceOf(parameter.ParameterType)) {
                    result = function.MakeGenericMethod(type.GenericTypeArguments);
                    break;
                }
            }
        
        lock(SerializationConverters)
            SerializationConverters[type] = result;
        return result != null;
    }

    public static bool 
    TryGetDeserializationConverter(this Type type, out MethodInfo result) {
        if (DeserializationConverters.TryGetValue(type, out result))
            return result != null;

        foreach(var function in type.GetAssemblies().SelectMany(assembly => assembly.GetAllStaticFunctions()))
            if (function.ContainsAttribute<BinaryDeserializationConverterAttribute>())
                if (function.GetParameters().Length == 1) {
                    if (function.ReturnType == type) {
                        result = function;
                        break;
                    }

                    if (type.IsGenericInstanceOf(function.ReturnType)) {
                        result = function.MakeGenericMethod(type.GenericTypeArguments);
                        break;
                    }
                }

        lock(DeserializationConverters)
            DeserializationConverters[type] = result;
        return result != null;
    }

    public static IEnumerable<Assembly>
    GetAssemblies(this Type type) {
        if (!type.Name.StartsWith("System."))
            yield return type.Assembly;
        yield return typeof(SerializableValueFunctions).Assembly;
    }

    public static bool
    IsBinarySerialiazableType(this Type type) => 
        type.CustomAttributes
            .Any(attribute => attribute.AttributeType == typeof(BinarySerializableAttribute) || attribute.AttributeType == typeof(TypeTagAttribute));

    public static bool 
    IsBinarySerializableMember(this MemberInfo member) => 
        member.CustomAttributes
            .Any(attribute => attribute.AttributeType == typeof(TagAttribute));

    public static bool
    TryGetBinaryDeserializationConstructor(this Type type, out MethodBase result) {
        if (Constructors.TryGetValue(type, out result)) 
            return result != null;
        
        lock (Constructors) {
            if (type.TryGetBinaryDeserializationConstructorSlow(out result)) {
                Constructors[type] = result;
                return true;
            }
            Constructors[type] = null;
        }
        return false;
    }
    public static bool
    TryGetBinaryDeserializationConstructorSlow(this Type type, out MethodBase result) {
        result = default;
        foreach(var method in type.GetMethods())
            if (method.IsStatic && method.HasAttribute<BinaryDeserializationConstructorAttribute>()) {
                result =  method;
                return true;
            }
        var serializableProperties = type.GetSerializableProperies().ToList();
        var constructors = type.GetConstructorsIncludingProtected().Where(constuctor => constuctor.GetParameters().Length == serializableProperties.Count).ToList();
        if (constructors.Count == 0)
            return false;

        foreach(var constructor in constructors) {
            var matches = true;
            //Verify parameters name. Take the first constructor with matching parameter names
            foreach(var parameter in constructor.GetParameters()) {
                if (!serializableProperties.Any(property => 
                    parameter.Name.ContructorParameterMatchesPropertyName(propertyName: property.memberInfo.Name, declaringType: type))) 
                {
                    if (constructors.Count == 1)
                        throw new InvalidOperationException($"Matching property for the constructor parameter {type.Name}.{parameter.Name} not found.");
                    else {
                        matches = false;
                        break;
                    }
                }
            }
            if (matches) {
                result = constructor;
                return true;
            }
        }
        return false;
    }

    public static object 
    InvokeBinaryDeserializationConstructor(this MethodBase method, params object[] arguments) {
        if (method is MethodInfo methodInfo)
            if (!methodInfo.IsStatic)
                throw new InvalidOperationException("Method must be static to be a binary deserialization constructor");
            else    
                return methodInfo.Invoke(null, arguments);
        return method.VerifyType<ConstructorInfo>().Invoke(arguments);
    }

    public static void
    VerifyBinarySerializable(this object @object) {
        var type =@object.GetType();
        if (!type.IsSystemType() &&
            !type.IsEnum &&
            !type.IsCollection() && 
            !type.IsDictionaryType() &&
            !type.HasAttribute<BinarySerializableAttribute>() && 
            !type.IsAnonymousType())
            throw new InvalidOperationException($"Type {type.Name.Quoted()} doesn't have a {nameof(BinarySerializableAttribute)}");
    }
}
