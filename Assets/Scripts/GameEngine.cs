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

    public StateSpace State { get; set; } = new StateSpace();
    public int Difficulty { get; set; } = 1;
    public List<Card> Deck { get; set; } = new List<Card>();
    public static List<Card> FULL_DECK { get; set; }
    public List<Card> RedDeck { get; set; } = new List<Card>();
    public bool IsHumanTurn { get; set; } = true;
    public Card DrawnCard { get; private set; } = null;
    public bool playerMustChoose { get; set; } = false;

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

    /// <summary>
    /// parameter 1: the card you want to move
    /// parameter 2: if you move it from middle, mark it true, if from player hand, mark it false
    /// </summary>
    public void MoveCardToPlayerCollection(Card card, bool source)
    {
        if (source)
        {
            State.CardsInMiddle.Remove(card);
            State.CardsCollectedByPlayer.Add(card);
        } else
        {
            State.CardsAtPlayer.Remove(card);
            State.CardsCollectedByPlayer.Add(card);
        }
    }

    public void MoveCardFromPlayerToMiddle(Card card)
    {
        State.CardsAtPlayer.Remove(card);
        State.CardsInMiddle.Add(card);
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

    public List<Card> HandleDrawnCard(Card drawnCard)
    {
        var matchingCards = State.CardsInMiddle.Where(c => c.Id / 10 == drawnCard.Id / 10);
        if (matchingCards.Count() == 1 || matchingCards.Count() == 3)
        {
            foreach (var item in matchingCards)
            {
                State.CardsInMiddle.Remove(item);
                State.CardsCollectedByPlayer.Add(item);
            }
            State.CardsInMiddle.Remove(drawnCard);
            State.CardsCollectedByPlayer.Add(drawnCard);
            return matchingCards.ToList();
        }
        else if (matchingCards.Count() == 2)
        {
            return matchingCards.ToList();
        }
        return new List<Card>();
    }

    public (List<Card>, List<Card>) CalculateAiMove()
    {
        (List<Card> possibleCardsFromHand, List<Card> possibleCardsInMiddle) = GetMatchingCardsOfAi();
        List<Card> collectedCardsFromMiddle = new List<Card>();
        List<Card> discardedCardsToMiddle = new List<Card>();
        if (possibleCardsFromHand.Count == 0)
        {
            //calculate best droppable card
        }
        else if (possibleCardsFromHand.Count > 1)
        {
            //choose card from hand
        }
        else if (possibleCardsInMiddle.Count > 1)
        {
            //choose card from middle
        }
        else
        {
            State.CardsAtAI.Remove(possibleCardsFromHand[0]);
            State.CardsInMiddle.Remove(possibleCardsInMiddle[0]);
            State.CardsCollectedByAI.Add(possibleCardsFromHand[0]);
            State.CardsCollectedByAI.Add(possibleCardsInMiddle[0]);
            collectedCardsFromMiddle.Add(possibleCardsInMiddle[0]);
        }
        //create method for flipping card and handling that flipped card
        return (collectedCardsFromMiddle, discardedCardsToMiddle);
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
