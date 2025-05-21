namespace Hand2Note4.Api;

/// <summary>
/// Represents a table a hand played at. 
/// </summary>
[BinarySerializable]
public class 
Table {
    public const int MaxTableSize = 10;
    [Tag(1)] public Rooms Room {get; private set; }
    [Tag(12)] public int? ClubId {get;private set; }
    [Tag(2)] public string? Name {get;private set; }
    [Tag(3)] public int TableSize {get;private set; }
    [Tag(4)] public PokerGameType GameType {get;private set; }
    [Tag(5)] public Stakes Stakes {get;private set; }
    /// <summary>
    /// Currency of chips in the hand.
    /// </summary>
    [Tag(6)] public Currencies Currency {get;private set; }
    [Tag(7)] public Tournament? Tournament {get;private set; }
    [Tag(8)] public int TournamentTableNumber {get;private set; }
    [Tag(9)] public bool IsAnonymousTable {get; private set;}
    [Tag(10)] public string? HomeGameHost {get; private set;}
    public Table(Rooms room, int tableSize, PokerGameType gameType, Stakes stakes, Currencies currency, Tournament? tournament = null, string? name = null, int? clubId = null, bool isAnonymousTable = false, int tournamentTableNumber = 0, string? homeGameHost = null) =>
        Fill(
            room: room, 
            tableSize: tableSize, 
            gameType: gameType, 
            stakes: stakes, 
            currency: currency, 
            tournament: tournament, 
            name: name, 
            clubId: clubId, 
            isAnonymousTable: isAnonymousTable, 
            tournamentTableNumber: tournamentTableNumber, 
            homeGameHost: homeGameHost);

    public void 
    Fill(Rooms room, int tableSize, PokerGameType gameType, Stakes stakes, Currencies currency, Tournament? tournament = null, string? name = null, int? clubId = null, bool isAnonymousTable = false, int tournamentTableNumber = 0, string? homeGameHost = null) {
        this.Name = name;
        this.TableSize = tableSize;
        this.GameType = gameType;
        this.Stakes = stakes;
        this.Currency = currency;
        this.HomeGameHost = homeGameHost??string.Empty;
        this.TournamentTableNumber = tournamentTableNumber;
        this.Room = room;
        this.ClubId = clubId;
        this.Tournament = tournament;
        this.IsAnonymousTable = isAnonymousTable;
        this.MainBlindType =  GameType.PokerGame.IsShortDeckFamily() && !room.IsStandardBlindsInShortDeck() ? RegularBlindTypes.ButtonBlind : RegularBlindTypes.BigBlind;
        if (gameType.IsTournament && tournament == null)
            throw new ArgumentException("Tournament can't be null for a tournament hand", nameof(tournament));
        if (gameType.IsCash && tournament != null)
            throw new ArgumentException("Tournament must be null for cash games", nameof(tournament));
        if ((currency == Currencies.None || currency == Currencies.Points) && !IsTournament) throw new ArgumentException($"Invalid currency {currency}", nameof(currency));

        if (IsTournament && currency != Currencies.None)
            throw new ArgumentException($"Currency of chips in tournaments should be always None but was {currency}");

        if (GameType.IsBombPot && Stakes.Ante.IsZero())
            throw new InvalidOperationException($"Ante can't be zero in Bomb Pots");
    }
    
    public RegularBlindTypes MainBlindType {get;private set; }

    public bool IsTournament => GameType.IsTournament;
}