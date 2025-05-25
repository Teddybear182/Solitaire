using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Runtime.InteropServices;
using System.Reflection.Emit;
using System.ComponentModel;
using System.Data.Common;


//----main game class----
public class Game
{
  static List<Card> deck = new List<Card>();
  static Column[] columns = new Column[7];
  static Move? move;
  static CommandHandler? commandHandler;
  static Stock? stock;

  public static Foundation foundation = new Foundation();

  public Win win = new Win(foundation, columns);

  static void Main(string[] args)
  {
    Game game = new Game();
    game.StartGame();
  }

  public void StartGame()
  {
    Console.Clear();
    Console.WriteLine("\n\n******Welcome to Solitaire!******");
    Console.WriteLine("     >>>>  Enter your move   <<<<     \n\n");

    deck = CreateDeck();
    columns = CreateColumns();
    stock = new Stock(deck);
    move = new Move(columns, stock, foundation);
    commandHandler = new CommandHandler(move, columns, stock, foundation);

    int moveCounter = 0;

    Console.WriteLine("To see available commands type 'help'");
    Console.WriteLine("To end the game type 'exit'\n");
    ShuffleDeck();
    ShuffleDeck();
    ShuffleDeck();
    ManagingColumns();

    DisplayDeck();
    stock.DisplayStock();
    foundation.DisplayFoundation();
    Console.WriteLine($" >>>>  Move nr {moveCounter}:  <<<<<\n");

    var input = Console.ReadLine();
    while (true)
    {
      if (input != null)
      {
        Console.WriteLine($"\n");
        commandHandler.handleCommand(input.ToString());
        moveCounter++;
        DisplayDeck();
        stock.DisplayStock();
        foundation.DisplayFoundation();
        win.CheckWin();
        Console.WriteLine($" >>>>  Move nr {moveCounter}:  <<<<<\n");
        input = Console.ReadLine();
      }
      else
      {
        Console.WriteLine("invalid command\n");
        input = Console.ReadLine();
      }
    }
  }

  //creates columns
  public Column[] CreateColumns()
  {
    Column[] columns = new Column[7];
    for (int i = 0; i < 7; i++)
    {
      columns[i] = new Column(i + 1);
    }
    return columns;
  }

  //shuffles the deck of cards
  public void ShuffleDeck()
  {
    Random rand = new Random();
    for (int i = deck.Count - 1; i > 0; i--)
    {
      Card tempValue = deck[i];
      int f = rand.Next(0, i + 1);
      deck[i] = deck[f];
      deck[f] = tempValue;
    }
  }

  //creates a deck of cards
  public List<Card> CreateDeck()
  {
    List<Card> deck = new List<Card>();
    string[] suits = { "Hearts", "Diamonds", "Clubs", "Spades" };
    for (int i = 1; i <= 13; i++)
    {
      foreach (string suit in suits)
      {
        deck.Add(new Card(i, suit));
      }
    }
    return deck;
  }

  //creates columns and adds cards to them
  public void ManagingColumns()
  {
    Console.WriteLine("\n");
    {
      int columnCount = 7;
      for (int i = 0; i < columnCount; i++)
      {
        columns[i].Number = i + 1;
        for (int j = 0; j <= i; j++)
        {
          if (deck.Count > 0)
          {
            Card card = deck[0];
            if (j == i)
            {
              columns[i].AddCard(card);
            }
            else
            {
              columns[i].AddHiddenCard(card);
            }
            deck.RemoveAt(0);
          }
        }
      }
    }
    ShuffleDeck();
  }


  //hard function for displaying the deck:)
  public void DisplayDeck()
  {
    int columnCount = 0;
    int maxRow = 0;
    string header = "";
    foreach (Column column in columns)
    {
      int a = column.hiddenCards.Count + column.shownCards.Count;
      if (a > maxRow)
      {
        maxRow = a;
      }
      int tab = columnCount * 25 - header.Length;
      if (tab < 0)
      {
        tab = 0;
      }
      columnCount++;
      header += new string(' ', tab) + $"Column {columnCount}:";
    }
    //Console.WriteLine($"Debug: maxRow == " + maxRow + $"\n");
    Console.WriteLine(header);

    for (int row = 0; row < maxRow; row++)
    {
      string message = "";
      for (int i = 0; i < columnCount; i++)
      {
        int maxHiddenCards = columns[i].hiddenCards.Count;
        int maxShownCards = columns[i].shownCards.Count;
        if (row < maxHiddenCards)
        {
          int messageLength = message.Length;
          int tab = i * 25 - messageLength;
          if (tab < 0)
          {
            tab = 0;
          }
          //message += new string(' ', tab) + $"{columns[i].hiddenCards[row].Value} {columns[i].hiddenCards[row].Suit} (hidden)";
          message += new string(' ', tab) + $"XXXX (hidden)";
          continue;
        }
        if (row - maxHiddenCards >= 0 && row - maxHiddenCards < maxShownCards)
        {
          int messageLength = message.Length;
          int tab = i * 25 - messageLength;
          if (tab < 0)
          {
            tab = 0;
          }
          message += new string(' ', tab) + $"{columns[i].shownCards[row - maxHiddenCards].Value} {columns[i].shownCards[row - maxHiddenCards].Suit}";
        }
      }
      Console.WriteLine(message);
    }
  }
}


