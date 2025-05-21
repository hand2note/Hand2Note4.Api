namespace Hand2Note4.Api;
internal static class Objects {

    public static T 
    VerifyType<T>(this object? @object) {
        if (@object is null)
            return default(T);
        if (@object is not T result)
            throw new InvalidOperationException($"Expected object of type {typeof(T).Name} but was {@object.GetType().Name}");
        return result;
    }

    public static T 
    VerifyType<T>(this T? @object) {
        if (@object is null)
            return default(T);
        if (@object is not T result)
            throw new InvalidOperationException($"Expected object of type {typeof(T).Name} but was {@object.GetType().Name}");
        return result;
    }
}
