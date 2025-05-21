namespace Hand2Note4.Api;

[Flags]
public enum 
PokerGames: byte {
    TexasHoldem = 1,
    Omaha = 1 << 1,
    /// <summary>
    /// Short Deck Hold'em when straight beats trips
    /// </summary>
    ShortDeck = 1 << 2,
    /// <summary>
    /// Omaha with five pocket cards
    /// </summary>
    OmahaFive = 1 << 3,
    /// <summary>
    /// Short Deck Hold'em when trips beats straight
    /// </summary>
    ShortDeckTbs = 1 << 4,
    OmahaSix = 1 << 5,
    AllGames = TexasHoldem | Omaha | ShortDeck | OmahaFive | OmahaSix | ShortDeckTbs,
    OmahaFamily = Omaha | OmahaFive | OmahaSix
}

public static class 
PokerGamesFunctions {
    
    public static bool 
    IsShortDeckFamily(this PokerGames pokerGame) => pokerGame is PokerGames.ShortDeck or PokerGames.ShortDeckTbs or (PokerGames.ShortDeck | PokerGames.ShortDeckTbs);
}