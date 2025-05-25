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


//----główna klasa gry----
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
    Console.WriteLine("\n\n******Witam w grze pasjans!******");
    Console.WriteLine("     >>>>  Wpisz swój ruch   <<<<     \n\n");

    deck = CreateDeck();
    columns = CreateColumns();
    stock = new Stock(deck);
    move = new Move(columns, stock, foundation);
    commandHandler = new CommandHandler(move, columns, stock, foundation);

    int moveCounter = 0;

    Console.WriteLine("Aby zobaczyć dostępne komendy wpisz 'help'");
    Console.WriteLine("Aby zakończyć grę wpisz 'exit'\n");
    ShuffleDeck();
    ShuffleDeck();
    ShuffleDeck();
    ManagingColumns();

    DisplayDeck();
    stock.DisplayStock();
    foundation.DisplayFoundation();
    Console.WriteLine($" >>>>  Ruch nr {moveCounter}:  <<<<<\n");

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
        Console.WriteLine($" >>>>  Ruch nr {moveCounter}:  <<<<<\n");
        input = Console.ReadLine();
      }
      else
      {
        Console.WriteLine("nieprawidłowe polecenie\n");
        input = Console.ReadLine();
      }
    }
  }

  //twory listy kolumn
  public Column[] CreateColumns()
  {
    Column[] columns = new Column[7];
    for (int i = 0; i < 7; i++)
    {
      columns[i] = new Column(i + 1);
    }
    return columns;
  }

  //tasuje talię kart
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

  //tworzy talię kart
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

  //tworzy kolumny i dodaje do nich karty
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


  //trudna funkcja do ładnego wyświetlania kolumn:)
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
      header += new string(' ', tab) + $"Kolumna {columnCount}:";
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
          //message += new string(' ', tab) + $"{columns[i].hiddenCards[row].Value} {columns[i].hiddenCards[row].Suit} (ukryta)";
          message += new string(' ', tab) + $"XXXX (ukryta)";
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


/*--klasa reprezentująca stosy końcowe--*/
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

  //dodaje kartę do podstawy
  public void AddCard(Card card, string suitName, Column fromColumn)
  {
    suitName = suitName.Trim();
    int pile = Array.IndexOf(suits, suitName);
    //Console.WriteLine($"Debug: suitIndex == {suitIndex} pile == {pile}, allCards[pile] == {allCards[pile]}");
    if (pile == -1)
    {
      Console.WriteLine("nie można dodać karty do podstawy\n");
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
      Console.WriteLine("nie można dodać karty do podstawy\n");
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
        Console.WriteLine("nie można dodać karty do podstawy\n");
      }
    }
  }

  //usuwa kartę z podstawy
  public void RemoveCard(Card card, string suitIndex)
  {
    suitIndex = suitIndex.Trim();
    int pile = Array.IndexOf(suits, suitIndex);
    if (allCards[pile].cards.Count == 0)
    {
      Console.WriteLine("nie można usunąć karty z pustej podstawy!\n");
      return;
    }
    allCards[pile].Remove(card);
  }

  //wyświetla stosy końcowe
  public void DisplayFoundation()
  {
    Console.WriteLine("\nStosy końcowe:");
    foreach (Pile pile in allCards)
    {
      Console.WriteLine(pile.suit + ":");
      foreach (Card card in pile.cards)
      {
        Console.WriteLine($"- {card.Value} {card.Suit}");
      }
      if (pile.cards.Count == 0)
      {
        Console.WriteLine("- pusta");
      }
    }
    Console.WriteLine("\n");
  }

  //sprawdza czy wszystkie stosy końcowe są pełne
  public bool IsComplete()
  {
    return hearts.cards.Count == 13 && diamonds.cards.Count == 13 && clubs.cards.Count == 13 && spades.cards.Count == 13;
  }
}


/*--klasa reprezentująca stos rezerwowy kart--*/
public class Stock
{
  public List<Card> cards;
  public List<Card> waste = new List<Card>();

  public Stock(List<Card> Cards)
  {
    cards = Cards;
  }

  //dodaje kartę do stosu rezerwowego
  public void AddCard(Card card)
  {
    cards.Add(card);
  }

  //usuwa kartę ze stosu rezerwowego
  public void RemoveCard(Card card)
  {
    waste.Remove(card);
  }

  //dodaje kartę do odkrytych kart
  public void AddWaste(Card card)
  {
    cards.Remove(card);
    waste.Add(card);
  }

  //resetuje odkryte karty, dodaje je z powrotem do stosu i tasuje
  public void ResetWaste()
  {
    foreach (Card card in waste)
    {
      cards.Add(card);
    }
    ShuffleCards();
    waste.Clear();
  }

  //tasuje karty w stosie
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

  //przechodzi do następnej karty w stosie, dodaje poprzednią do odkrytych kart
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

  //wyświetla stos kart
  public void DisplayStock()
  {
    Console.WriteLine("\n\nStos kart:");
    string stockOutput = "";
    foreach (Card card in cards)
    {
      stockOutput += "XXXX ";
    }
    Console.WriteLine(stockOutput + "\n");
    DisplayWaste();
  }

