using Assets.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GameEngine
{
    private static GameEngine singleton = null;
    private static StateFactory stateFactory = new StateFactory();
    public static bool endGameAlreadyStarted = false;

    public StateSpace State { get; set; } = new StateSpace();
    public int Difficulty { get; set; } = 6;
    public int NumberOfCards { get; set; } = 8;
    public List<Card> Deck { get; set; } = new List<Card>();
    public static List<Card> FULL_DECK { get; set; } = new List<Card>();
    public List<Card> RED_DECK { get; set; } = new List<Card>();
    public Card DrawnCard { get; set; } = null;
    public Phase Phase { get; set; } = Phase.PLAYER_FROM_HAND;

    private GameEngine() { }

    public static GameEngine getGE()
    {
        if (singleton == null)
        {
            singleton = new GameEngine();
        }
        return singleton;
    }

    private void ShuffleCards()
    {
        int counter = Deck.Count;
        while (counter > 1)
        {
            counter--;
            int index = UnityEngine.Random.Range(0, counter + 1);
            Card replace = Deck[index];
            Deck[index] = Deck[counter];
            Deck[counter] = replace;
        }
    }
    
    public async Task DealCards()
    {
        bool fourSame;
        do
        {
            ShuffleCards();
            State.CardsAtPlayer.Clear();
            State.CardsAtAI.Clear();
            State.CardsInMiddle.Clear();
            for (int i = 0; i < NumberOfCards; i++)
            {
                State.CardsAtPlayer.Add(Deck[i]);
            }
            for (int i = NumberOfCards; i < NumberOfCards * 2; i++)
            {
                State.CardsAtAI.Add(Deck[i]);
            }
            for (int i = NumberOfCards * 2; i < NumberOfCards * 3; i++)
            {
                State.CardsInMiddle.Add(Deck[i]);
            }
            fourSame = (CheckIfTheresFourSame(State.CardsAtPlayer) ||
                CheckIfTheresFourSame(State.CardsAtAI) ||
                CheckIfTheresFourSame(State.CardsInMiddle));
        } while (fourSame);

        Deck = Deck.GetRange(NumberOfCards * 3, Deck.Count - (NumberOfCards * 3));

        await Task.Delay(1);
    }

    private bool CheckIfTheresFourSame(IEnumerable<Card> cards)
    {
        int[] monthArray = new int[12];
        foreach (var item in cards)
        {
            monthArray[item.Id/10 - 1]++;
        }
        return monthArray.Contains(4);
    }

    public void MoveCardFromPlayerToCollection(GameObject cardObject)
    {
        Card card = State.CardsAtPlayer.First(c => c.Id == int.Parse(cardObject.name));
        State.CardsAtPlayer.Remove(card);
        State.CardsCollectedByPlayer.Add(card);
    }

    public void MoveCardFromMiddleToPlayerCollection(GameObject cardObject)
    {
        Card card = State.CardsInMiddle.First(c => c.Id == int.Parse(cardObject.name));
        var matchingCards = State.CardsInMiddle.Where(c => c.Month == card.Month);
        if (matchingCards.Count() < 3)
        {
            State.CardsCollectedByPlayer.Add(card);
            State.CardsInMiddle.Remove(card);
        }
        else
        {
            foreach (var matchingCard in matchingCards)
            {
                State.CardsCollectedByPlayer.Add(matchingCard);
                State.CardsInMiddle.Remove(matchingCard);
            }
        }
    }

    public void MoveCardFromPlayerToMiddle(GameObject cardObject)
    {
        Card card = State.CardsAtPlayer.First(c => c.Id == int.Parse(cardObject.name));
        State.CardsAtPlayer.Remove(card);
        State.CardsInMiddle.Add(card);
    }

    //private Card ConvertGameObjectToCard(GameObject cardObject)
    //{
    //    return FULL_DECK.First(c => c.Id == int.Parse(cardObject.name));
    //}

    public void DrawCard()
    {
        DrawnCard = Deck[0];
        Deck.RemoveAt(0);
    }

    public void HandleDrawnCard()
    {
        var matchingCards = State.CardsInMiddle.Where(c => c.Month == DrawnCard.Month).ToList();
        if (matchingCards.Count() == 0)
        {
            State.CardsInMiddle.Add(DrawnCard);
        }
        else if (matchingCards.Count() == 1 || matchingCards.Count() == 3)
        {
            State.CardsInMiddle.RemoveAll(m => m.Month == DrawnCard.Month);
            if (Phase == Phase.AI_TURN)
            {
                State.CardsCollectedByAI.Add(DrawnCard);
                foreach (var matchingCard in matchingCards)
                {
                    State.CardsCollectedByAI.Add(matchingCard);
                }
            }
            else
            {
                State.CardsCollectedByPlayer.Add(DrawnCard);
                foreach (var matchingCard in matchingCards)
                {
                    State.CardsCollectedByPlayer.Add(matchingCard);
                }
            }
        }
        else if (matchingCards.Count() == 2 && Phase == Phase.AI_TURN)
        {
            StateSpace stateA = (StateSpace)State.Clone();
            stateA.CardsInMiddle.Remove(matchingCards[0]);
            Node nodeA = new Node(stateA, NodeType.MIN);
            int scoreA = Expectiminimax.CalculateNodeValue(nodeA, Difficulty);

            StateSpace stateB = (StateSpace)State.Clone();
            stateB.CardsInMiddle.Remove(matchingCards[1]);
            Node nodeB = new Node(stateB, NodeType.MIN);
            int scoreB = Expectiminimax.CalculateNodeValue(nodeB, Difficulty);

            State = scoreA > scoreB ? stateA : stateB;
        }
    }

    public void HandleChoiceAfterDraw(GameObject cardObject)
    {
        State.CardsCollectedByPlayer.Add(DrawnCard);
        Card chosenCard = State.CardsInMiddle.First(c => c.Id == int.Parse(cardObject.name));
        State.CardsCollectedByPlayer.Add(chosenCard);
        State.CardsInMiddle.Remove(chosenCard);
    }

    public IEnumerable<Card> CalculateAiMoveFromHand()
    {
        stateFactory.InitialState = (StateSpace)State.Clone();
        stateFactory.NodeType = NodeType.MAX;
        List<StateSpace> possibleStates = stateFactory.CreatePossibleStates();
        int indexOfMostFavorableState = 0;
        int highestValue = int.MinValue;
        for (int i = 0; i < possibleStates.Count; i++)
        {
            Node node = new Node(possibleStates[i], NodeType.CHANCE_AFTER_MAX);
            int actualValue = Expectiminimax.CalculateNodeValue(node, Difficulty);
            if (actualValue > highestValue)
            {
                highestValue = actualValue;
                indexOfMostFavorableState = i;
            }
        }
        List<Card> collectedFromMiddle = new List<Card>();
        foreach (var c1 in State.CardsInMiddle)
        {
            bool match = false;
            foreach (var c2 in possibleStates[indexOfMostFavorableState].CardsInMiddle)
            {
                if (c1 == c2)
                    match = true;
            }
            if (!match)
                collectedFromMiddle.Add(c1);
        }
        //var collectedFromMiddle = possibleStates[indexOfMostFavorableState].CardsCollectedByAI.Where(c => State.CardsInMiddle.Contains(c)).ToList();
        State = possibleStates[indexOfMostFavorableState];
        return collectedFromMiddle;
    }

    //public void DebugLog()
    //{
    //    StringBuilder sb = new StringBuilder();
    //    sb.Append("Cards at player:\n");
    //    foreach (var item in State.CardsAtPlayer)
    //    {
    //        sb.Append("- " + item);
    //    }
    //    sb.Append("\nCards at robot:\n");
    //    foreach (var item in State.CardsAtAI)
    //    {
    //        sb.Append("- " + item);
    //    }
    //    sb.Append("\nCards in middle:\n");
    //    foreach (var item in State.CardsInMiddle)
    //    {
    //        sb.Append("- " + item);
    //    }
    //    Debug.Log(sb.ToString());
    //}
}
