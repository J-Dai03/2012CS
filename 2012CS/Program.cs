// Skeleton Program code for the AQA A Level Paper 1 Summer 2022 examination
//this code should be used in conjunction with the Preliminary Material
//written by the AQA Programmer Team
//developed in the Visual Studio Community Edition programming environment

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Breakthrough
{
    class Program
    {
        static void Main(string[] args)
        {
            Breakthrough ThisGame = new Breakthrough();
            ThisGame.PlayGame();
            Console.ReadLine();
        }
    }

    class Breakthrough
    {
        private static Random RNoGen = new Random();
        private CardCollection Deck;
        private CardCollection Hand;
        private CardCollection Sequence;
        private CardCollection Discard;
        private List<Lock> Locks = new List<Lock>();
        private int Score;
        private bool GameOver;
        private Lock CurrentLock;
        private bool LockSolved;

        public Breakthrough()
        {
            Deck = new CardCollection("DECK");
            Hand = new CardCollection("HAND");
            Sequence = new CardCollection("SEQUENCE");
            Discard = new CardCollection("DISCARD");
            Score = 0;
            LoadLocks();
        }

        public void PlayGame()
        {
            string MenuChoice;
            if (Locks.Count > 0)
            {
                GameOver = false;
                CurrentLock = new Lock();
                SetupGame();
                while (!GameOver)
                {
                    LockSolved = false;
                    while (!LockSolved && !GameOver)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Current score: " + Score);
                        Console.WriteLine(CurrentLock.GetLockDetails());
                        Console.WriteLine(Sequence.GetCardDisplay());
                        Console.WriteLine(Hand.GetCardDisplay());
                        MenuChoice = GetChoice();
                        switch (MenuChoice)
                        {
                            case "D":
                                {
                                    Console.WriteLine(Discard.GetCardDisplay());
                                    break;
                                }
                            case "U":
                                {
                                    int CardChoice = GetCardChoice();
                                    string DiscardOrPlay = GetDiscardOrPlayChoice();
                                    if (DiscardOrPlay == "D")
                                    {
                                        MoveCard(Hand, Discard, Hand.GetCardNumberAt(CardChoice - 1));
                                        GetCardFromDeck(CardChoice);
                                    }
                                    else if (DiscardOrPlay == "P")
                                        PlayCardToSequence(CardChoice);
                                    break;
                                }
                        }
                        if (CurrentLock.GetLockSolved())
                        {
                            LockSolved = true;
                            ProcessLockSolved();
                        }
                    }
                    GameOver = CheckIfPlayerHasLost();
                }
            }
            else
                Console.WriteLine("No locks in file.");
        }

        private void ProcessLockSolved()
        {
            Score += 10;
            Console.WriteLine("Lock has been solved.  Your score is now: " + Score);
            while (Discard.GetNumberOfCards() > 0)
            {
                MoveCard(Discard, Deck, Discard.GetCardNumberAt(0));
            }
            Deck.Shuffle();
            CurrentLock = GetRandomLock();
        }

        private bool CheckIfPlayerHasLost()
        {
            if (Deck.GetNumberOfCards() == 0)
            {
                Console.WriteLine("You have run out of cards in your deck.  Your final score is: " + Score);
                return true;
            }
            else
            {
                return false;
            }
        }

        private void SetupGame()
        {
            string Choice;
            Console.Write("Enter L to load a game from a file, anything else to play a new game:> ");
            Choice = Console.ReadLine().ToUpper();
            if (Choice == "L")
            {
                if (!LoadGame("game1.txt"))
                {
                    GameOver = true;
                }
            }
            else
            {
                CreateStandardDeck();
                Deck.Shuffle();
                for (int Count = 1; Count <= 5; Count++)
                {
                    MoveCard(Deck, Hand, Deck.GetCardNumberAt(0));
                }
                AddDifficultyCardsToDeck();
                Deck.Shuffle();
                CurrentLock = GetRandomLock();
            }
        }

        private void PlayCardToSequence(int cardChoice)
        {
            if (Sequence.GetNumberOfCards() > 0)
            {
                if (Hand.GetCardDescriptionAt(cardChoice - 1)[0] != Sequence.GetCardDescriptionAt(Sequence.GetNumberOfCards() - 1)[0])
                {
                    Score += MoveCard(Hand, Sequence, Hand.GetCardNumberAt(cardChoice - 1));
                    GetCardFromDeck(cardChoice);
                }
            }
            else
            {
                Score += MoveCard(Hand, Sequence, Hand.GetCardNumberAt(cardChoice - 1));
                GetCardFromDeck(cardChoice);
            }
            if (CheckIfLockChallengeMet())
            {
                Console.WriteLine();
                Console.WriteLine("A challenge on the lock has been met.");
                Console.WriteLine();
                Score += 5;
            }
        }

        private bool CheckIfLockChallengeMet()
        {
            string SequenceAsString = "";
            for (int Count = Sequence.GetNumberOfCards() - 1; Count >= Math.Max(0, Sequence.GetNumberOfCards() - 3); Count--)
            {
                if (SequenceAsString.Length > 0)
                {
                    SequenceAsString = ", " + SequenceAsString;
                }
                SequenceAsString = Sequence.GetCardDescriptionAt(Count) + SequenceAsString;
                if (CurrentLock.CheckIfConditionMet(SequenceAsString))
                {
                    return true;
                }
            }
            return false;
        }

        private void SetupCardCollectionFromGameFile(string lineFromFile, CardCollection cardCol)
        {
            List<string> SplitLine;
            int CardNumber;
            if (lineFromFile.Length > 0)
            {
                SplitLine = lineFromFile.Split(',').ToList();
                foreach (var Item in SplitLine)
                {
                    if (Item.Length == 5)
                    {
                        CardNumber = Convert.ToInt32(Item[4]);
                    }
                    else
                    {
                        CardNumber = Convert.ToInt32(Item.Substring(4, 2));
                    }
                    if (Item.Substring(0, 3) == "Dif")
                    {
                        DifficultyCard CurrentCard = new DifficultyCard(CardNumber);
                        cardCol.AddCard(CurrentCard);
                    }
                    else
                    {
                        ToolCard CurrentCard = new ToolCard(Item[0].ToString(), Item[2].ToString(), CardNumber);
                        cardCol.AddCard(CurrentCard);
                    }
                }
            }
        }

        private void SetupLock(string line1, string line2)
        {
            List<string> SplitLine;
            SplitLine = line1.Split(';').ToList();
            foreach (var Item in SplitLine)
            {
                List<string> Conditions;
                Conditions = Item.Split(',').ToList();
                CurrentLock.AddChallenge(Conditions);
            }
            SplitLine = line2.Split(';').ToList();
            for (int Count = 0; Count < SplitLine.Count; Count++)
            {
                if (SplitLine[Count] == "Y")
                {
                    CurrentLock.SetChallengeMet(Count, true);
                }
            }
        }

        private bool LoadGame(string fileName)
        {
            string LineFromFile;
            string LineFromFile2;
            try
            {
                using (StreamReader MyStream = new StreamReader(fileName))
                {
                    LineFromFile = MyStream.ReadLine();
                    Score = Convert.ToInt32(LineFromFile);
                    LineFromFile = MyStream.ReadLine();
                    LineFromFile2 = MyStream.ReadLine();
                    SetupLock(LineFromFile, LineFromFile2);
                    LineFromFile = MyStream.ReadLine();
                    SetupCardCollectionFromGameFile(LineFromFile, Hand);
                    LineFromFile = MyStream.ReadLine();
                    SetupCardCollectionFromGameFile(LineFromFile, Sequence);
                    LineFromFile = MyStream.ReadLine();
                    SetupCardCollectionFromGameFile(LineFromFile, Discard);
                    LineFromFile = MyStream.ReadLine();
                    SetupCardCollectionFromGameFile(LineFromFile, Deck);
                }
                return true;
            }
            catch
            {
                Console.WriteLine("File not loaded");
                return false;
            }
        }

        private void LoadLocks()
        {
            string FileName = "locks.txt";
            string LineFromFile;
            List<string> Challenges;
            Locks = new List<Lock>();
            try
            {
                using (StreamReader MyStream = new StreamReader(FileName))
                {
                    LineFromFile = MyStream.ReadLine();
                    while (LineFromFile != null)
                    {
                        Challenges = LineFromFile.Split(';').ToList();
                        Lock LockFromFile = new Lock();
                        foreach (var C in Challenges)
                        {
                            List<string> Conditions = new List<string>();
                            Conditions = C.Split(',').ToList();
                            LockFromFile.AddChallenge(Conditions);
                        }
                        Locks.Add(LockFromFile);
                        LineFromFile = MyStream.ReadLine();
                    }
                }
            }
            catch
            {
                Console.WriteLine("File not loaded");
            }
        }

        private Lock GetRandomLock()
        {
            return Locks[RNoGen.Next(0, Locks.Count)];
        }

        private void GetCardFromDeck(int cardChoice)
        {
            if (Deck.GetNumberOfCards() > 0)
            {
                if (Deck.GetCardDescriptionAt(0) == "Dif")
                {
                    Card CurrentCard = Deck.RemoveCard(Deck.GetCardNumberAt(0));
                    Console.WriteLine();
                    Console.WriteLine("Difficulty encountered!");
                    Console.WriteLine(Hand.GetCardDisplay());
                    Console.Write("To deal with this you need to either lose a key ");
                    Console.Write("(enter 1-5 to specify position of key) or (D)iscard five cards from the deck:> ");
                    string Choice = Console.ReadLine();
                    Console.WriteLine();
                    Discard.AddCard(CurrentCard);
                    CurrentCard.Process(Deck, Discard, Hand, Sequence, CurrentLock, Choice, cardChoice);
                }
            }
            while (Hand.GetNumberOfCards() < 5 && Deck.GetNumberOfCards() > 0)
            {
                if (Deck.GetCardDescriptionAt(0) == "Dif")
                {
                    MoveCard(Deck, Discard, Deck.GetCardNumberAt(0));
                    Console.WriteLine("A difficulty card was discarded from the deck when refilling the hand.");
                }
                else
                {
                    MoveCard(Deck, Hand, Deck.GetCardNumberAt(0));
                }
            }
            if (Deck.GetNumberOfCards() == 0 && Hand.GetNumberOfCards() < 5)
            {
                GameOver = true;
            }
        }

        private int GetCardChoice()
        {
            string Choice;
            int Value;
            do
            {
                Console.Write("Enter a number between 1 and 5 to specify card to use:> ");
                Choice = Console.ReadLine();
            }
            while (!int.TryParse(Choice, out Value));
            return Value;
        }

        private string GetDiscardOrPlayChoice()
        {
            string Choice;
            Console.Write("(D)iscard or (P)lay?:> ");
            Choice = Console.ReadLine().ToUpper();
            return Choice;
        }

        private string GetChoice()
        {
            Console.WriteLine();
            Console.Write("(D)iscard inspect, (U)se card:> ");
            string Choice = Console.ReadLine().ToUpper();
            return Choice;
        }

        private void AddDifficultyCardsToDeck()
        {
            for (int Count = 1; Count <= 5; Count++)
            {
                Deck.AddCard(new DifficultyCard());
            }
        }

        private void CreateStandardDeck()
        {
            Card NewCard;
            for (int Count = 1; Count <= 5; Count++)
            {
                NewCard = new ToolCard("P", "a");
                Deck.AddCard(NewCard);
                NewCard = new ToolCard("P", "b");
                Deck.AddCard(NewCard);
                NewCard = new ToolCard("P", "c");
                Deck.AddCard(NewCard);
            }
            for (int Count = 1; Count <= 3; Count++)
            {
                NewCard = new ToolCard("F", "a");
                Deck.AddCard(NewCard);
                NewCard = new ToolCard("F", "b");
                Deck.AddCard(NewCard);
                NewCard = new ToolCard("F", "c");
                Deck.AddCard(NewCard);
                NewCard = new ToolCard("K", "a");
                Deck.AddCard(NewCard);
                NewCard = new ToolCard("K", "b");
                Deck.AddCard(NewCard);
                NewCard = new ToolCard("K", "c");
                Deck.AddCard(NewCard);
            }
        }

        private int MoveCard(CardCollection fromCollection, CardCollection toCollection, int cardNumber)
        {
            int Score = 0;
            if (fromCollection.GetName() == "HAND" && toCollection.GetName() == "SEQUENCE")
            {
                Card CardToMove = fromCollection.RemoveCard(cardNumber);
                if (CardToMove != null)
                {
                    toCollection.AddCard(CardToMove);
                    Score = CardToMove.GetScore();
                }
            }
            else
            {
                Card CardToMove = fromCollection.RemoveCard(cardNumber);
                if (CardToMove != null)
                {
                    toCollection.AddCard(CardToMove);
                }
            }
            return Score;
        }
    }

    class Challenge
    {
        /* Consists of a list of strings (Condition) and a bool (Met)
         * Condition represents the sequence needed to beat the challenge
         * Met represents whether or not the challenge has been met
         */
        protected List<string> Condition;
        protected bool Met;

        //A constructor that sets Met to false and doesn't set Condition
        public Challenge()
        {
            Met = false;
        }

        //Get/Set functions for Met and Condition
        public bool GetMet()
        {
            return Met;
        }
        public List<string> GetCondition()
        {
            return Condition;
        }
        public void SetMet(bool newValue)
        {
            Met = newValue;
        }
        public void SetCondition(List<string> newCondition)
        {
            Condition = newCondition;
        }
    }

    class Lock
    {
        //???????
        protected List<Challenge> Challenges = new List<Challenge>();

        public virtual void AddChallenge(List<string> condition)
        {
            Challenge C = new Challenge();
            C.SetCondition(condition);
            Challenges.Add(C);
        }

        private string ConvertConditionToString(List<string> c)
        {
            string ConditionAsString = "";
            for (int Pos = 0; Pos <= c.Count - 2; Pos++)
            {
                ConditionAsString += c[Pos] + ", ";
            }
            ConditionAsString += c[c.Count - 1];
            return ConditionAsString;
        }

        public virtual string GetLockDetails()
        {
            string LockDetails = Environment.NewLine + "CURRENT LOCK" + Environment.NewLine + "------------" + Environment.NewLine;
            foreach (var C in Challenges)
            {
                if (C.GetMet())
                {
                    LockDetails += "Challenge met: ";
                }
                else
                {
                    LockDetails += "Not met:       ";
                }
                LockDetails += ConvertConditionToString(C.GetCondition()) + Environment.NewLine;
            }
            LockDetails += Environment.NewLine;
            return LockDetails;
        }

        public virtual bool GetLockSolved()
        {
            foreach (var C in Challenges)
            {
                if (!C.GetMet())
                {
                    return false;
                }
            }
            return true;
        }

        public virtual bool CheckIfConditionMet(string sequence)
        {
            foreach (var C in Challenges)
            {
                if (!C.GetMet() && sequence == ConvertConditionToString(C.GetCondition()))
                {
                    C.SetMet(true);
                    return true;
                }
            }
            return false;
        }

        public virtual void SetChallengeMet(int pos, bool value)
        {
            Challenges[pos].SetMet(value);
        }

        public virtual bool GetChallengeMet(int pos)
        {
            return Challenges[pos].GetMet();
        }

        public virtual int GetNumberOfChallenges()
        {
            return Challenges.Count;
        }
    }

    class Card
    {
        /* The base class card has 2 integers, CardNumber and Score
         * CardNumber represents the order the card was made, 
            so the 1st card would have a CardNumber of 1, 2nd would have 2, etc
         * Score is zero by default, but can be changed in the ToolCard class
         */
        protected int CardNumber, Score;
        protected static int NextCardNumber = 1;

        //Constructor for the Card class
        //See above for explanation of the variables
        public Card()
        {
            CardNumber = NextCardNumber;
            NextCardNumber += 1;
            Score = 0;
        }

        //Get functions
        public virtual int GetScore()
        {
            return Score;
        }

        public virtual int GetCardNumber()
        {
            return CardNumber;
        }

        // Returns the CardNumber as a string, with 
        // a space before if CardNumber is less than 10
        // i.e. a single digit number
        public virtual string GetDescription()
        {
            if (CardNumber < 10)
            {
                return " " + CardNumber.ToString();
            }
            else
            {
                return CardNumber.ToString();
            }
        }

        /*In the Card base class, this does nothing, 
         * but it is overriden in the DifficultyCard class
         */
        public virtual void Process(CardCollection deck, CardCollection discard,
            CardCollection hand, CardCollection sequence, Lock currentLock,
            string choice, int cardChoice)
        {
        }
    }

    class ToolCard : Card
    {
        /* Has 2 strings, ToolType and Kit
         * ToolType represents what type of tool the ToolCard represents,
         *  So K for key, F for file, P for pick
         * Kit represents which toolkit the ToolCard comes from,
         *  So a for acute, b for basic, c for crude
         */
        protected string ToolType;
        protected string Kit;

        /* Constructor for the ToolCard class
         * Calls the base class of Card to set the CardNumber and Score
         * Takes 2 strings as inputs and uses them to set the ToolType and Kit
         * Uses the SetScore function to reassign Score.
         */
        public ToolCard(string t, string k) : base()
        {
            ToolType = t;
            Kit = k;
            SetScore();
        }

        /* Constructor for the ToolCard class
         * DOES NOT call the base class (Card)
         * Takes 2 strings as inputs and uses them to set the ToolType and Kit
         * Takes an integer and sets the CardNumber as that, rather than using-
           -the base class and incrementing the static variable NextCardNumber
         * Uses the SetScore function to assign Score.
         */
        public ToolCard(string t, string k, int cardNo)
        {
            ToolType = t;
            Kit = k;
            CardNumber = cardNo;
            SetScore();
        }

        //Reassigns score based on ToolType
        //K->3, F->2, P->1
        private void SetScore()
        {
            switch (ToolType)
            {
                case "K":
                    {
                        Score = 3;
                        break;
                    }
                case "F":
                    {
                        Score = 2;
                        break;
                    }
                case "P":
                    {
                        Score = 1;
                        break;
                    }
            }
        }

        /* Returns a string describing the card using ToolType and-
           -Kit with a space between
         * Overrides the function of the same name in the base class (Card)
         */
        public override string GetDescription()
        {
            return ToolType + " " + Kit;
        }

        /*Note: since there is no override for the virtual function Process 
         * from the base class, calling Process will just call the version in-
         * the base class, wichh literally does nothing.
         */
    }

    class DifficultyCard : Card
    {
        //The class has it's own variable, a string called Cardtype
        protected string CardType;

        /* Constructor for the DifficultyCard class
         * Calls the base class of Card to set the CardNumber and Score
         * Sets the variable CardType to "Dif"
         */
        public DifficultyCard()
            : base()
        {
            CardType = "Dif";
        }

        /* Constructor for the DifficultyCard class
         * DOES NOT call the base class (Card)
         * Sets the variable CardType to "Dif"
         * Takes an integer and sets the CardNumber as that, rather than using-
           -the base class and incrementing the static variable NextCardNumber
         */
        public DifficultyCard(int cardNo)
        {
            CardType = "Dif";
            CardNumber = cardNo;
        }

        /* Returns the CardType
         * Overrides the function of the same name in the base class (Card)
         */
        public override string GetDescription()
        {
            return CardType;
        }

        /* Overrides the function defined in the base class (Card) that did nothing
         * Takes five inputs, 4 CardCollections, 1 Lock, 1 string, 1 integer
            * The CardCollections are deck, discard, hand, and sequence
            * The Lock is currentLock
            * The string is choice
            * The integer is cardChoice
         * d
         */
        public override void Process(CardCollection deck, CardCollection discard, CardCollection hand, CardCollection sequence, Lock currentLock, string choice, int cardChoice)
        {
            int ChoiceAsInteger;

            /* Attempts to convert choice to an integer and- 
               -assign the result to ChoiceAsInteger
               It returns True if it succeeded, and false if it failed */
            if (int.TryParse(choice, out ChoiceAsInteger))
            {

                /* If ChoiceAsInteger is between 1 and 5 inclusive, 
                   i.e. a valid postion in the hand */
                if (ChoiceAsInteger >= 1 && ChoiceAsInteger <= 5)
                {

                    /* If ChoiceAsInteger is greater than or equal to-
                       -the input integer cardChoice, decrement it by 1
                     */
                    if (ChoiceAsInteger >= cardChoice)
                    {
                        ChoiceAsInteger -= 1;
                    }

                    /* If ChoiceAsInteger is still greater than zero,
                       decrement it by 1
                     */
                    if (ChoiceAsInteger > 0)
                    {
                        ChoiceAsInteger -= 1;
                    }

                    /* If the first character of the description of the card-
                       -in position ChoiceAsInterger of the CardCollection hand is 'K'
                       i.e.:
                       If the card in position ChoiceAsInterger of hand is a key
                     */
                    if (hand.GetCardDescriptionAt(ChoiceAsInteger)[0] == 'K')
                    {
                        // Move the card in position ChoiceAsInterger of hand to discard
                        Card CardToMove = hand.RemoveCard(hand.GetCardNumberAt(ChoiceAsInteger));
                        discard.AddCard(CardToMove);
                        // And end the function early
                        return;
                    }
                }
            }
            // If you haven't ended the function early
            /* Move the top 5 cards in the deck to the discard pile, 
              stopping early if there are no more cards in the deck
             */
            int Count = 0;
            while (Count < 5 && deck.GetNumberOfCards() > 0)
            {
                Card CardToMove = deck.RemoveCard(deck.GetCardNumberAt(0));
                discard.AddCard(CardToMove);
                Count += 1;
            }
        }
    }

    class CardCollection
    {
        // Consists of a list of cards and a string desccribing the name
        protected List<Card> Cards = new List<Card>();
        protected string Name;

        // Class constructor - sets Name to the input
        public CardCollection(string n)
        {
            Name = n;
        }

        // Get functions
        public string GetName()
        {
            return Name;
        }
        public int GetNumberOfCards()
        {
            return Cards.Count;
        }
        /* These functions get the data of the card in position x in-
           -the (zero-based) list
         */
        public int GetCardNumberAt(int x)
        {
            return Cards[x].GetCardNumber();
        }
        public string GetCardDescriptionAt(int x)
        {
            return Cards[x].GetDescription();
        }

        // Adds the card given as an argument to the list of cards
        public void AddCard(Card c)
        {
            Cards.Add(c);
        }
        
        // Shuffles the cards in the list
        /* It does this by picking 2 positions in 
         * the list and swapping the cards in those 
         * positions, and repeating this 10000 times
         */
        public void Shuffle()
        {
            Random RNoGen = new Random();
            Card TempCard;
            int RNo1, RNo2;
            for (int Count = 1; Count <= 10000; Count++)
            {
                RNo1 = RNoGen.Next(0, Cards.Count);
                RNo2 = RNoGen.Next(0, Cards.Count);
                TempCard = Cards[RNo1];
                Cards[RNo1] = Cards[RNo2];
                Cards[RNo2] = TempCard;
            }
        }

        // Pops the first instance of the card with the value equal to the input.
        // If there is no card with that value in the list, null is returned.
        public Card RemoveCard(int cardNumber)
        {
            bool CardFound = false;
            int Pos = 0;
            Card CardToGet = null;
            while (Pos < Cards.Count && !CardFound)
            {
                if (Cards[Pos].GetCardNumber() == cardNumber)
                {
                    CardToGet = Cards[Pos];
                    CardFound = true;
                    Cards.RemoveAt(Pos);
                }
                Pos++;
            }
            return CardToGet;
        }

        //Returns a string consisting of 6 times the input number of dashes
        private string CreateLineOfDashes(int size)
        {
            string LineOfDashes = "";
            for (int Count = 1; Count <= size; Count++)
            {
                LineOfDashes += "------";
            }
            return LineOfDashes;
        }

        /* Returns a multi-line string describing: 
         *  contents of the variable Name
         *  the descriptions of the cards in the list Cards
         */
        public string GetCardDisplay()
        {
            /* Sets CardDisplay to a string consisting of 2 lines, 
             * The first line is empty
             * The second line consists of the contents of the variable Name, 
              * followed by a colon
             */
            string CardDisplay = Environment.NewLine + Name + ":";

            // If the list of cards, Cards, is empty
            if (Cards.Count == 0)
            {
                /* Returns a string consisting of 4 lines,
                 * The first, third, and fourth lines are empty
                 * The second line consists of the contents of the variable Name, 
                  * followed by ": empty"
                 */
                return CardDisplay + " empty" + Environment.NewLine + Environment.NewLine;
            }
            else
            {
                // Adds 2 more new lines to the string
                CardDisplay += Environment.NewLine + Environment.NewLine;
            }

            //Creates string variable LineOfDashes,
            //and declares the constant CardsPerLine,
            //which is the maximum number of lines per row
            string LineOfDashes;
            const int CardsPerLine = 10;

            /* If there are more cards in the list than the 
             * maximum number of cards allowed per line, 
             *  set the LineOfDashes to a string of dashes 
             *  based on the maximum number of cards allowed in a line
             * Else, 
             *  set the LineOfDashes to a string of dashes 
             *  based on the number of cards in the list
             */
            if (Cards.Count > CardsPerLine)
            {
                LineOfDashes = CreateLineOfDashes(CardsPerLine);
            }
            else
            {
                LineOfDashes = CreateLineOfDashes(Cards.Count);
            }

            /*Adds the contents of the LineOfDashes function and 
             * a new line to CardToDisplay
             */
            CardDisplay += LineOfDashes + Environment.NewLine;

            /* Adds the card descriptions to CardDisplay
             * The card descriptions are seperated such that 
             * the maximum number of descriptions per line is CardsPerLine
             */
            bool Complete = false;
            int Pos = 0;
            while (!Complete)
            {
                CardDisplay += "| " + Cards[Pos].GetDescription() + " ";
                Pos++;

                /*----------------------------
                // Splits the card descriptions such that
                // the maximum number of descriptions per line is CardsPerLine
                */
                if (Pos % CardsPerLine == 0)
                {
                    CardDisplay += "|" + Environment.NewLine + LineOfDashes + Environment.NewLine;
                }
                //----------------------------

                // If every position has been considered, 
                // set complete to true to break the while loop.
                if (Pos == Cards.Count)
                {
                    Complete = true;
                }
            }

            /* If the number of cards in the list Cards 
             * is not a multiple of the maximum number of cards in a line:
             */
            if (Cards.Count % CardsPerLine > 0)
            {
                // Then add a closing | followed by a new line
                CardDisplay += "|" + Environment.NewLine;

                // If the are more cards in the list than can fit on one line:
                if (Cards.Count > CardsPerLine)
                {
                    //Reassign LineOfDashes to
                    LineOfDashes = CreateLineOfDashes(Cards.Count % CardsPerLine);
                }

                //Adds LineOfDashes and a new line to CardDisplay
                /* Note: if the previous if statement was not triggered 
                 * and LineOfDashes was not reassigned, 
                 * then the contents of LineOfDashes are determined by 
                 * the if (Cards.Count > CardsPerLine){...} else{...} code block, 
                 * where I commented:
                 *  If there are more cards in the list than the maximum number of
                 *  cards allowed per line, 
                 *      set the LineOfDashes to a string of dashes
                 *      based on the maximum number of cards allowed in a line
                 *  Else, 
                 *      set the LineOfDashes to a string of dashes 
                 *      based on the number of cards in the list
                 */           
                CardDisplay += LineOfDashes + Environment.NewLine;
            }

            //Returns the contents of CardDisplay
            return CardDisplay;
        }
    }
}