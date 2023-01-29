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

    public StateSpace State { get; set; } = new StateSpace();
    public int Difficulty { get; set; } = 1;
    public List<Card> Deck { get; set; } = new List<Card>();
    public static List<Card> FULL_DECK { get; set; } = new List<Card>();
    public List<Card> RED_DECK { get; set; } = new List<Card>();
    public bool IsHumanTurn { get; set; } = true;
    public Card DrawnCard { get; set; } = null;
    public bool playerMustChoose { get; set; } = false;
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
            for (int i = 0; i < 8; i++)
            {
                State.CardsAtPlayer.Add(Deck[i]);
            }
            for (int i = 8; i < 16; i++)
            {
                State.CardsAtAI.Add(Deck[i]);
            }
            for (int i = 16; i < 24; i++)
            {
                State.CardsInMiddle.Add(Deck[i]);
            }
            fourSame = (CheckIfTheresFourSame(State.CardsAtPlayer) ||
                CheckIfTheresFourSame(State.CardsAtAI) ||
                CheckIfTheresFourSame(State.CardsInMiddle));
        } while (fourSame);

        Deck = Deck.GetRange(24, Deck.Count-24);

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
        Card card = ConvertGameObjectToCard(cardObject);
        State.CardsAtPlayer.Remove(card);
        State.CardsCollectedByPlayer.Add(card);
    }

    public void MoveCardFromMiddleToPlayerCollection(GameObject cardObject)
    {
        Card card = ConvertGameObjectToCard(cardObject);
        var matchingCards = State.CardsInMiddle.Where(c => c.Month == card.Month);
        if (matchingCards.Count() == 2)
        {
            State.CardsCollectedByPlayer.Add(card);
        }
        else
        {
            // check if it works fine
            State.CardsCollectedByPlayer.AddRange(matchingCards);
        }
    }

    public void MoveCardFromPlayerToMiddle(GameObject cardObject)
    {
        Card card = ConvertGameObjectToCard(cardObject);
        State.CardsAtPlayer.Remove(card);
        State.CardsInMiddle.Add(card);
    }

    private Card ConvertGameObjectToCard(GameObject cardObject)
    {
        return FULL_DECK.First(c => c.Id == int.Parse(cardObject.name));
    }

    public void MoveDrawnCardToMiddle()
    {
        State.CardsInMiddle.Add(DrawnCard);
        DrawnCard = null;
    }

    public void MoveDrawnCardToPlayerCollection()
    {
        State.CardsCollectedByPlayer.Add(DrawnCard);
        DrawnCard = null;
    }

    public void DrawCard()
    {
        DrawnCard = Deck[0];
        Deck.RemoveAt(0);
    }

    public void HandleDrawnCard()
    {
        var matchingCards = State.CardsInMiddle.Where(c => c.Month == DrawnCard.Month);
        if (matchingCards.Count() == 0)
        {
            State.CardsInMiddle.Add(DrawnCard);
        }
        if (matchingCards.Count() == 1 || matchingCards.Count() == 3)
        {
            State.CardsInMiddle.RemoveAll(m => m.Month == DrawnCard.Month);
            if (Phase == Phase.AI_TURN)
            {
                State.CardsCollectedByAI.Add(DrawnCard);
                State.CardsCollectedByAI.AddRange(matchingCards);
            }
            else
            {
                State.CardsCollectedByPlayer.Add(DrawnCard);
                State.CardsCollectedByPlayer.AddRange(matchingCards);
            }
        }
    }

    public StateSpace HandleTheoreticalDrawnCard()
    {
        StateSpace modifiedState = (StateSpace)State.Clone();
        var matchingCards = modifiedState.CardsInMiddle.Where(c => c.Month == DrawnCard.Month);
        if (matchingCards.Count() == 0)
        {
            modifiedState.CardsInMiddle.Add(DrawnCard);
        }
        else if (matchingCards.Count() == 1 || matchingCards.Count() == 3)
        {
            modifiedState.CardsInMiddle.RemoveAll(m => m.Month == DrawnCard.Month);
            modifiedState.CardsCollectedByAI.Add(DrawnCard);
            modifiedState.CardsCollectedByAI.AddRange(matchingCards);
        }
        else if (matchingCards.Count() == 2)
        {
            StateSpace state1 = (StateSpace)State.Clone();
            state1.CardsCollectedByAI.Add(DrawnCard);
            state1.CardsCollectedByAI.Add(matchingCards.ElementAt(0));
            state1.CardsInMiddle.Remove(matchingCards.ElementAt(0));
            Node node1 = new Node(state1, NodeType.MIN);

            StateSpace state2 = (StateSpace)State.Clone();
            state2.CardsCollectedByAI.Add(DrawnCard);
            state2.CardsCollectedByAI.Add(matchingCards.ElementAt(1));
            state2.CardsInMiddle.Remove(matchingCards.ElementAt(1));
            Node node2 = new Node(state2, NodeType.MIN);

            modifiedState = node1.Value > node2.Value ? state1 : state2;
        }
        DrawnCard = null;
        return modifiedState;
    }

    public (IEnumerable<Card>, IEnumerable<Card>, IEnumerable<Card>) CalculateAiMove()
    {
        //(List<Card> possibleCardsFromHand, List<Card> possibleCardsInMiddle) = GetMatchingCardsOfAi();
        //List<Card> collectedCardsFromMiddle = new List<Card>();
        //List<Card> discardedCardsToMiddle = new List<Card>();
        //List<Card> collectedCardsFromHand = new List<Card>();

        stateFactory.InitialState = (StateSpace)State.Clone();
        stateFactory.NodeType = NodeType.MAX;
        List<StateSpace> possibleStates = stateFactory.CreatePossibleStates();
        StateSpace bestOption = possibleStates.First();
        int bestValue = int.MinValue;
        foreach (var possibleState in possibleStates)
        {
            Node node = new Node(possibleState, NodeType.CHANCE_AFTER_MAX);
            int actualValue = Expectiminimax.CalculateNodeValue(node, Difficulty);
            if (actualValue > bestValue)
            {
                bestValue = actualValue;
                bestOption = possibleState;
            }
        }

        var collectedCardsFromMiddle = bestOption.CardsCollectedByAI.Where(c => State.CardsInMiddle.Contains(c)).ToList();
        var collectedCardsFromHand = bestOption.CardsCollectedByAI.Where(c => State.CardsAtAI.Contains(c)).ToList();
        var newCardsInMiddle = bestOption.CardsInMiddle.Where(c => !State.CardsInMiddle.Contains(c)).ToList();


        Debug.Log("BEFORE DRAWING");
        foreach (var card in collectedCardsFromMiddle)
            Debug.Log($"Collected card from middle: {card}");
        foreach (var card in collectedCardsFromHand)
            Debug.Log($"Collected card from hand: {card}");
        foreach (var card in newCardsInMiddle)
            Debug.Log($"New card in middle: {card}");


        State = bestOption;
        DrawCard();
        StateSpace modifiedState = HandleTheoreticalDrawnCard();
        collectedCardsFromMiddle.AddRange(modifiedState.CardsCollectedByAI.Where(c => State.CardsInMiddle.Contains(c)));
        newCardsInMiddle.AddRange(modifiedState.CardsInMiddle.Where(c => !State.CardsInMiddle.Contains(c)));
        State = modifiedState;

        Debug.Log("AFTER DRAWING");
        foreach (var card in collectedCardsFromMiddle)
            Debug.Log($"Collected card from middle: {card}");
        foreach (var card in collectedCardsFromHand)
            Debug.Log($"Collected card from hand: {card}");
        foreach (var card in newCardsInMiddle)
            Debug.Log($"New card in middle: {card}");


        /*
        if (possibleCardsFromHand.Count == 0)
        {
            stateFactory.InitialState = (StateSpace)State.Clone();
            stateFactory.NodeType = NodeType.MAX;
            List<StateSpace> possibleStates = stateFactory.CreatePossibleStates();
            StateSpace bestOption = possibleStates.First();
            int bestValue = int.MinValue;
            foreach (var possibleState in possibleStates)
            {
                Node node = new Node(possibleState, NodeType.MAX);
                int actualValue = Expectiminimax.CalculateNodeValue(node, 1);
                if (actualValue > bestValue)
                {
                    bestValue = actualValue;
                    bestOption = possibleState;
                }
            }
            Card droppedCard = bestOption.CardsInMiddle.Where(c => !State.CardsInMiddle.Contains(c)).First();
            State.CardsInMiddle.Add(droppedCard);
            discardedCardsToMiddle.Add(droppedCard);
        }
        else if (possibleCardsFromHand.Count > 1)
        {
            stateFactory.InitialState = (StateSpace)State.Clone();
            stateFactory.NodeType = NodeType.MAX;
            List<StateSpace> possibleStates = stateFactory.CreatePossibleStates();
            //choose card from hand
        }
        else if (possibleCardsInMiddle.Count > 1)
        {
            //choose card from middle
        }
        else
        {
            State.CardsAtAI.Remove(possibleCardsFromHand[0]);
            State.CardsInMiddle.Remove(possibleCardsInMiddle[0]); //itt még nincs lekezelve, hogy ha középről hármat gyűjt be
            State.CardsCollectedByAI.Add(possibleCardsFromHand[0]);
            State.CardsCollectedByAI.Add(possibleCardsInMiddle[0]);
            collectedCardsFromMiddle.Add(possibleCardsInMiddle[0]);
            collectedCardsFromHand.Add(possibleCardsFromHand[0]);
        }
        */
        //create method for flipping card and handling that flipped card
        return (collectedCardsFromMiddle, newCardsInMiddle, collectedCardsFromHand);
    }

    private (List<Card>, List<Card>) GetMatchingCardsOfAi()
    {
        List<Card> possibleAiCards = new List<Card>();
        List<Card> matchingMiddleCards = new List<Card>();
        foreach (var aiCard in State.CardsAtAI)
        {
            foreach (var middleCard in State.CardsInMiddle)
            {
                if (aiCard.Month == middleCard.Month)
                {
                    if (!possibleAiCards.Contains(aiCard))
                    {
                        possibleAiCards.Add(aiCard);
                    }
                    if (!matchingMiddleCards.Contains(middleCard))
                    { 
                        matchingMiddleCards.Add(middleCard);
                    }
                }
            }
        }
        return (possibleAiCards, matchingMiddleCards);
    }
}
