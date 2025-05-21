namespace Hand2Note4.Api;

public static class 
Numbers {
    public const double DefaultDoubleDeviation = 0.000001;
    
    public static bool
    IsGreaterZero(this double value, double deviation = DefaultDoubleDeviation) => value > deviation;
    
    public static bool
    IsLessOrEqual(this double value, double other, double deviation = DefaultDoubleDeviation) => other - value > -deviation;
    
    public static bool
    IsGreaterOrEqual(this double value, double other, double deviation = DefaultDoubleDeviation) => value - other > -deviation;
    
    public static bool
    IsLess(this double value, double other, double deviation = DefaultDoubleDeviation) => other - value > deviation;
    
    public static bool
    IsLessZero(this double value, double deviation = DefaultDoubleDeviation) => value < -deviation;
    
    public static bool
    IsZero(this double value, double deviation = DefaultDoubleDeviation) => Math.Abs(value) < deviation;
    
    public static bool
    IsNotZero(this double value, double deviation = DefaultDoubleDeviation) => !value.IsZero(deviation);
    
    public static double
    VerifyArgumentPositive(this double value, string? name = null) {
        if (value.IsLessOrEqual(0))
            throw new ArgumentOutOfRangeException($"Value must be positive but was {value}", name);
        return value;
    }
    
    public static double
    VerifyArgumentNotNegative(this double value, string? name = null) {
        if (value.IsLessZero())
            throw new ArgumentOutOfRangeException($"Value can't be negative but was {value}", name);
        return value;
    }
    
    public static bool
    IsEqual(this double value, double other, double deviation = DefaultDoubleDeviation) {
        if (value == other || double.IsNaN(value) && double.IsNaN(other))
            return true;
            
        if (Math.Abs(value -  other)  < deviation)
            return true;

        return value.IsEqualByInfinityOrNaN(other);
    }
    
    public static bool 
    IsEqualByInfinityOrNaN(this double value, double other) => 
        value.IsMaxValue() && other.IsMaxValue() || 
        value.IsMinValue() && other.IsMinValue() || 
        double.IsNaN(value) && double.IsNaN(other);
    
    public static bool
    IsMaxValue(this double value) => double.IsInfinity(value) || value == double.MaxValue;
  
    public static bool 
    IsMinValue(this double value) => double.IsNegativeInfinity(value) || value == double.MinValue;
    
    public static bool
    IsPowerOfTwo(this int x) => x != 0 && (x & x - 1) == 0;
    
    public static int
    VerifyIsPowerOfTwo(this int value, string? message = null) {
        if (!value.IsPowerOfTwo())
            throw new InvalidOperationException(message ?? $"Expecting power of two but was {value}");
        return value;
    }
    
    public static int
    VerifyNotNegative(this int value, string? name = null) =>
    value >= 0 ? value : throw new InvalidOperationException($"Value {name} can't be negative but was {value}");
}