namespace Hand2Note4.Api;
public static class
Cards {
    
    public static InlineList<Card> 
    ParseCards(this string str) => TryParseCards(str, out var res)
        ? res : throw new InvalidOperationException($"Failed to parse cards from \"{str}\"");
    
    public static bool 
    TryParseCards(this string @string, out InlineList<Card> result) {
        result = new InlineList<Card>();
        var ranksWithoutSuits = new List<CardRanks>();
        for (int i = 0; i < @string.Length - 1; i++) {
            if (TryParseCard(@string[i], @string[i + 1], out var card)) {
                if (ranksWithoutSuits.Count > 0) {
                    foreach(var rank in ranksWithoutSuits) 
                        result.Add(new Card(rank, suit: card.Suit));
                    ranksWithoutSuits.Clear();
                }
                    
                result.Add(card);
                i++;
            }
            else {
                if (@string[i].TryParseCardRank(out var rank)) 
                    ranksWithoutSuits.Add(rank);
                else 
                    return false;
            }
        }
        return true;
    }
    
    public static bool 
    TryParseCard(char rankChar, char suitChar,  out Card card) {
        if (suitChar.TryParseSuits(out var suit) && rankChar.TryParseCardRank(out var rank)) {
            card = new Card(rank, suit);
            return true;
        }
        card = default;
        return false;
    }

    public static bool 
    TryParseCardRank(this char c, out CardRanks res) {
        switch (c) {
            case 'A' or 'a':
                res = CardRanks.Ace;
                return true;
            case 'K' or 'k':
                res = CardRanks.King;
                return true;
            case 'Q' or 'q':
                res = CardRanks.Queen;
                return true;
            case 'J' or 'j':
                res = CardRanks.Jack;
                return true;
            case 'T' or 't':
                res = CardRanks.Ten;
                return true;
            case '9':
                res = CardRanks.Nine;
                return true;
            case '8':
                res = CardRanks.Eight;
                return true;
            case '7':
                res = CardRanks.Seven;
                return true;
            case '6':
                res = CardRanks.Six;
                return true;
            case '5':
                res = CardRanks.Five;
                return true;
            case '4':
                res = CardRanks.Four;
                return true;
            case '3':
                res = CardRanks.Three;
                return true;
            case '2':
                res = CardRanks.Deuce;
                return true;
            default:
                res = default;
                return false;
        }
    }
    
    public static bool 
    TryParseSuits(this char suitChar, out Suits suit) {
        switch(suitChar){
            case 'h' or 'H': 
                suit = Suits.Hearts;
                return true;
            case 's' or 'S': 
                suit = Suits.Spades;
                return true;
            case 'c' or 'C': 
                suit = Suits.Clubs;
                return true;
            case 'd' or 'D':
                suit = Suits.Diamonds;
                return true;
        }
       
        suit = default;
        return false;
    }
}

