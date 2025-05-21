namespace Hand2Note4.Api;

[Flags]
public enum 
SessionTypes {
    Cash = 1,
    Tournament = 1 << 1,
    All = Cash | Tournament
}