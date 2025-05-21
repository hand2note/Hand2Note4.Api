namespace Hand2Note4.Api;
  
[TypeTag(1, typeof(Board))]
[TypeTag(2, typeof(MultiRunBoard))]
public interface 
IBoard { }

[BinarySerializable]
public class 
Board: IBoard {
    public const int FirstBoardNumber = 1;
    [Tag(1)] public InlineList<Card> Cards { get; private set;}
    public Streets Street {get; private set;}

    public Board(InlineList<Card> cards) {
        Cards = cards;
        Street = cards.Count.CardsCountToStreet();
    }
    
    public Board(List<Card> cards) {
        Cards = cards.ToInlineList();
        Street = cards.Count.CardsCountToStreet();
    }

    public bool IsPreflop => Street == Streets.Preflop;
    public bool IsRiver => Street == Streets.River;

}

[BinarySerializable]
public class 
MultiRunBoard : IBoard {
    [Tag(1)] public BoardMap<Board> Boards { get;}
    public MultiRunBoard(BoardMap<Board> boards) => Boards = boards;
}  

[BinarySerializable]
public struct 
BoardMap<T> {
    [Tag(1)] public Items10<T> InnerValues; 
    [Tag(2)] public int BoardNumbers {get;private set;}
    public BoardMap(Items10<T> innerValues, int boardNumbers) {
        InnerValues = innerValues;
        BoardNumbers = boardNumbers;
    }
}

public static class 
BoardHelper {
        
    public static Streets 
    CardsCountToStreet(this int cardsCount) {
        if (cardsCount == 0)
            return Streets.Preflop;
        if (cardsCount < 3 || cardsCount > 5)
            throw new ArgumentException($"Invalid board cards count {cardsCount}", nameof(cardsCount));
        return (Streets)(cardsCount - 2);
    }
}
