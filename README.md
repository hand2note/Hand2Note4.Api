# Hand2Note API

Contains Hand2Note API for third party tools integration.

Hand2Note is an online poker HUD software at https://hand2note.com

Currently available on C#.

**Installation**

> dotnet add package Hand2Note4Api

## HUD integration

Use it if you need to show dynamic or static HUD in any window or you need to send a hand history to Hand2Note, process it and save it into the database.

This API is intended for third party converters on poker sites unsupported by Hand2Note itself. This API should **NOT** be used to violate poker sites' software restrictions especially on **PokerStars** and measures to maintain the compliance will be taken by us.

General use cases:

Notifies Hand2Note that the new hand has been started
```C#
Hand2Note.Send(new HandStartMessage(
    windowId: 1,
    gameNumber: 1,
    userUsername: null,
    table: new Table(
        room: Rooms.PokerStars,
        gameType: new PokerGameType(
            pokerGame: PokerGames.TexasHoldem,
            betLimits: BetLimits.NoLimit, 
            sessionType: SessionTypes.Cash),
        stakes: new Stakes(smallBlind: 1, bigBlind: 2),
        tableSize: 6,
        currency: Currencies.Dollar),
    players: ... //Fill ImmutableList<HandStartMessagePlayer>,
    cashDrop: 0)
);
```

Notifies  Hand2Note that the street has been changed
```C#
Hand2Note.Send( 
    new DealMessage(
      windowId: 1,
      gameNumber: 1,
      board: new Board(cards: "AcAhAd".ParseCards()),
      bankRake: BankRake.Create(potRakes: PotMap<double>.Empty.With(potNumber: 1, value: 0.1)));
);
```

Notifies  Hand2Note that the action has been done
```C#
Hand2Note.Send(new ActionMessage(
    windowId: 1,
    seatNumber: 1,
    gameNumber: 1,
    actionType: ActionTypes.Raise,
    amount: 2)
);
```

Sends a "static" hand history to Hand2Note to update players' statistics. 
Set format property to 1, hand history should be base64 encoded.
```C#
Hand2Note.Send(new HandHistoryMessage(
    room: Rooms.GGNetwork,
    gameNumber: 79556501, 
    format:1,
    handHistoryBase64: Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(handHistoryText)))
);
```

Checks if Hand2Note is running
```C#
if (Hand2Note.IsHand2NoteRunning()){
...
}
```

Use Activity monitor to know when Hand2Note client was started or closed
```C#
var activityMonitor = new ActivityMonitor();
activityMonitor.Hand2NoteStarted += Hand2NoteStartedHandler;
activityMonitor.Hand2NoteClosed += Hand2NoteClosedHandler;
```