/*--class representing the foundation--*/
public class Foundation
{
  public class Pile
  {
    public string suit;
    public List<Card> cards = new List<Card>();

    public Pile(string Suit)
    {
      suit = Suit;
    }

    public void Add(Card card)
    {
      cards.Add(card);
    }

    public void Remove(Card card)
    {
      cards.Remove(card);
    }
  }

  public string[] suits = { "Hearts", "Diamonds", "Clubs", "Spades" };

  public Pile hearts = new Pile("Hearts");
  public Pile diamonds = new Pile("Diamonds");
  public Pile clubs = new Pile("Clubs");
  public Pile spades = new Pile("Spades");
  public List<Pile> allCards;

  public Foundation()
  {
    allCards = new List<Pile>
    {
      hearts,
      diamonds,
      clubs,
      spades
    };
  }

  //adds a card to the foundation
  public void AddCard(Card card, string suitName, Column fromColumn)
  {
    suitName = suitName.Trim();
    int pile = Array.IndexOf(suits, suitName);
    //Console.WriteLine($"Debug: suitIndex == {suitIndex} pile == {pile}, allCards[pile] == {allCards[pile]}");
    if (pile == -1)
    {
      Console.WriteLine("cant add card to foundation\n");
      return;
    }

    if (allCards[pile].cards.Count == 0 && card.Value == 1 && card.Suit == suitName)
    {
      allCards[pile].Add(card);
      fromColumn.RemoveCard(card);
      if (fromColumn.shownCards.Count == 0 && fromColumn.hiddenCards.Count > 0)
      {
        fromColumn.ShowCard();
      }
    }
    else if (allCards[pile].cards.Count == 0 && card.Value != 1)
    {
      Console.WriteLine("cant add card to foundation\n");
    }
    else
    {
      if (card.Value == allCards[pile].cards.Count + 1 && card.Suit == suitName)
      {
        allCards[pile].Add(card);
        fromColumn.RemoveCard(card);
        if (fromColumn.shownCards.Count == 0 && fromColumn.hiddenCards.Count > 0)
        {
          fromColumn.ShowCard();
        }
      }
      else
      {
        Console.WriteLine("cant add card to foundation\n");
      }
    }
  }

  //removes a card from the foundation
  public void RemoveCard(Card card, string suitIndex)
  {
    suitIndex = suitIndex.Trim();
    int pile = Array.IndexOf(suits, suitIndex);
    if (allCards[pile].cards.Count == 0)
    {
      Console.WriteLine("cant remove card from empty foundation!\n");
      return;
    }
    allCards[pile].Remove(card);
  }

  //displaying the foundation
  public void DisplayFoundation()
  {
    Console.WriteLine("\nFoundation:");
    foreach (Pile pile in allCards)
    {
      Console.WriteLine(pile.suit + ":");
      foreach (Card card in pile.cards)
      {
        Console.WriteLine($"- {card.Value} {card.Suit}");
      }
      if (pile.cards.Count == 0)
      {
        Console.WriteLine("- empty");
      }
    }
    Console.WriteLine("\n");
  }

  //checking if the player has won
  public bool IsComplete()
  {
    return hearts.cards.Count == 13 && diamonds.cards.Count == 13 && clubs.cards.Count == 13 && spades.cards.Count == 13;
  }
}


/*--class representing a stock pile of cards--*/
public class Stock
{
  public List<Card> cards;
  public List<Card> waste = new List<Card>();

  public Stock(List<Card> Cards)
  {
    cards = Cards;
  }

  //adds a card to the stock
  public void AddCard(Card card)
  {
    cards.Add(card);
  }

  //removes a card from the stock
  public void RemoveCard(Card card)
  {
    cards.Remove(card);
  }

  //adds a card to the waste pile, removing it from the stock
  public void AddWaste(Card card)
  {
    cards.Remove(card);
    waste.Add(card);
  }

  //resets the waste pile, moving all cards back to the stock and shuffling them
  public void ResetWaste()
  {
    foreach (Card card in waste)
    {
      cards.Add(card);
    }
    ShuffleCards();
    waste.Clear();
  }

