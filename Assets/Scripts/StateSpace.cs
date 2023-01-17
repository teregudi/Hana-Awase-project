using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateSpace
{
    public List<Card> CardsAtPlayer { get; set; } = new List<Card>();
    public List<Card> CardsAtAI { get; set; } = new List<Card>();
    public List<Card> CardsInMiddle { get; set; } = new List<Card>();
    public List<Card> CardsCollectedByPlayer { get; set; } = new List<Card>();
    public List<Card> CardsCollectedByAI { get; set; } = new List<Card>();

    public int ActualPlayerPoints { get; set; }
    public int ActualAIPonts { get; set; }
}
