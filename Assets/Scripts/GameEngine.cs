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
    private static StateFactory stateFactory = new StateFactory();
    private static int numberOfCardsToDeal = 8; // ezt talán ki lehet majd törölni
    public static bool endGameAlreadyStarted = false;
    public static bool isZeroSum = true;
    public static int difficulty = 3;
    public static List<Card> FULL_DECK = new List<Card>();
    public static List<Card> RED_DECK = new List<Card>();

    public StateSpace currentState = new StateSpace();
    public List<Card> deck = new List<Card>();
    public Card flippedCard = null;
    public Phase currentPhase = Phase.PLAYER_FROM_HAND;

    private GameEngine() { }

    public static GameEngine GetGameEngine()
    {
        if (singleton == null)
        {
            singleton = new GameEngine();
        }
        return singleton;
    }
    
    public async Task DealCards()
    {
        //TestDeal();
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

    private void TestDeal()
    {
        currentState.CardsAtAI.Add(deck.First(c => c.Id == 84));
        currentState.CardsAtAI.Add(deck.First(c => c.Id == 121));
        currentState.CardsAtAI.Add(deck.First(c => c.Id == 32));
        currentState.CardsAtAI.Add(deck.First(c => c.Id == 81));
        currentState.CardsAtAI.Add(deck.First(c => c.Id == 92));
        currentState.CardsAtAI.Add(deck.First(c => c.Id == 101));
        currentState.CardsAtAI.Add(deck.First(c => c.Id == 93));
        currentState.CardsAtAI.Add(deck.First(c => c.Id == 44));

        deck.Remove(deck.First(c => c.Id == 84));
        deck.Remove(deck.First(c => c.Id == 121));
        deck.Remove(deck.First(c => c.Id == 32));
        deck.Remove(deck.First(c => c.Id == 81));
        deck.Remove(deck.First(c => c.Id == 92));
        deck.Remove(deck.First(c => c.Id == 101));
        deck.Remove(deck.First(c => c.Id == 93));
        deck.Remove(deck.First(c => c.Id == 44));

        currentState.CardsInMiddle.Add(deck.First(c => c.Id == 11));
        currentState.CardsInMiddle.Add(deck.First(c => c.Id == 124));
        currentState.CardsInMiddle.Add(deck.First(c => c.Id == 31));
        currentState.CardsInMiddle.Add(deck.First(c => c.Id == 82));
        currentState.CardsInMiddle.Add(deck.First(c => c.Id == 91));
        currentState.CardsInMiddle.Add(deck.First(c => c.Id == 102));
        currentState.CardsInMiddle.Add(deck.First(c => c.Id == 103));
        currentState.CardsInMiddle.Add(deck.First(c => c.Id == 42));

        deck.Remove(deck.First(c => c.Id == 11));
        deck.Remove(deck.First(c => c.Id == 124));
        deck.Remove(deck.First(c => c.Id == 31));
        deck.Remove(deck.First(c => c.Id == 82));
        deck.Remove(deck.First(c => c.Id == 91));
        deck.Remove(deck.First(c => c.Id == 102));
        deck.Remove(deck.First(c => c.Id == 103));
        deck.Remove(deck.First(c => c.Id == 42));

        currentState.CardsAtPlayer.Add(deck.First(c => c.Id == 12));
        currentState.CardsAtPlayer.Add(deck.First(c => c.Id == 13));
        currentState.CardsAtPlayer.Add(deck.First(c => c.Id == 14));
        currentState.CardsAtPlayer.Add(deck.First(c => c.Id == 111));
        currentState.CardsAtPlayer.Add(deck.First(c => c.Id == 61));
        currentState.CardsAtPlayer.Add(deck.First(c => c.Id == 62));
        currentState.CardsAtPlayer.Add(deck.First(c => c.Id == 123));
        currentState.CardsAtPlayer.Add(deck.First(c => c.Id == 34));

        deck.Remove(deck.First(c => c.Id == 12));
        deck.Remove(deck.First(c => c.Id == 13));
        deck.Remove(deck.First(c => c.Id == 14));
        deck.Remove(deck.First(c => c.Id == 111));
        deck.Remove(deck.First(c => c.Id == 61));
        deck.Remove(deck.First(c => c.Id == 62));
        deck.Remove(deck.First(c => c.Id == 123));
        deck.Remove(deck.First(c => c.Id == 34));
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
        Card card = ConvertGameObjectToCard(cardObject);
        currentState.CardsAtPlayer.Remove(card);
        currentState.CardsCollectedByPlayer.Add(card);
    }

    public void MoveCardFromMiddleToPlayerCollection(GameObject cardObject)
    {
        Card card = ConvertGameObjectToCard(cardObject);
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
        Card card = ConvertGameObjectToCard(cardObject);
        currentState.CardsAtPlayer.Remove(card);
        currentState.CardsInMiddle.Add(card);
    }

    private Card ConvertGameObjectToCard(GameObject cardObject)
    {
        return FULL_DECK.First(c => c.Id == int.Parse(cardObject.name));
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
                Node nodeA = new Node(stateA, NodeType.MIN);
                int scoreA = Expectiminimax.CalculateNodeValue(nodeA, difficulty);

                StateSpace stateB = (StateSpace)currentState.Clone();
                stateB.CardsCollectedByAI.Add(flippedCard);
                stateB.CardsInMiddle.Remove(matchingCards[1]);
                stateB.CardsCollectedByAI.Add(matchingCards[1]);
                Node nodeB = new Node(stateB, NodeType.MIN);
                int scoreB = Expectiminimax.CalculateNodeValue(nodeB, difficulty);

                currentState = scoreA > scoreB ? stateA : stateB;
            }
        }
    }

    public void HandleChoiceAfterFlipByPlayer(GameObject cardObject)
    {
        currentState.CardsCollectedByPlayer.Add(flippedCard);
        Card chosenCard = ConvertGameObjectToCard(cardObject);
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
        Debug.Log("POSSIBLE NODES AND THEIR VALUES");
        for (int i = 0; i < possibleStates.Count; i++)
        {
            Node node = new Node(possibleStates[i], NodeType.CHANCE_AFTER_MAX);
            int actualValue = Expectiminimax.CalculateNodeValue(node, difficulty);
            if (actualValue > highestValue)
            {
                highestValue = actualValue;
                indexOfMostFavorableState = i;
            }
            Debug.Log("NODE " + i);
            DebugNode(possibleStates[i], actualValue);
        }
        Debug.Log("CHOSEN NODE: " + indexOfMostFavorableState);
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

    public void DebugLog()
    {
        StringBuilder sb = new StringBuilder();
        //sb.Append("Cards at player:\n");
        //foreach (var item in State.CardsAtPlayer)
        //{
        //    sb.Append("- " + item);
        //}
        //sb.Append("\nCards at robot:\n");
        //foreach (var item in State.CardsAtAI)
        //{
        //    sb.Append("- " + item);
        //}
        //sb.Append("\nCards in middle:\n");
        //foreach (var item in State.CardsInMiddle)
        //{
        //    sb.Append("- " + item);
        //}
        //Debug.Log(sb.ToString());

        //sb.Append("Cards collected by player: \n");
        //foreach (var card in currentState.CardsCollectedByPlayer)
        //{
        //    sb.Append(card + " - ");
        //}
        //sb.Append("\nCards collected by robot: \n");
        //foreach (var card in currentState.CardsCollectedByAI)
        //{
        //    sb.Append(card + " - ");
        //}

        Debug.Log(sb.ToString());
    }
}