  //shuffles the cards in the stock
  public void ShuffleCards()
  {
    Random rand = new Random();
    for (int i = cards.Count - 1; i > 0; i--)
    {
      Card tempValue = cards[i];
      int f = rand.Next(0, i + 1);
      cards[i] = cards[f];
      cards[f] = tempValue;
    }
  }

  //proceed to the next card in the stock, adding the previous one to the waste pile
  public void NextCard()
  {
    if (cards.Count > 0)
    {
      Card card = cards[0];
      cards.RemoveAt(0);
      waste.Add(card);
    }
    else
    {
      ResetWaste();
    }
  }

  //displaying stock pile
  public void DisplayStock()
  {
    Console.WriteLine("\n\nStock Pile:");
    string stockOutput = "";
    foreach (Card card in cards)
    {
      stockOutput += "XXXX ";
    }
    Console.WriteLine(stockOutput + "\n");
    DisplayWaste();
  }

  //displaying the waste pile
  public void DisplayWaste()
  {
    Console.WriteLine("\n\nWaste Pile:");
    string wasteOutput = "";
    foreach (Card card in waste)
    {
      wasteOutput += $"{card.Value} {card.Suit} ";
    }
    Console.WriteLine(wasteOutput + "\n");
  }
}


/*--class representing a card--*/
public class Card
{
  public int Value;
  public string Suit;

  public Card(int value, string suit)
  {
    Value = value;
    Suit = suit;
  }
}


/*--class representing a column of cards--*/
public class Column
{
  public int Number;
  public List<Card> shownCards;
  public List<Card> hiddenCards;

  public Column(int number)
  {
    Number = number;
    shownCards = new List<Card>();
    hiddenCards = new List<Card>();
  }

  //adding a shown card to the column
  public void AddCard(Card card)
  {
    shownCards.Add(card);
  }

  //adding a hidden card to the column
  public void AddHiddenCard(Card card)
  {
    hiddenCards.Add(card);
    //Console.WriteLine($"Card added {card.Value} {card.Suit} to column {Number}");
  }

  //deletes a shown card from the column
  public void RemoveCard(Card card)
  {
    shownCards.Remove(card);
  }

  //deletes a hidden card from the column
  public void RemoveHiddenCard(Card card)
  {
    hiddenCards.Remove(card);
  }

  //shows the last hidden card in the column
  public void ShowCard()
  {
    if (hiddenCards.Count > 0 && shownCards.Count == 0)
    {
      Card card = hiddenCards[hiddenCards.Count - 1];
      hiddenCards.Remove(card);
      shownCards.Add(card);
    }
  }


  //checking if the column is empty
  public bool IsEmpty()
  {
    return shownCards.Count == 0 && hiddenCards.Count == 0;
  }
}


/*--class responsible for moves--*/
public class Move
{
  private Column[] Columns;

  private Stock stock;

  private Foundation foundation;

  public Move(Column[] columns, Stock Stock, Foundation Foundation)
  {
    Columns = columns;
    stock = Stock;
    foundation = Foundation;
  }

  //methods for moving cards
  public void moveCardColumn(int from, int to, int amount)
  {
    CheckAddToColumn(from, to, amount);
  }

  public void moveCardStock(int to)
  {
    if (stock.waste.Count == 0)
    {
      Console.WriteLine("No uncovered cards in stock!");
      return;
    }
    Card card = stock.waste[stock.waste.Count - 1];
    CheckAddToColumnFromStock(card, to);
  }

  public void moveCardToFoundation(int from, string to)
  {
    Column fromColumn = Columns[from];
    if (fromColumn.shownCards.Count == 0)
    {
      Console.WriteLine("cant move card from empty column!\n");
      return;
    }
    Card card = fromColumn.shownCards.Last();
    foundation.AddCard(card, to, fromColumn);
  }


  //checking if the column is empty
  public void CheckAddToColumn(int from, int to, int amount)
  {
    Column fromColumn = Columns[from];
    Column toColumn = Columns[to];

    if (amount <= 0 || amount > fromColumn.shownCards.Count)
    {
      Console.WriteLine("\ninvalid number of cards to move!");
      return;
    }

    List<Card> cardsToMove = fromColumn.shownCards.Skip(fromColumn.shownCards.Count - amount).ToList();
    Card firstCard = cardsToMove.First();
    //Console.WriteLine($"Debug: firstCard == {firstCard.Value} {firstCard.Suit} fromColumn == {fromColumn.Number} toColumn == {toColumn.Number}");
    if (toColumn.shownCards.Count == 0)
    {
      if (firstCard.Value != 13)
      {
        Console.WriteLine("\ncant move card to empty column!");
        return;
      }
    }
    else
    {
      Card topCard = toColumn.shownCards.Last();
      bool differentColorRed = (firstCard.Suit == "Hearts" || firstCard.Suit == "Diamonds") != (topCard.Suit == "Hearts" || topCard.Suit == "Diamonds");
      bool differentColorBlack = (firstCard.Suit == "Clubs" || firstCard.Suit == "Spades") != (topCard.Suit == "Clubs" || topCard.Suit == "Spades");

      if (!differentColorRed && !differentColorBlack || firstCard.Value != topCard.Value - 1)
      {
        Console.WriteLine("cant move card to this column!\n");
        return;
      }
    }

    foreach (Card card in cardsToMove)
    {
      toColumn.AddCard(card);
      fromColumn.RemoveCard(card);
    }
    fromColumn.ShowCard();
  }


