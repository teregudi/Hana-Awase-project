using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StateSpace : ICloneable
{
    public List<Card> CardsAtPlayer { get; set; } = new List<Card>();
    public List<Card> CardsAtAI { get; set; } = new List<Card>();
    public List<Card> CardsInMiddle { get; set; } = new List<Card>();
    public List<Card> CardsCollectedByPlayer { get; set; } = new List<Card>();
    public List<Card> CardsCollectedByAI { get; set; } = new List<Card>();

    public int PlayerScoreCurrently
    {
        get { return CalculateScore(CardsCollectedByPlayer); }
    }

    public int AiScoreCurrently
    {
        get { return CalculateScore(CardsCollectedByAI); }
    }

    public List<StateSpace> CreatePossibleStates(NodeType type)
    {
        switch (type)
        {
            case NodeType.MAX:
                return CreateStatesByPlayingCard(CardsAtAI, type);
            case NodeType.MIN:
                return CreateStatesByPlayingCard(CardsAtPlayer, type);
            case NodeType.CHANCE_AFTER_MAX:
                return CreateStatesByDrawingFromDeck(CardsCollectedByAI);
            case NodeType.CHANCE_AFTER_MIN:
                return CreateStatesByDrawingFromDeck(CardsCollectedByPlayer);
            default:
                return new List<StateSpace>();
        }
    }

    private int CalculateScore(List<Card> cards)
    {
        int score = 0;
        List<Card> brightCards = new List<Card>();
        List<Card> animalCards = new List<Card>();
        List<Card> ribbonCards = new List<Card>();
        List<Card> chaffCards = new List<Card>();
        foreach (var card in cards)
        {
            switch (card.Type)
            {
                case CardType.BRIGHT:
                    brightCards.Add(card);
                    break;
                case CardType.ANIMAL:
                    animalCards.Add(card);
                    break;
                case CardType.RIBBON:
                    ribbonCards.Add(card);
                    break;
                case CardType.CHAFF:
                    chaffCards.Add(card);
                    break;
            }
        }
        score += (brightCards.Count * 20) + (animalCards.Count * 10) + (ribbonCards.Count * 5) + chaffCards.Count;

        score += CalculateBrightYaku(brightCards, animalCards);
        score += CalculateAnimalYaku(animalCards);
        score += CalculateRibbonYaku(ribbonCards);
        score += CalculateViewingYaku(brightCards, animalCards);
        score += CalculateFourOfAKindYaku(cards);

        return score;
    }

    private int CalculateBrightYaku(List<Card> brights, List<Card> animals)
    {
        if (brights.Count == 5)
            return 100;
        var valuableBrights = brights.Where(b => b.Month != Month.NOVEMBER);
        if (valuableBrights.Count() == 4)
            return 60;
        if (animals.Where(a => a.Month == Month.FEBRUARY).Count() == 1 && brights.Where(b => b.Month == Month.JANUARY || b.Month == Month.MARCH).Count() == 2)
            return 30;
        return brights.Where(b => b.Month == Month.JANUARY || b.Month == Month.AUGUST || b.Month == Month.DECEMBER).Count() == 3 ? 20 : 0;
    }

    private int CalculateAnimalYaku(List<Card> animals)
    {
        return animals.Where(a => a.Month == Month.JUNE || a.Month == Month.JULY || a.Month == Month.OCTOBER).Count() == 3 ? 20 : 0;
    }

    private int CalculateRibbonYaku(List<Card> ribbons)
    {
        var valuableRibbons = ribbons.Where(r => r.Month != Month.NOVEMBER);
        if (valuableRibbons.Count() >= 7)
            return 40;
        int poetry = 0, blue = 0, grass = 0;
        foreach (var ribbon in valuableRibbons)
        {
            if (ribbon.Month == Month.JANUARY || ribbon.Month == Month.FEBRUARY || ribbon.Month == Month.MARCH)
                if (++poetry == 3)
                    return 40;
            else if (ribbon.Month == Month.JUNE || ribbon.Month == Month.SEPTEMBER || ribbon.Month == Month.OCTOBER)
                if (++blue == 3)
                    return 40;
            else if (ribbon.Month == Month.APRIL || ribbon.Month == Month.MAY || ribbon.Month == Month.JULY)
                grass++;
        }
        if (valuableRibbons.Count() == 6)
            return 30;
        if (grass == 3)
            return 20;

        return 0;
    }

    private int CalculateViewingYaku(List<Card> brights, List<Card> animals)
    {
        if (animals.Where(a => a.Month == Month.SEPTEMBER).Count() == 0)
            return 0;
        int sakuraAndSun = brights.Where(b => b.Month == Month.MARCH || b.Month == Month.AUGUST).Count();
        if (sakuraAndSun == 2)
            return 40;
        if (sakuraAndSun == 1)
            return 20;

        return 0;
    }

    private int CalculateFourOfAKindYaku(List<Card> cards)
    {
        int score = 0;
        int april = 0, november = 0, december = 0;
        foreach (var card in cards)
        {
            switch (card.Month)
            {
                case Month.APRIL: april++; break;
                case Month.NOVEMBER: november++; break;
                case Month.DECEMBER: december++; break;
            }
        }
        score += april == 4 ? 10 : 0;
        score += november == 4 ? 10 : 0;
        score += december == 4 ? 10 : 0;
        return score;
    }

    private List<StateSpace> CreateStatesByPlayingCard(List<Card> cardsInHand, NodeType type)
    {
        // ha már elfogytak a lapok a kézbõl
        if (cardsInHand.Count == 0)
            return new List<StateSpace> { (StateSpace)Clone() };

        List<StateSpace> states = new List<StateSpace>();
        foreach (var card in cardsInHand)
        {
            StateSpace clone = (StateSpace)Clone();
            // kiveszi a lapot a kézbõl
            if (type == NodeType.MAX)
                clone.CardsAtAI.Remove(card);
            else if (type == NodeType.MIN)
                clone.CardsAtPlayer.Remove(card);

            var matchingCards = CardsInMiddle.Where(c => c.Month == card.Month);
            // ha nincs match, bedobja középre
            if (matchingCards.Count() == 0)
            {
                clone.CardsInMiddle.Add(card);
                states.Add(clone);
            }
            // ha 1 vagy 3 match van, megy mind a collectionbe
            else if (matchingCards.Count() == 1 || matchingCards.Count() == 3)
            {
                if (type == NodeType.MAX)
                {
                    clone.CardsCollectedByAI.Add(card);
                    clone.CardsCollectedByAI.AddRange(matchingCards);
                }
                else if (type == NodeType.MIN)
                {
                    clone.CardsCollectedByPlayer.Add(card);
                    clone.CardsCollectedByPlayer.AddRange(matchingCards);
                }
                states.Add(clone);
            }
            // ha 2 match van, akkor két külön state keletkezik a választástól függõen
            else if (matchingCards.Count() == 2)
            {
                foreach (var matchingCard in matchingCards)
                {
                    StateSpace cloneVariant = (StateSpace)clone.Clone();
                    if (type == NodeType.MAX)
                    {
                        cloneVariant.CardsCollectedByAI.Add(card);
                        cloneVariant.CardsCollectedByAI.Add(matchingCard);
                    }
                    else if (type == NodeType.MIN)
                    {
                        cloneVariant.CardsCollectedByPlayer.Add(card);
                        cloneVariant.CardsCollectedByPlayer.Add(matchingCard);
                    }
                    states.Add(cloneVariant);
                }
            }
        }
        return states;
    }

    private List<StateSpace> CreateStatesByDrawingFromDeck(List<Card> collectedCards)
    {

    }

    public object Clone()
    {
        StateSpace clone = (StateSpace)MemberwiseClone();
        clone.CardsAtPlayer = new List<Card>(CardsAtPlayer);
        clone.CardsAtAI = new List<Card>(CardsAtAI);
        clone.CardsInMiddle = new List<Card>(CardsInMiddle);
        clone.CardsCollectedByPlayer = new List<Card>(CardsCollectedByPlayer);
        clone.CardsCollectedByAI = new List<Card>(CardsCollectedByAI);
        return clone;
    }
}