  //wyświetla odkryte karty
  public void DisplayWaste()
  {
    Console.WriteLine("\n\nOdkryte karty:");
    string wasteOutput = "";
    foreach (Card card in waste)
    {
      wasteOutput += $"{card.Value} {card.Suit} ";
    }
    Console.WriteLine(wasteOutput + "\n");
  }
}


/*--klasa reprezentująca kartę--*/
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


/*--klasa reprezentująca kolumnę kart--*/
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

  //dodaje kartę do kolumny
  public void AddCard(Card card)
  {
    shownCards.Add(card);
  }

  //dodaje ukrytą kartę do kolumny
  public void AddHiddenCard(Card card)
  {
    hiddenCards.Add(card);
    //Console.WriteLine($"dodano kartę {card.Value} {card.Suit} do kolumny {Number}");
  }

  //usuwa kartę z kolumny
  public void RemoveCard(Card card)
  {
    shownCards.Remove(card);
  }

  //usuwa ukrytą kartę z kolumny
  public void RemoveHiddenCard(Card card)
  {
    hiddenCards.Remove(card);
  }

  //pokazuje ostatnią ukrytą kartę w kolumnie
  public void ShowCard()
  {
    if (hiddenCards.Count > 0 && shownCards.Count == 0)
    {
      Card card = hiddenCards[hiddenCards.Count - 1];
      hiddenCards.Remove(card);
      shownCards.Add(card);
    }
  }


  //sprawdza czy kolumna jest pusta
  public bool IsEmpty()
  {
    return shownCards.Count == 0 && hiddenCards.Count == 0;
  }
}


/*--klasa odpowiadająca za ruchy--*/
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

  //metody do przenoszenia kart
  public void moveCardColumn(int from, int to, int amount)
  {
    CheckAddToColumn(from, to, amount);
  }

  public void moveCardStock(int to)
  {
    if (stock.waste.Count == 0)
    {
      Console.WriteLine("Brak odkrytych kart na stosie!");
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
      Console.WriteLine("nie można przenieść karty z pustej kolumny!\n");
      return;
    }
    Card card = fromColumn.shownCards.Last();
    foundation.AddCard(card, to, fromColumn);
  }


  //sprawdza czy można przenieść karty do kolumny
  public void CheckAddToColumn(int from, int to, int amount)
  {
    Column fromColumn = Columns[from];
    Column toColumn = Columns[to];

    if (amount <= 0 || amount > fromColumn.shownCards.Count)
    {
      Console.WriteLine("\nnieprawidłowa liczba kart do przeniesienia!");
      return;
    }

    List<Card> cardsToMove = fromColumn.shownCards.Skip(fromColumn.shownCards.Count - amount).ToList(); //nie wiedziałem że takie coś jak Skip() istnieje ale fajne
    Card firstCard = cardsToMove.First();
    //Console.WriteLine($"Debug: firstCard == {firstCard.Value} {firstCard.Suit} fromColumn == {fromColumn.Number} toColumn == {toColumn.Number}");
    if (toColumn.shownCards.Count == 0)
    {
      if (firstCard.Value != 13)
      {
        Console.WriteLine("\nnie można przenieść karty do pustej kolumny!");
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
        Console.WriteLine("nie można przenieść karty do tej kolumny!\n");
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
        Console.WriteLine("nie można przenieść karty do pustej kolumny!\n");
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
        Console.WriteLine("nie można przenieść karty do tej kolumny!\n");
        return;
      }
    }

    toColumn.AddCard(card);
    stock.RemoveCard(card);
  }
}


/*--klasa interpretująca komendy, podawane przez gracza--*/
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

  //metoda obsługująca komendy wpisywane przez gracza
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
        Console.WriteLine("Dostępne komendy:");
        Console.WriteLine("mv <kolumna> <kolumna docelowa> <ilość kart> - przenieś karty z jednej kolumny do drugiej");
        Console.WriteLine("mv stock <kolumna docelowa> - przenieś kartę ze stosu do kolumny");
        Console.WriteLine("mv foundation <kolumna> <kolor karty> - przenieś kartę do wskazanego stosu końcowego");
        Console.WriteLine("nextCard - odkryj następną kartę ze stosu");
        Console.WriteLine("reset - zresetuj odkryte karty ze stosu");
        Console.WriteLine("exit - zakończ grę");
        Console.WriteLine("restart - rozpocznij nową grę\n");
      }
      else
      {
        Console.WriteLine("nieprawidłowe polecenie\n");
      }
    }
    catch (IndexOutOfRangeException)
    {
      Console.WriteLine("nieprawidłowy indeks karty\n");
    }
    catch (Exception ex)
    {
      Console.WriteLine($"wystąpił błąd: {ex.Message}\n");
    }
  }
}


/*--klasa odpowiadająca za wygraną--*/
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

  //sprawdza czy gracz wygrał
  public void CheckWin()
  {
    if (foundation.IsComplete() && columns.All(c => c.IsEmpty()))
    {
      isWin = true;
      Console.WriteLine(">>>>>-Wygrałeś!-<<<<<<<");
    }
  }
}


// podsumowanie:
// dodałem wszystkie potrzebne klasy i funkcje
// dodałem obsługę komend
// zaimplementowałem wszystkie rzeczy potrzebne do granie w pasjansa
// gra działa z tego co wiem dobrze
// bardzo się starałem i cieszę się, że skończyłem ten projekt
// Dziękuję za przeczytanie! :]