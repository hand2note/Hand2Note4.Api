namespace Hand2Note4.Api;

public enum PokerSites {
    PokerStars = 1,
    Pacific = 2,
    IPoker = 3,
    PartyPoker = 4,
    WinningPokerNetwork = 5,
    Winamax = 6,
    Microgaming = 7,
    ChicoNetwork = 8,
    AconcaguaPoker = 9,
    EuropeBet = 10,
    SpartanPoker = 11,
    PokerDom = 12,
    Khelo365 = 13,
    PokerMatch = 14,
    RedArgentinaDePoker = 15,
    Baazi = 16,
    PokerMIRA = 20,
    GGNetwork = 22,
    Vbet = 23,
    Win2Day = 24,
    WWin = 25,
    HhPoker = 26,
    PlanetWin365 = 27,
    PPPoker = 28,
    PokerKingdom = 29,
    PokerKing = 30,
    FishPokers = 31,
    OhPoker = 32,
    OnePS = 33,
    PokerClans = 34,
    KKPoker = 35,
    PokerCommunity = 36,
    RedDragon = 37,
    WePoker = 38,
    PotatoPoker = 39,
    AllInAsia = 40,
    UPoker = 41,
    Adda52 = 42,
    SvenskaSpelPoker = 43,
    NineStacks = 44,
    PokerTime = 45,
    PandaPoker = 46,
    PokerBros = 47,
    CityOfPoker = 48,
    SwCPoker = 49,
    PokerSaint = 50,
    PokerHigh = 51,
    Ignition = 52,
    Pmu = 53,
    DPZX = 54,
    Pokio = 55,
    SupremaPoker = 56,
    Xpoker = 57,
    ClubGG = 58,
    MrPoker = 59,
    Pokerrr2 = 60,
    PeoplesPoker = 61,
    CoinPoker = 62,
    Intertops = 63,
    HighStakes = 64,
    YueJuBa = 65,
    Poker4U = 66,
    BetMGM = 68,
    WePokerPlus = 69,
    PokerWorld = 70,
    WptGlobal = 71,
    JackPoker = 72,
    PokerMan = 73,
    PokerNow = 74,
    PokerHub = 75,
    PokerRoom = 76,
    TribalPioneer = 77,
    NewPoker = 78,
    AAPoker = 79,
    MopoClub = 80,
    DDPoker = 81,
    DxVip = 82,
    FishPokerNet = 83,
    Aion = 84,
    CyberPoker = 85,
    Betclic = 86,
    TexasCardHouse = 109,
    SunBetPokerTour = 110,
    HongKongPokerTour = 111,
    HardRockPoker = 112,
    ChampionsPokerLive = 113,
    PBKCPokerStream = 114,
    PartyPokerTV = 115,
    TCHLiVEPoker = 116,
    PokerstarsLive = 117,
    PokerXpress = 118,
    WorldPokerTour = 119,
    GGPokerLive = 120,
    PokerGO = 121,
    PokerNightInAmerica = 122,
    KingsResort = 123,
    BallyLivePoker = 124,
    TritonPoker = 125,
    PokerAtTheLodge = 126,
    HustlerCasino = 127
}

public static class
RoomsFunctions {
    public static bool 
    IsStandardBlindsInShortDeck(this PokerSites pokerSite) {
        if (pokerSite is PokerSites.IPoker or PokerSites.Vbet)
            return true;

        return false;
    }
}