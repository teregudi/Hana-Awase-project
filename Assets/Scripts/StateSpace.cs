using System;
using System.Collections.Generic;
using System.Linq;

public class StateSpace : ICloneable
{
    private static GameEngine GE = GameEngine.getGE();

    public List<Card> CardsAtPlayer { get; set; } = new List<Card>();
    public List<Card> CardsAtAI { get; set; } = new List<Card>();
    public List<Card> CardsInMiddle { get; set; } = new List<Card>();
    public List<Card> CardsCollectedByPlayer { get; set; } = new List<Card>();
    public List<Card> CardsCollectedByAI { get; set; } = new List<Card>();
    public float Probability { get; set; } = 1;

    public int GetPlayerAdditiveScore()
    {
        return CalculateAdditiveScore(CardsCollectedByPlayer);
    }

    public int GetAiAdditiveScore()
    {
        return PointsForCapturedCards(CardsCollectedByAI) + PointsForYakus(CardsCollectedByAI);
    }

    public int GetPlayerZeroSumScore()
    {
        return CalculateAdditiveScore(CardsCollectedByPlayer) - PointsForYakus(CardsCollectedByAI) - 132;
    }

    public int GetAiZeroSumScore()
    {
        return CalculateAdditiveScore(CardsCollectedByAI) - PointsForYakus(CardsCollectedByPlayer) - 132;
    }

    private int CalculateAdditiveScore(List<Card> cards)
    {
        return PointsForCapturedCards(cards) + PointsForYakus(cards);
    }

    private int PointsForCapturedCards(List<Card> cards)
    {
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
        return (brightCards.Count * 20) + (animalCards.Count * 10) + (ribbonCards.Count * 5) + chaffCards.Count;
    }

    private int PointsForYakus(List<Card> cards)
    {
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
        return CalculateBrightYaku(brightCards, animalCards)
            + CalculateAnimalYaku(animalCards)
            + CalculateRibbonYaku(ribbonCards)
            + CalculateViewingYaku(brightCards, animalCards)
            + CalculateFourOfAKindYaku(cards);
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
                case Month.APRIL:
                    april++;
                    break;
                case Month.NOVEMBER:
                    november++;
                    break;
                case Month.DECEMBER:
                    december++;
                    break;
            }
        }
        score += april == 4 ? 10 : 0;
        score += november == 4 ? 10 : 0;
        score += december == 4 ? 10 : 0;
        return score;
    }

    public object Clone()
    {
        StateSpace clone = (StateSpace)MemberwiseClone();
        clone.CardsAtPlayer = new List<Card>(CardsAtPlayer);
        clone.CardsAtAI = new List<Card>(CardsAtAI);
        clone.CardsInMiddle = new List<Card>(CardsInMiddle);
        clone.CardsCollectedByPlayer = new List<Card>(CardsCollectedByPlayer);
        clone.CardsCollectedByAI = new List<Card>(CardsCollectedByAI);
        clone.Probability = 1;
        return clone;
    }
}
