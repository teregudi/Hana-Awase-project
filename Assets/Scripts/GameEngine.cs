using Assets.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GameEngine
{
    private static GameEngine singleton = null;
    private static StateFactory stateFactory;
    public static int numberOfCardsToDeal;
    public static bool endGameAlreadyStarted;
    public static bool isZeroSum;
    public static int difficulty;
    public static List<Card> FULL_DECK;
    public static List<Card> RED_DECK;

    public StateSpace currentState;
    public List<Card> deck;
    public Card flippedCard;
    public Phase currentPhase;

    private GameEngine() { }

    public static GameEngine GetGameEngine()
    {
        if (singleton == null)
        {
            singleton = new GameEngine();
        }
        return singleton;
    }

    private static void StaticReset()
    {
        stateFactory = new StateFactory();
        numberOfCardsToDeal = 8;
        endGameAlreadyStarted = false;
        FULL_DECK = new List<Card>();
        RED_DECK = new List<Card>();
    }

    public void Reset()
    {
        StaticReset();
        currentState = new StateSpace();
        deck = new List<Card>();
        flippedCard = null;
        currentPhase = Phase.PLAYER_FROM_HAND;
    }

    
    public async Task DealCards()
    {
        bool fourSame;
        do
        {
            ShuffleCards();
            currentState.CardsAtPlayer.Clear();
            currentState.CardsAtAI.Clear();
            currentState.CardsInMiddle.Clear();
            for (int i = 0; i < numberOfCardsToDeal; i++)
            {
                currentState.CardsAtPlayer.Add(deck[i]);
            }
            for (int i = numberOfCardsToDeal; i < numberOfCardsToDeal * 2; i++)
            {
                currentState.CardsAtAI.Add(deck[i]);
            }
            for (int i = numberOfCardsToDeal * 2; i < numberOfCardsToDeal * 3; i++)
            {
                currentState.CardsInMiddle.Add(deck[i]);
            }
            fourSame = (CheckIfTheresFourSame(currentState.CardsAtPlayer) ||
                CheckIfTheresFourSame(currentState.CardsAtAI) ||
                CheckIfTheresFourSame(currentState.CardsInMiddle));
        } while (fourSame);

        deck = deck.GetRange(numberOfCardsToDeal * 3, deck.Count - (numberOfCardsToDeal * 3));

        await Task.Delay(1);
    }

    public void CopyDeck()
    {
        foreach (var item in deck)
        {
            FULL_DECK.Add(new Card(item.FrontPic, item.Id, item.Type, item.Month));
        }
    }

    private void ShuffleCards()
    {
        int counter = deck.Count;
        while (counter > 1)
        {
            counter--;
            int index = UnityEngine.Random.Range(0, counter + 1);
            Card replace = deck[index];
            deck[index] = deck[counter];
            deck[counter] = replace;
        }
    }

    private bool CheckIfTheresFourSame(List<Card> cards)
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
        Card card = currentState.CardsAtPlayer.First(c => c.Id == int.Parse(cardObject.name));
        currentState.CardsAtPlayer.Remove(card);
        currentState.CardsCollectedByPlayer.Add(card);
    }

    public void MoveCardFromMiddleToPlayerCollection(GameObject cardObject)
    {
        Card card = currentState.CardsInMiddle.First(c => c.Id == int.Parse(cardObject.name));
        var matchingCards = currentState.CardsInMiddle.Where(c => c.Month == card.Month).ToList();
        if (matchingCards.Count() < 3)
        {
            currentState.CardsCollectedByPlayer.Add(card);
            currentState.CardsInMiddle.Remove(card);
        }
        else
        {
            foreach (var matchingCard in matchingCards)
            {
                currentState.CardsCollectedByPlayer.Add(matchingCard);
                currentState.CardsInMiddle.Remove(matchingCard);
            }
        }
    }

    public void MoveCardFromPlayerToMiddle(GameObject cardObject)
    {
        Card card = currentState.CardsAtPlayer.First(c => c.Id == int.Parse(cardObject.name));
        currentState.CardsAtPlayer.Remove(card);
        currentState.CardsInMiddle.Add(card);
    }

    public void FlipTopCard()
    {
        flippedCard = deck[0];
        deck.RemoveAt(0);
    }

    public void HandleFlippedCard()
    {
        var matchingCards = currentState.CardsInMiddle.Where(c => c.Month == flippedCard.Month).ToList();
        if (matchingCards.Count() == 0)
        {
            currentState.CardsInMiddle.Add(flippedCard);
        }
        else if (matchingCards.Count() == 1 || matchingCards.Count() == 3)
        {
            currentState.CardsInMiddle.RemoveAll(m => m.Month == flippedCard.Month);
            if (currentPhase == Phase.AI_TURN)
            {
                currentState.CardsCollectedByAI.Add(flippedCard);
                foreach (var matchingCard in matchingCards)
                {
                    currentState.CardsCollectedByAI.Add(matchingCard);
                }
            }
            else
            {
                currentState.CardsCollectedByPlayer.Add(flippedCard);
                foreach (var matchingCard in matchingCards)
                {
                    currentState.CardsCollectedByPlayer.Add(matchingCard);
                }
            }
        }
        else if (matchingCards.Count() == 2 && currentPhase == Phase.AI_TURN)
        {
            if (matchingCards[0].Type == matchingCards[1].Type)
            {
                currentState.CardsCollectedByAI.Add(flippedCard);
                currentState.CardsInMiddle.Remove(matchingCards[0]);
                currentState.CardsCollectedByAI.Add(matchingCards[0]);
            }
            else
            {
                StateSpace stateA = (StateSpace)currentState.Clone();
                stateA.CardsCollectedByAI.Add(flippedCard);
                stateA.CardsInMiddle.Remove(matchingCards[0]);
                stateA.CardsCollectedByAI.Add(matchingCards[0]);
                Node nodeA = new Node(stateA, NodeType.MIN, 1);
                int scoreA = Expectiminimax.CalculateNodeValue(nodeA, difficulty, int.MinValue, int.MaxValue);

                StateSpace stateB = (StateSpace)currentState.Clone();
                stateB.CardsCollectedByAI.Add(flippedCard);
                stateB.CardsInMiddle.Remove(matchingCards[1]);
                stateB.CardsCollectedByAI.Add(matchingCards[1]);
                Node nodeB = new Node(stateB, NodeType.MIN, 1);
                int scoreB = Expectiminimax.CalculateNodeValue(nodeB, difficulty, int.MinValue, int.MaxValue);

                currentState = scoreA > scoreB ? stateA : stateB;
            }
        }
    }

    public void HandleChoiceAfterFlipByPlayer(GameObject cardObject)
    {
        currentState.CardsCollectedByPlayer.Add(flippedCard);
        Card chosenCard = currentState.CardsInMiddle.First(c => c.Id == int.Parse(cardObject.name));
        currentState.CardsCollectedByPlayer.Add(chosenCard);
        currentState.CardsInMiddle.Remove(chosenCard);
    }

    public (Card, IEnumerable<Card>) CalculateAiMoveFromHand()
    {
        stateFactory.InitialState = (StateSpace)currentState.Clone();
        stateFactory.NodeType = NodeType.MAX;
        List<StateSpace> possibleStates = stateFactory.CreatePossibleStates();
        int indexOfMostFavorableState = 0;
        int highestValue = int.MinValue;
        Expectiminimax.counter = 0;
        Debug.Log("POSSIBLE NODES AND THEIR VALUES");
        for (int i = 0; i < possibleStates.Count; i++)
        {
            Node node = new Node(possibleStates[i], NodeType.CHANCE_AFTER_MAX, 1);
            int actualValue = Expectiminimax.CalculateNodeValue(node, difficulty, int.MinValue, int.MaxValue);
            if (actualValue > highestValue)
            {
                highestValue = actualValue;
                indexOfMostFavorableState = i;
            }
            Debug.Log("NODE " + i);
            DebugNode(possibleStates[i], actualValue);
        }
        Debug.Log("CHOSEN NODE: " + indexOfMostFavorableState);
        Debug.Log("Calculated nodes: " + Expectiminimax.counter);
        Expectiminimax.counter = 0;
        Card playedCard = currentState.CardsAtAI.First(c => !possibleStates[indexOfMostFavorableState].CardsAtAI.Contains(c));
        var collectedFromMiddle = currentState.CardsInMiddle.Where(c => possibleStates[indexOfMostFavorableState].CardsCollectedByAI.Contains(c));
        currentState = possibleStates[indexOfMostFavorableState];
        return (playedCard, collectedFromMiddle);
    }

    public void DebugNode(StateSpace state, int value)
    {
        Card playedCard = currentState.CardsAtAI.First(c => !state.CardsAtAI.Contains(c));
        var collectedCards = state.CardsCollectedByAI.Where(c => !currentState.CardsCollectedByAI.Contains(c));
        StringBuilder sb = new StringBuilder();
        sb.Append("PLAYED: " + playedCard + ", COLLECTED: ");
        foreach (var card in collectedCards)
        {
            sb.Append(card + ", ");
        }
        sb.Append("VALUE: " + value);
        Debug.Log(sb.ToString());
    }
}