  //sprawdza czy można przenieść karty do kolumny ze stosu
  public void CheckAddToColumnFromStock(Card card, int to)
  {
    Column toColumn = Columns[to];

    if (toColumn.shownCards.Count == 0)
    {
      if (card.Value != 13)
      {
        Console.WriteLine("cant move card to empty column!\n");
        return;
      }
    }
    else
    {
      Card topCard = toColumn.shownCards.Last();
      bool differentColorRed = (card.Suit == "Hearts" || card.Suit == "Diamonds") != (topCard.Suit == "Hearts" || topCard.Suit == "Diamonds");
      bool differentColorBlack = (card.Suit == "Clubs" || card.Suit == "Spades") != (topCard.Suit == "Clubs" || topCard.Suit == "Spades");

      if (!differentColorRed && !differentColorBlack || card.Value != topCard.Value - 1)
      {
        Console.WriteLine("cant move card to this column!\n");
        return;
      }
    }

    toColumn.AddCard(card);
    stock.RemoveCard(card);
  }
}


/*--class interpreting commands given by the player--*/
public class CommandHandler
{
  private Move move;
  private Column[] columns;
  private Foundation foundation;
  private Stock stock;

  public CommandHandler(Move Move, Column[] Columns, Stock Stock, Foundation Foundation)
  {
    move = Move;
    columns = Columns;
    stock = Stock;
    foundation = Foundation;
  }

  //method for handling commands entered by the player
  public void handleCommand(string input)
  {
    try
    {
      string[] commandParts = input.Split(' ');
      int from;
      int to;
      int amount;
      if (commandParts[0] == "mv")
      {
        if (commandParts[1] == "stock")
        {
          to = int.Parse(commandParts[2]) - 1;
          move.moveCardStock(to);
        }
        else if (commandParts[1] == "foundation")
        {
          from = int.Parse(commandParts[2]) - 1;
          string toSuit = commandParts[3].ToString();
          move.moveCardToFoundation(from, toSuit);
        }
        else
        {
          from = int.Parse(commandParts[1]) - 1;
          to = int.Parse(commandParts[2]) - 1;
          amount = int.Parse(commandParts[3]);
          move.moveCardColumn(from, to, amount);
        }
      }
      else if (commandParts[0] == "nextCard")
      {
        stock.NextCard();
      }
      else if (commandParts[0] == "reset")
      {
        stock.ResetWaste();
      }
      else if (commandParts[0] == "exit")
      {
        Environment.Exit(0);
      }
      else if (commandParts[0] == "restart")
      {
        Console.Clear();
        Game game = new Game();
        game.StartGame();
      }
      else if (commandParts[0] == "help")
      {
        Console.WriteLine("Available commands:");
        Console.WriteLine("mv <column> <target column> <number of cards> - move cards from one column to another");
        Console.WriteLine("mv stock <target column> - move card from stock to column");
        Console.WriteLine("mv foundation <column> <card color> - move card to specified foundation");
        Console.WriteLine("nextCard - reveal next card from stock");
        Console.WriteLine("reset - reset revealed cards from stock");
        Console.WriteLine("exit - end game");
        Console.WriteLine("restart - start a new game\n");
      }
      else
      {
        Console.WriteLine("invalid command\n");
      }
    }
    catch (IndexOutOfRangeException)
    {
      Console.WriteLine("invalid card index\n");
    }
    catch (Exception ex)
    {
      Console.WriteLine($"an error occurred: {ex.Message}\n");
    }
  }
}


/*--class responsible for win condition--*/
public class Win
{
  private Foundation foundation;
  private Column[] columns;
  public bool isWin = false;

  public Win(Foundation Foundation, Column[] Columns)
  {
    foundation = Foundation;
    columns = Columns;
  }

  //checking if the player has won
  public void CheckWin()
  {
    if (foundation.IsComplete() && columns.All(c => c.IsEmpty()))
    {
      isWin = true;
      Console.WriteLine(">>>>>-You Won!-<<<<<<<");
    }
  }
}