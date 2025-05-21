using System.Collections.Immutable;

namespace Hand2Note4.Api.Tests;

public static class ApiTests {

    [TestCase(333)]
    public static void
    TestHandStart(int windowId) {
        SetupTable(windowId: windowId, smallBlind: 0.5, bigBLind: 1);
       
        StartNewHand();
        Action(1, ActionTypes.Fold, 0);
        Action(2, ActionTypes.Fold, 0);
        Action(3, ActionTypes.Fold, 0);
        Action(4, ActionTypes.Raise, 3);
        Action(5, ActionTypes.Fold, 0);
        Action(6, ActionTypes.Call, 2);
        
        DealBoard("AcThJd");
        Action(6, ActionTypes.Check, 0);
        Action(4, ActionTypes.Check, 0);
      
        
        DealBoard("AcThJdKh");
        Action(6, ActionTypes.Check, 0);
        Action(4, ActionTypes.Check, 0);
        
        DealBoard("AcThJdKh3h");
        Action(6, ActionTypes.Bet, 5);
        Action(4, ActionTypes.Fold, 0);
    }

    private static void
    SetupTable(int windowId, double smallBlind, double bigBLind) {
        _windowId = windowId;
        _smallBlind = smallBlind;
        _bigBlind = bigBLind;
    }
    private static int _windowId = 0;
    private static double _smallBlind = 0.5;
    private static double _bigBlind = 1;
    
    private static void
    StartNewHand() {
        var message = new HandStartMessage(
            windowId: _windowId,
            gameNumber: 1,
            userUsername: null,
            table: new Table(
                room: Rooms.PokerStars,
                gameType: new PokerGameType(
                    pokerGame: PokerGames.TexasHoldem,
                    betLimits: BetLimits.NoLimit, 
                    sessionType: SessionTypes.Cash),
                stakes: new Stakes(smallBlind: _smallBlind, bigBlind: _bigBlind),
                tableSize: 6,
                currency: Currencies.Dollar),
            players: GetTestPlayers().ToImmutableList(),
            cashDrop: 0);
        Hand2Note.Send(message);
    }
    
    private static void
    Action(int seatNumber, ActionTypes actionType, double amountInBlinds) {
        Task.Delay(1000).Wait();
        var message = new ActionMessage(
            windowId: _windowId,
            seatNumber: seatNumber,
            gameNumber: 1,
            actionType: actionType,
            amount: amountInBlinds * _bigBlind);
      
        Hand2Note.Send(message);
    }
 
    private static void
    DealBoard(string cards) {
        Task.Delay(1000).Wait();
        var message = new DealMessage(
            windowId: _windowId,
            gameNumber: 1,
            board: new Board(cards: cards.ParseCards()),
            bankRake: BankRake.Create(potRakes: PotMap<double>.Empty.With(potNumber: 1, value: 0.1)));
      
        Hand2Note.Send(message);
    }

    private static IEnumerable<HandStartMessagePlayer>
    GetTestPlayers() {
        yield return GetPlayer(seatNumber: 1, isDealer: false, postedBlind: RegularPostedBlind.None);
        yield return GetPlayer(seatNumber: 2, isDealer: false, postedBlind: RegularPostedBlind.None);
        yield return GetPlayer(seatNumber: 3, isDealer: false, postedBlind: RegularPostedBlind.None);
        yield return GetPlayer(seatNumber: 4, isDealer: true, postedBlind: RegularPostedBlind.None);
        yield return GetPlayer(seatNumber: 5, isDealer: false, postedBlind: new RegularPostedBlind(RegularBlindTypes.SmallBlind, _smallBlind));
        yield return GetPlayer(seatNumber: 6, isDealer: false, postedBlind: new RegularPostedBlind(RegularBlindTypes.BigBlind, _bigBlind));
    }
    
    private static HandStartMessagePlayer
    GetPlayer(int seatNumber, bool isDealer, RegularPostedBlind postedBlind) =>
        new HandStartMessagePlayer(
            seatNumber: seatNumber,
            username: $"seat{seatNumber}",
            pseudonym: null,
            initialStackSize: 100,
            isSittingOut: false,
            postedBlind: postedBlind, 
            postedBlindOutOfQueue: LiveMissedPostedBlind.None,
            postedDeadBlind: DeadPostedBlind.None, 
            isDealer: isDealer
            );
}