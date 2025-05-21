using System.Collections.Immutable;

namespace Hand2Note4.Api;

[TypeTag(1, typeof(JustPlayersMessage))]
[TypeTag(2, typeof(ActionMessage))]
[TypeTag(3, typeof(HandStartMessage))]
[TypeTag(4, typeof(DealMessage))]
[TypeTag(5, typeof(TableClosedMessage))]
[TypeTag(6, typeof(TableOpenedMessage))]
[TypeTag(7, typeof(HandHistoryMessage))]
[TypeTag(8, typeof(TableErrorDynamicMessage))]
[BinarySerializable]
public interface 
DynamicMessage {}

/// <summary>
/// Use this to show the HUD as soon as the players at the table are known, 
/// even if other information like stack sizes is still unavailable. 
/// </summary>
[BinarySerializable]
public class 
JustPlayersMessage : DynamicMessage {
    [Tag(1)] public int WindowId {get;}
    [Tag(2)] public string? UserUsername {get;}
    [Tag(3)] public Table Table {get; }
    [Tag(4)] public ImmutableList<(int seatNumber, string username)> Players {get; }
    public JustPlayersMessage(int windowId, string? userUsername, Table table, ImmutableList<(int seatNumber, string username)> players) {
        WindowId = windowId;
        UserUsername = userUsername;
        Table = table;
        Players = players;
    }
}

/// <summary>
/// Notifies Hand2Note that the action has been done
/// </summary>
[BinarySerializable]
public class 
ActionMessage: DynamicMessage {
    [Tag(1)] public int WindowId {get;}
    [Tag(2)] public long GameNumber {get;}
    [Tag(3)] public int SeatNumber {get;}
    [Tag(4)] public ActionTypes ActionType {get;}
    [Tag(5)] public double Amount {get;}
    public ActionMessage(int windowId, long gameNumber, int seatNumber, ActionTypes actionType, double amount) {
        WindowId = windowId;
        GameNumber = gameNumber;
        SeatNumber = seatNumber;
        ActionType = actionType;
        Amount = amount;
    }
}

/// <summary>
/// Notifies Hand2Note that the new hand has been started
/// </summary>
[BinarySerializable]
public record 
HandStartMessage: DynamicMessage {
    [Tag(1)] public int WindowId {get;}
    [Tag(2)] public long GameNumber {get;}
    [Tag(3)] public string? UserUsername {get;}
    [Tag(4)] public Table Table {get;}
    [Tag(5)] public ImmutableList<HandStartMessagePlayer> Players  {get;init;}
    [Tag(6)] public double CashDrop {get;}
    public HandStartMessage(int windowId, long gameNumber, string? userUsername, Table table, ImmutableList<HandStartMessagePlayer> players, double cashDrop) {
        WindowId = windowId;
        GameNumber = gameNumber;
        UserUsername = userUsername;
        Table = table;
        Players = players;
        CashDrop = cashDrop.VerifyArgumentNotNegative(nameof(cashDrop));
    }
}

/// <summary>
/// Notifies Hand2Note that the street has been changed
/// </summary>
[BinarySerializable]
public class 
DealMessage : DynamicMessage {
    [Tag(1)] public long GameNumber {get;}
    [Tag(2)] public int WindowId {get;}
    [Tag(3)] public IBoard Board {get;}
    [Tag(4)] public BankRake? BankRake { get; }
    public DealMessage(long gameNumber, int windowId, IBoard board, BankRake? bankRake) {
        GameNumber = gameNumber;
        WindowId = windowId;
        Board = board;
        BankRake = bankRake;
    }
}

/// <summary>
/// Contains information about the player at the beginning of the hand, before any blinds are posted.
/// </summary>
[BinarySerializable]
public record 
HandStartMessagePlayer:IHasSeatNumber, IHasUsername {
    [Tag(1)] public int SeatNumber {get;}
    [Tag(2)] public string Username {get;init;}
    [Tag(3)] public double InitialStackSize {get;}
    [Tag(5)] public bool IsSittingOut {get;}
    [Tag(6)] public RegularPostedBlind PostedBlind {get;}
    [Tag(7)] public LiveMissedPostedBlind PostedBlindOutOfQueue {get;}
    [Tag(8)] public DeadPostedBlind PostedDeadBlind {get;}
    [Tag(9)] public bool IsDealer {get;}
    /// <summary>
    /// An alternative name of the player in the room. For example, a nickname in chinese mobile rooms.
    /// Null in case the player may have a pseudonym but is is unknown.
    /// </summary>
    [Tag(10)] public IPseudonym? Pseudonym { get; }
    public HandStartMessagePlayer(int seatNumber, string username, IPseudonym? pseudonym, double initialStackSize, bool isSittingOut, RegularPostedBlind postedBlind, LiveMissedPostedBlind postedBlindOutOfQueue, DeadPostedBlind postedDeadBlind, bool isDealer) {
        SeatNumber = seatNumber;
        Username = username;
        Pseudonym = pseudonym;
        InitialStackSize = initialStackSize;
        IsSittingOut = isSittingOut;
        PostedBlind = postedBlind;
        PostedBlindOutOfQueue = postedBlindOutOfQueue;
        PostedDeadBlind = postedDeadBlind;
        IsDealer = isDealer;
    }
}

/// <summary>
/// Notifies Hand2Note that a new poker table has been opened.
/// </summary>
[BinarySerializable]
public class 
TableOpenedMessage: DynamicMessage {
    [Tag(1)] public int WindowId {get;}
    [Tag(2)] public Rooms Room {get; }
    [Tag(3)] public Table? Table {get;}
    public TableOpenedMessage(int windowId, Rooms room, Table? table) {
        WindowId = windowId;
        Room = room;
        Table = table;
    }
}

/// <summary>
/// Notifies Hand2Note that the HUD should be removed.
/// </summary>
[BinarySerializable]
public class 
TableClosedMessage : DynamicMessage {
    [Tag(1)] public int WindowId {get;}
    public TableClosedMessage(int windowId) => WindowId = windowId;

    public override string 
    ToString() => $"{nameof(TableClosedMessage)}: windowId={WindowId}";
}

/// <summary>
/// Sends a hand history to Hand2Note to update player statistics.
/// </summary>
[BinarySerializable]
public class 
HandHistoryMessage : DynamicMessage {
    [Tag(1)] public Rooms Room { get; }
    [Tag(2)] public long GameNumber { get; }
    [Tag(3)] public int Format {get; }
    [Tag(4)] public string HandHistoryBase64 { get; }
    public HandHistoryMessage(Rooms room, long gameNumber, int format, string handHistoryBase64) {
        Room = room;
        GameNumber = gameNumber;
        Format = format;
        HandHistoryBase64 = handHistoryBase64;
    }
}

/// <summary>
/// Notifies Hand2Note that poker table should be reopened
/// </summary>
[BinarySerializable]
public class 
TableErrorDynamicMessage : DynamicMessage {
    [Tag(1)]public int WindowId {get;}
    [Tag(2)]public string Message {get;}
    [Tag(3)]public Rooms Room { get; }
    public TableErrorDynamicMessage(int windowId, string message, Rooms room) {
        WindowId = windowId;
        Message = message;
        Room = room;
    }
}