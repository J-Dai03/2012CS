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

            /* Some code that would allow you to replay the game without having to restart the program
            
            Breakthrough ThisGame = new Breakthrough();
            while (true)
            {
                ThisGame.PlayGame();
                Console.ReadLine();
            }
             */
        }
    }

    class Breakthrough
    {
        /*Note: each lock in the game can  be thought of as a level*/

        //This is a static variable used to generate random numbers
        private static Random RNoGen = new Random();

        //These are self explanatory
        private CardCollection Deck;
        private CardCollection Hand;
        private CardCollection Sequence;
        private CardCollection Discard;
        private int Score;
        private Lock CurrentLock;

        // Stores all the locks in a list
        private List<Lock> Locks = new List<Lock>();

        // Stores whether the game should continue
        private bool GameOver;

        // Stores whether the current lock is solved or not
        private bool LockSolved;

        //The constructor for the Breakthrough class.
        /* Note: the variable Locks is assigned when 
         * it is intialised, rather than in the constructor.
         * However, it gets items added to it in the LoadLocks
         * function, which is called in the constructor.
         */
        public Breakthrough()
        {
            Deck = new CardCollection("DECK");
            Hand = new CardCollection("HAND");
            Sequence = new CardCollection("SEQUENCE");
            Discard = new CardCollection("DISCARD");
            Score = 0;
            LoadLocks();
        }

        //Self explanatory - plays the game
        public void PlayGame()
        {
            string MenuChoice;

            //If Locks is not empty, i.e. if it loaded properly
            if (Locks.Count > 0)
            {
                //Sets up the game
                //Why these 2 lines weren't included in the SetUpGame function is beyond me
                GameOver = false;
                CurrentLock = new Lock();
                SetupGame();

                //While GameOver is false
                while (!GameOver)
                {
                    LockSolved = false;

                    //This while loop is only continues when 
                    //  LockSolved is false 
                    //  and GameOver is false
                    while (!LockSolved && !GameOver)
                    {
                        //Displays data
                        Console.WriteLine();
                        Console.WriteLine("Current score: " + Score);
                        Console.WriteLine(CurrentLock.GetLockDetails());
                        Console.WriteLine(Sequence.GetCardDisplay());
                        Console.WriteLine(Hand.GetCardDisplay());

                        //Gets the player's choice of action
                        MenuChoice = GetChoice();

                        switch (MenuChoice)
                        {
                            //When D is chosen, the Discard is displayed
                            case "D":
                                {
                                    Console.WriteLine(Discard.GetCardDisplay());
                                    break;
                                }
                            //When U is chosen, a card is selected, then discarded or played
                            case "U":
                                {
                                    //Gets the card chosen, or rather the postion of the card chosen
                                    int CardChoice = GetCardChoice();

                                    //Gets whether the card is to be discarded for played, in the form of D or P
                                    string DiscardOrPlay = GetDiscardOrPlayChoice();

                                    if (DiscardOrPlay == "D")
                                    {
                                        MoveCard(Hand, Discard, Hand.GetCardNumberAt(CardChoice - 1));
                                        GetCardFromDeck(CardChoice);
                                    }
                                    else if (DiscardOrPlay == "P")
                                        PlayCardToSequence(CardChoice);
                                        //PlayCardToSequence has GetCardFromDeck built in, 
                                        //so it doesn't need to be called separately
                                    break;
                                }
                        }

                        //Once the player's actions have been run, it checks if the CurrentLock has been solved, 
                        //And if so, it will change the bool LockSolved to true and run ProcessLockSolved
                        if (CurrentLock.GetLockSolved())
                        {
                            LockSolved = true;

                            //ProcessLockSolved does several simple things, check its commenting
                            ProcessLockSolved();
                        }
                    }

                    //Checks if the game is over
                    GameOver = CheckIfPlayerHasLost();
                }
            }
            else
                Console.WriteLine("No locks in file.");
        }

        /* Does the stuff that needs to be done once a lock is solved. 
         * These are:
            * Adding 10 to the Score
            * Telling the player that the lock has been solved and outputting their score
            * Moving the cards in the Discard pile to the Deck then reshuffling the Deck
            * Loading a new Random lock
         */
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

        // Returns a bool depending on whether or not the player has lost. 
        // Will output a nessage to the console if they have
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

        //Sets up the game
        //Checks if you want to load a game from file or start a new one
        private void SetupGame()
        {
            string Choice;
            Console.Write("Enter L to load a game from a file, anything else to play a new game:> ");
            Choice = Console.ReadLine().ToUpper();
            if (Choice == "L")
            {
                //If it fails to load, GameOver is set to true
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

        /* Takes an integer input and attempts to play a card to the sequence
         * Note that cardChoice uses a ONE-BASED SYSTEM, which needs to be compensated*/
        private void PlayCardToSequence(int cardChoice)
        {
            //if the Sequence is not empty
            if (Sequence.GetNumberOfCards() > 0)
            {
                /* If the first character of the card they are trying to play 
                 *  and of the last item in the Sequence are NOT the same, 
                 * i.e. If the card they are trying to play and 
                 *  the last item of the Sequence DO NOT have the same ToolType
                 */
                if (Hand.GetCardDescriptionAt(cardChoice - 1)[0] != Sequence.GetCardDescriptionAt(Sequence.GetNumberOfCards() - 1)[0])
                {
                    //Play the card and draw the replacement
                    Score += MoveCard(Hand, Sequence, Hand.GetCardNumberAt(cardChoice - 1));
                    GetCardFromDeck(cardChoice);
                }
            }

            else
            {
                //Play the card and draw the replacement
                Score += MoveCard(Hand, Sequence, Hand.GetCardNumberAt(cardChoice - 1));
                GetCardFromDeck(cardChoice);
            }

            //Self explanatory - If a challenge has been met, it tells the player and adds 5 to the score
            if (CheckIfLockChallengeMet())
            {
                Console.WriteLine();
                Console.WriteLine("A challenge on the lock has been met.");
                Console.WriteLine();
                Score += 5;
            }
        }

        //Returns a bool that tells you if a lock challenge has been met using the last 1, 2, or 3 cards
        private bool CheckIfLockChallengeMet()
        {
            string SequenceAsString = "";

            /* Runs through the sequence from back to front, 
             * stopping when 3 items have been tested 
             * or when you have reached the front of the sequence
             */
            for (int Count = Sequence.GetNumberOfCards() - 1; Count >= Math.Max(0, Sequence.GetNumberOfCards() - 3); Count--)
            {
                /*if the Sequence so far has been greater than zero, 
                 * a seperating comma and space is added to the front of SequenceAsString
                 */
                if (SequenceAsString.Length > 0)
                {
                    SequenceAsString = ", " + SequenceAsString;
                }

                //The Card Description of the position we are currently considering is added to the front of SequenceAsString
                SequenceAsString = Sequence.GetCardDescriptionAt(Count) + SequenceAsString;

                //Checks if the current version of SequenceAsString can be used to matches the condition of the CurrentLock
                if (CurrentLock.CheckIfConditionMet(SequenceAsString))
                {
                    return true;
                }
            }
            return false;
        }

        /*Takes a string that represents a list of cards and a CardCollection, 
         * and fills the CardCollection with those cards.
         * The string's format: A series of cards seperated by commas
         * The card format: The ToolType, the ToolKit, then the CardNumber, each seperatede by a space
         */
        private void SetupCardCollectionFromGameFile(string lineFromFile, CardCollection cardCol)
        {
            //A list of strings that stores lineFromFile after it has been split into is components
            List<string> SplitLine;
            int CardNumber;

            // if the string is not empty
            if (lineFromFile.Length > 0)
            {
                //Splits the line into a list of strings
                SplitLine = lineFromFile.Split(',').ToList();
                foreach (var Item in SplitLine)
                {
                    //If the string is 5 characters long, the CardNumber is the character in the 5th position
                    //Else, the CardNumber is the characters in the 5th and 6th positions
                    if (Item.Length == 5)
                    {
                        CardNumber = Convert.ToInt32(Item[4]);
                    }
                    else
                    {
                        CardNumber = Convert.ToInt32(Item.Substring(4, 2));
                    }

                    /* If the  first 3 characters are "Dif", 
                        * a difficulty card is created with the previously found CardNumber 
                        * and added to the CardCollection
                     * Else, 
                        * a ToolCard is created with the previously found CardNumber
                        * and added to the CardCollection
                     */
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

        /* Sets up the CurrentLock
         * Takes 2 inputs
            * line1 is the lock and its challenges,
            * line2 represents which challenges have and haven't been completed
         * 
         */
        private void SetupLock(string line1, string line2)
        {
            //represents the current line you are examining-
            //-after it has been split up into a list of strings
            List<string> SplitLine;

            //Splits up line1 into strings, each one representing a challenge
            SplitLine = line1.Split(';').ToList();

            /* Splits each challenge-representing-string into 
             * its consituent conditions and adds the challenge to the lock
             */
            foreach (var Item in SplitLine)
            {
                List<string> Conditions;
                Conditions = Item.Split(',').ToList();
                CurrentLock.AddChallenge(Conditions);
            }

            // Splits up line2 into strings, 
                //each one representing a challenge's completetion status
            // Y means the challenge has been completed, N means it has not
            SplitLine = line2.Split(';').ToList();

            //Applies to completion statuses
            for (int Count = 0; Count < SplitLine.Count; Count++)
            {
                if (SplitLine[Count] == "Y")
                {
                    CurrentLock.SetChallengeMet(Count, true);
                }
            }
        }

        //Loads data from the file
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

        // Loads the locks represented in locks.txt into the Locks variable
        // See the section on locks.txt in the shared file for more details
        // Also see the sections about LoadLocks for more detailed explanations
        private void LoadLocks()
        {
            /* This is the name of the file to load. Using this variable makes it easier to modify to load different files*/
            string FileName = "locks.txt";
            // Stores the current line of the file that you are processing
            string LineFromFile;
            // Stores the challenges on the current lock you are processing
            List<string> Challenges;
            // Assigns the variable lock
            Locks = new List<Lock>();
            /* Trys to run the code in the the try block until an error occurs, 
             * in which case it switches to the catch block, 
             * which just outputs an error message in the console
             */
            try
            {
                //The using code block means that MyStream can't be 
                //accessed from outside the codeblock
                using (StreamReader MyStream = new StreamReader(FileName))
                {
                    //Sets LineFromFile as the first line from the file
                    LineFromFile = MyStream.ReadLine();
                    while (LineFromFile != null)
                    {
                        // Splits the line into strings representing the challenges
                        Challenges = LineFromFile.Split(';').ToList();
                        // Creates a new Lock variable called LockFromFile, 
                        // used to hold the current lock to add to Locks
                        Lock LockFromFile = new Lock();
                        //For each challenge
                        foreach (var C in Challenges)
                        {
                            // Creates a list of strings representing the conditions of the challenge
                            List<string> Conditions = new List<string>();
                            Conditions = C.Split(',').ToList();
                            //Adds a challenge containing all the 
                            LockFromFile.AddChallenge(Conditions);
                        }
                        //Adds the newly created lock to Locks
                        Locks.Add(LockFromFile);
                        //Loads the next line into LineFromFile
                        /*Note that the last line, which is empty, 
                         * causes LineFromFile to be set to null,
                         * rather than an empty string for some reason
                         */
                        LineFromFile = MyStream.ReadLine();
                    }
                }
                //Here, I can't access MyStream
            }
            catch
            {
                Console.WriteLine("File not loaded");
            }
        }

        //Returns a random Lock from Locks
        private Lock GetRandomLock()
        {
            return Locks[RNoGen.Next(0, Locks.Count)];
        }

        //Gets the card from the deck
        /* Deals with:
            * drawing a difficulty cards, 
            * refilling the hand, 
            * and when you run out of cards in the deck
         */
        private void GetCardFromDeck(int cardChoice)
        {
            //If the deck isn't empty
            if (Deck.GetNumberOfCards() > 0)
            {
                //If the next card is a difficulty card
                if (Deck.GetCardDescriptionAt(0) == "Dif")
                {
                    // Removes the difficulty card,
                    Card CurrentCard = Deck.RemoveCard(Deck.GetCardNumberAt(0));

                    //Tells the player a difficulty has been encountered
                    Console.WriteLine();
                    Console.WriteLine("Difficulty encountered!");

                    //Shows the player their hand
                    Console.WriteLine(Hand.GetCardDisplay());

                    // Tells the player to specify a position of a key 
                    // or to discard 5 cards, 
                    //  then takes the input
                    Console.Write("To deal with this you need to either lose a key ");
                    Console.Write("(enter 1-5 to specify position of key) or (D)iscard five cards from the deck:> ");
                    string Choice = Console.ReadLine();
                    Console.WriteLine();

                    //Moves the difficulty card to the discard
                    Discard.AddCard(CurrentCard);

                    //Deals with drawing a difficulty card
                    CurrentCard.Process(Deck, Discard, Hand, Sequence, CurrentLock, Choice, cardChoice);
                }
            }

            //While the Hand does not have enough cards, and the deck isn't empty, keep trying to refill the hand
            while (Hand.GetNumberOfCards() < 5 && Deck.GetNumberOfCards() > 0)
            {
                //If the card you would move from the deck to the hand is a difficulty card,
                // move it to the Discard instead, and tell the player
                //Else, move it to the Hand
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

            //If the Deck is empty and the Hand still isn't full, the game is over
            if (Deck.GetNumberOfCards() == 0 && Hand.GetNumberOfCards() < 5)
            {
                GameOver = true;
            }
        }

        /* Outputs a prompt to enter a number specifying which card to use,
         * and returns the input after converting it to an integer.
         * Will keep asking if the input cannot be converted to an integer, 
         * but not if it is out of bounds
         */
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

        /* Outputs a prompt to enter a choice between discarding or playing a card,
         * and returns the input after capitalising it
         */
        private string GetDiscardOrPlayChoice()
        {
            string Choice;
            Console.Write("(D)iscard or (P)lay?:> ");
            Choice = Console.ReadLine().ToUpper();
            return Choice;
        }

        /* Outputs a prompt to enter a choice between inspecting the discard and using a card,
         * and returns the input after capitalising it
         */
        private string GetChoice()
        {
            Console.WriteLine();
            Console.Write("(D)iscard inspect, (U)se card:> ");
            string Choice = Console.ReadLine().ToUpper();
            return Choice;
        }

        //Self explanantory - adds 5 Difficulty cards to the deck
        private void AddDifficultyCardsToDeck()
        {
            for (int Count = 1; Count <= 5; Count++)
            {
                Deck.AddCard(new DifficultyCard());
            }
        }

        /* Adds the cards that would fill Deck when Deck is empty
         * The cards added are:
         *  15 picks, 5 from each kit
         *  9 files, 3 from each kit
         *  9 keys, 3 from each kit
         */
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

        /* Moves the first instance of the card with the input cardNumber
         * in fromCollection to toCollection
         * If you are moving a card from the Hand to the sequence, it will return the score of the card moved
         * If not, it returns 0.
         */
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
        //No given constructor, Challenges is assigned when it is initialised
        protected List<Challenge> Challenges = new List<Challenge>();

        /* Takes a List of strings and adds a challenge with  
         * that List of strings as its condition to Challenges
         */
        public virtual void AddChallenge(List<string> condition)
        {
            Challenge C = new Challenge();
            C.SetCondition(condition);
            Challenges.Add(C);
        }

        /* Takes a List of strings and returns a string consisting of 
         * the elements of the input list, seperated by ", "
         */
        private string ConvertConditionToString(List<string> c)
        {
            string ConditionAsString = "";
            /* Uses c.Count - 2 because we don't want to add a 
             * seperator after the last element, and they 
             * are using <= rather than < in the condition
             */
            for (int Pos = 0; Pos <= c.Count - 2; Pos++)
            {
                ConditionAsString += c[Pos] + ", ";
            }
            ConditionAsString += c[c.Count - 1];
            return ConditionAsString;
        }

        // Takes a string and returns a bool. 
        // If the string is the condition of one of the challenges,
        // it sets the met condition to true and returns true
        // If not, it returns false. 
        // Note that if the Challenge has already been met, it will not be tested
        public virtual bool CheckIfConditionMet(string sequence)
        {
            foreach (var C in Challenges)
            {
                //Won't return true if it is the current challenge being examined has already been met
                if (!C.GetMet() && sequence == ConvertConditionToString(C.GetCondition()))
                {
                    C.SetMet(true);
                    return true;
                }
            }
            return false;
        }

        /*Get/Set functions*/
        // Returns a string describing which challneges have and haven't been met
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
        // Returns a bool. If the lock is solved, it returns true, if not, false. 
        /* Works by iterating through each challenge and 
         * returning false if any of their Met boolean variable are false*/
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
        //Sets the Met bool of the the challenge in position 'pos' to 'value'
        public virtual void SetChallengeMet(int pos, bool value)
        {
            Challenges[pos].SetMet(value);
        }
        //Gets the Met bool of the the challenge in position 'pos'
        public virtual bool GetChallengeMet(int pos)
        {
            return Challenges[pos].GetMet();
        }
        //Gets the number of challenges by taking the length of Challenges
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
         * Calls the base class (Card) without explicitly saying it, 
         *  since it is an inherited class
         *  Calls the constructor with no parameters, which is the one described
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
         * Calls the base class of Card to set the CardNumber and Score
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

        /* Handles what happens if a difficulty card is drawn.
         * Overrides the function defined in the base class (Card) that did nothing
         * Takes five inputs, 4 CardCollections, 1 Lock, 1 string, 1 integer
            * The CardCollections are deck, discard, hand, and sequence
            * The Lock is currentLock
            * The string is choice
                * choice is the (1 based) position of the card the player has chosen to discard, 
                *   OR choice is "D" to say the player wants to Discard a card
                * Note: In reality:
                    * If choice can't be converted to an integer that is between 1 and 5 inclusive,
                        * The early return function will not be called and 
                          the code will progress to the discard a card bit of code
            * The integer is cardChoice
                * cardChoice is the position of the card that was played before you drew to replace it
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

                    /* If ChoiceAsInteger is greater than or equal to the input integer cardChoice, 
                        * decrement it by 1
                     * This is because the card you wanted to dicard was moved to the left by one 
                        * by the playing of the card
                     */
                    if (ChoiceAsInteger >= cardChoice)
                    {
                        ChoiceAsInteger -= 1;
                    }

                    /* If ChoiceAsInteger is still greater than zero,
                       decrement it by 1, 
                     * This is because the input system is one-based, 
                        * but the rest of it is zero-based
                     */
                    if (ChoiceAsInteger > 0)
                    {
                        ChoiceAsInteger -= 1;
                    }

                    /* If the first character of the description of the card-
                       -in position ChoiceAsInteger of the CardCollection hand is 'K'
                       i.e.:
                     * If the card in position ChoiceAsInterger of hand is a key
                     */
                    if (hand.GetCardDescriptionAt(ChoiceAsInteger)[0] == 'K')
                    {
                        // Moves the card in position ChoiceAsInterger of Hand to the Discard
                        Card CardToMove = hand.RemoveCard(hand.GetCardNumberAt(ChoiceAsInteger));
                        discard.AddCard(CardToMove);

                        // And ends the function early
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
        // Consists of a list of cards and a string describing the name
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

        // Pops the first instance of the card with a CardNumber equal to the input.
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