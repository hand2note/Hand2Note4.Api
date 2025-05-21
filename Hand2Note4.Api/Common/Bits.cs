namespace Hand2Note4.Api;

public static class 
BitHelper {

    public static bool 
    ContainsBit(this int value, int bitNumber) => (value & (1 << bitNumber - 1)) != 0;

    public static InlineList<int>
    GetBitNumbers(this int value) {
        var result = new InlineList<int>();
        for (int i = 0; i < 32; i++) {
            if ((value & (1 << i)) != 0)
                result.Add(i+1);
            if ((1 << i) >= value)
                return result;
        }
        return result;
    }
}
