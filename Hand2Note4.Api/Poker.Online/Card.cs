using System.Text.Json.Serialization;

namespace Hand2Note4.Api;

[BinarySerializable]
public struct 
Card {
    [Tag(1)] public CardRanks Rank { get; private set;}
    [Tag(2)] public Suits Suit { get; private set;}
    [JsonConstructor]
    public Card(CardRanks rank, Suits suit) {
        Rank = rank;
        Suit = suit;
    }

    public static Card 
    FromCardIndex(int index) => new Card((CardRanks)(index % 13), (Suits)(index / 13));

    public static bool operator ==(Card card1, Card card2) => card1.Equals(card2);
    public static bool operator !=(Card card1, Card card2) => !card1.Equals(card2);
}

public enum 
Suits {
    Hearts = 0,
    Diamonds = 1,
    Clubs = 2,
    Spades = 3
}

public enum 
CardRanks {
    Deuce = 0,
    Three = 1,
    Four = 2,
    Five = 3,
    Six = 4,
    Seven = 5,
    Eight = 6,
    Nine = 7,
    Ten = 8,
    Jack = 9,
    Queen = 10,
    King = 11,
    Ace = 12
}



