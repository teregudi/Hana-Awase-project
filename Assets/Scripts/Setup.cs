using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class Setup : MonoBehaviour
{
    public GameObject CardPrefab;
    public GameObject PlayerArea;
    public GameObject MiddleArea;
    public GameObject OpponentArea;
    public GameObject DeckArea;

    public Texture BlackBack;

    public Texture January1;
    public Texture January2;
    public Texture January3;
    public Texture January4;
    public Texture February1;
    public Texture February2;
    public Texture February3;
    public Texture February4;
    public Texture March1;
    public Texture March2;
    public Texture March3;
    public Texture March4;
    public Texture April1;
    public Texture April2;
    public Texture April3;
    public Texture April4;
    public Texture May1;
    public Texture May2;
    public Texture May3;
    public Texture May4;
    public Texture June1;
    public Texture June2;
    public Texture June3;
    public Texture June4;
    public Texture July1;
    public Texture July2;
    public Texture July3;
    public Texture July4;
    public Texture August1;
    public Texture August2;
    public Texture August3;
    public Texture August4;
    public Texture September1;
    public Texture September2;
    public Texture September3;
    public Texture September4;
    public Texture October1;
    public Texture October2;
    public Texture October3;
    public Texture October4;
    public Texture November1;
    public Texture November2;
    public Texture November3;
    public Texture November4;
    public Texture December1;
    public Texture December2;
    public Texture December3;
    public Texture December4;

    public Texture January1Red;
    public Texture January2Red;
    public Texture January3Red;
    public Texture January4Red;
    public Texture February1Red;
    public Texture February2Red;
    public Texture February3Red;
    public Texture February4Red;
    public Texture March1Red;
    public Texture March2Red;
    public Texture March3Red;
    public Texture March4Red;
    public Texture April1Red;
    public Texture April2Red;
    public Texture April3Red;
    public Texture April4Red;
    public Texture May1Red;
    public Texture May2Red;
    public Texture May3Red;
    public Texture May4Red;
    public Texture June1Red;
    public Texture June2Red;
    public Texture June3Red;
    public Texture June4Red;
    public Texture July1Red;
    public Texture July2Red;
    public Texture July3Red;
    public Texture July4Red;
    public Texture August1Red;
    public Texture August2Red;
    public Texture August3Red;
    public Texture August4Red;
    public Texture September1Red;
    public Texture September2Red;
    public Texture September3Red;
    public Texture September4Red;
    public Texture October1Red;
    public Texture October2Red;
    public Texture October3Red;
    public Texture October4Red;
    public Texture November1Red;
    public Texture November2Red;
    public Texture November3Red;
    public Texture November4Red;
    public Texture December1Red;
    public Texture December2Red;
    public Texture December3Red;
    public Texture December4Red;

    public GameEngine GE;
    private static bool boolToAvoidMoreInstantiation = true;

    public async void Start()
    {
        Debug.Log("Game started.");

        if (boolToAvoidMoreInstantiation)
        {
            boolToAvoidMoreInstantiation = false;

            GE = GameEngine.getGE();

            GE.Deck.Add(new Card(January1, 11, CardType.BRIGHT, Month.JANUARY));
            GE.Deck.Add(new Card(January2, 12, CardType.RIBBON, Month.JANUARY));
            GE.Deck.Add(new Card(January3, 13, CardType.CHAFF, Month.JANUARY));
            GE.Deck.Add(new Card(January4, 14, CardType.CHAFF, Month.JANUARY));
            GE.Deck.Add(new Card(February1, 21, CardType.ANIMAL, Month.FEBRUARY));
            GE.Deck.Add(new Card(February2, 22, CardType.RIBBON, Month.FEBRUARY));
            GE.Deck.Add(new Card(February3, 23, CardType.CHAFF, Month.FEBRUARY));
            GE.Deck.Add(new Card(February4, 24, CardType.CHAFF, Month.FEBRUARY));
            GE.Deck.Add(new Card(March1, 31, CardType.BRIGHT, Month.MARCH));
            GE.Deck.Add(new Card(March2, 32, CardType.RIBBON, Month.MARCH));
            GE.Deck.Add(new Card(March3, 33, CardType.CHAFF, Month.MARCH));
            GE.Deck.Add(new Card(March4, 34, CardType.CHAFF, Month.MARCH));
            GE.Deck.Add(new Card(April1, 41, CardType.ANIMAL, Month.APRIL));
            GE.Deck.Add(new Card(April2, 42, CardType.RIBBON, Month.APRIL));
            GE.Deck.Add(new Card(April3, 43, CardType.CHAFF, Month.APRIL));
            GE.Deck.Add(new Card(April4, 44, CardType.CHAFF, Month.APRIL));
            GE.Deck.Add(new Card(May1, 51, CardType.ANIMAL, Month.MAY));
            GE.Deck.Add(new Card(May2, 52, CardType.RIBBON, Month.MAY));
            GE.Deck.Add(new Card(May3, 53, CardType.CHAFF, Month.MAY));
            GE.Deck.Add(new Card(May4, 54, CardType.CHAFF, Month.MAY));
            GE.Deck.Add(new Card(June1, 61, CardType.ANIMAL, Month.JUNE));
            GE.Deck.Add(new Card(June2, 62, CardType.RIBBON, Month.JUNE));
            GE.Deck.Add(new Card(June3, 63, CardType.CHAFF, Month.JUNE));
            GE.Deck.Add(new Card(June4, 64, CardType.CHAFF, Month.JUNE));
            GE.Deck.Add(new Card(July1, 71, CardType.ANIMAL, Month.JULY));
            GE.Deck.Add(new Card(July2, 72, CardType.RIBBON, Month.JULY));
            GE.Deck.Add(new Card(July3, 73, CardType.CHAFF, Month.JULY));
            GE.Deck.Add(new Card(July4, 74, CardType.CHAFF, Month.JULY));
            GE.Deck.Add(new Card(August1, 81, CardType.BRIGHT, Month.AUGUST));
            GE.Deck.Add(new Card(August2, 82, CardType.ANIMAL, Month.AUGUST));
            GE.Deck.Add(new Card(August3, 83, CardType.CHAFF, Month.AUGUST));
            GE.Deck.Add(new Card(August4, 84, CardType.CHAFF, Month.AUGUST));
            GE.Deck.Add(new Card(September1, 91, CardType.ANIMAL, Month.SEPTEMBER));
            GE.Deck.Add(new Card(September2, 92, CardType.RIBBON, Month.SEPTEMBER));
            GE.Deck.Add(new Card(September3, 93, CardType.CHAFF, Month.SEPTEMBER));
            GE.Deck.Add(new Card(September4, 94, CardType.CHAFF, Month.SEPTEMBER));
            GE.Deck.Add(new Card(October1, 101, CardType.ANIMAL, Month.OCTOBER));
            GE.Deck.Add(new Card(October2, 102, CardType.RIBBON, Month.OCTOBER));
            GE.Deck.Add(new Card(October3, 103, CardType.CHAFF, Month.OCTOBER));
            GE.Deck.Add(new Card(October4, 104, CardType.CHAFF, Month.OCTOBER));
            GE.Deck.Add(new Card(November1, 111, CardType.BRIGHT, Month.NOVEMBER));
            GE.Deck.Add(new Card(November2, 112, CardType.ANIMAL, Month.NOVEMBER));
            GE.Deck.Add(new Card(November3, 113, CardType.RIBBON, Month.NOVEMBER));
            GE.Deck.Add(new Card(November4, 114, CardType.CHAFF, Month.NOVEMBER));
            GE.Deck.Add(new Card(December1, 121, CardType.BRIGHT, Month.DECEMBER));
            GE.Deck.Add(new Card(December2, 122, CardType.CHAFF, Month.DECEMBER));
            GE.Deck.Add(new Card(December3, 123, CardType.CHAFF, Month.DECEMBER));
            GE.Deck.Add(new Card(December4, 124, CardType.CHAFF, Month.DECEMBER));

            GE.RED_DECK.Add(new Card(January1Red, 11, CardType.BRIGHT, Month.JANUARY));
            GE.RED_DECK.Add(new Card(January2Red, 12, CardType.RIBBON, Month.JANUARY));
            GE.RED_DECK.Add(new Card(January3Red, 13, CardType.CHAFF, Month.JANUARY));
            GE.RED_DECK.Add(new Card(January4Red, 14, CardType.CHAFF, Month.JANUARY));
            GE.RED_DECK.Add(new Card(February1Red, 21, CardType.ANIMAL, Month.FEBRUARY));
            GE.RED_DECK.Add(new Card(February2Red, 22, CardType.RIBBON, Month.FEBRUARY));
            GE.RED_DECK.Add(new Card(February3Red, 23, CardType.CHAFF, Month.FEBRUARY));
            GE.RED_DECK.Add(new Card(February4Red, 24, CardType.CHAFF, Month.FEBRUARY));
            GE.RED_DECK.Add(new Card(March1Red, 31, CardType.BRIGHT, Month.MARCH));
            GE.RED_DECK.Add(new Card(March2Red, 32, CardType.RIBBON, Month.MARCH));
            GE.RED_DECK.Add(new Card(March3Red, 33, CardType.CHAFF, Month.MARCH));
            GE.RED_DECK.Add(new Card(March4Red, 34, CardType.CHAFF, Month.MARCH));
            GE.RED_DECK.Add(new Card(April1Red, 41, CardType.ANIMAL, Month.APRIL));
            GE.RED_DECK.Add(new Card(April2Red, 42, CardType.RIBBON, Month.APRIL));
            GE.RED_DECK.Add(new Card(April3Red, 43, CardType.CHAFF, Month.APRIL));
            GE.RED_DECK.Add(new Card(April4Red, 44, CardType.CHAFF, Month.APRIL));
            GE.RED_DECK.Add(new Card(May1Red, 51, CardType.ANIMAL, Month.MAY));
            GE.RED_DECK.Add(new Card(May2Red, 52, CardType.RIBBON, Month.MAY));
            GE.RED_DECK.Add(new Card(May3Red, 53, CardType.CHAFF, Month.MAY));
            GE.RED_DECK.Add(new Card(May4Red, 54, CardType.CHAFF, Month.MAY));
            GE.RED_DECK.Add(new Card(June1Red, 61, CardType.ANIMAL, Month.JUNE));
            GE.RED_DECK.Add(new Card(June2Red, 62, CardType.RIBBON, Month.JUNE));
            GE.RED_DECK.Add(new Card(June3Red, 63, CardType.CHAFF, Month.JUNE));
            GE.RED_DECK.Add(new Card(June4Red, 64, CardType.CHAFF, Month.JUNE));
            GE.RED_DECK.Add(new Card(July1Red, 71, CardType.ANIMAL, Month.JULY));
            GE.RED_DECK.Add(new Card(July2Red, 72, CardType.RIBBON, Month.JULY));
            GE.RED_DECK.Add(new Card(July3Red, 73, CardType.CHAFF, Month.JULY));
            GE.RED_DECK.Add(new Card(July4Red, 74, CardType.CHAFF, Month.JULY));
            GE.RED_DECK.Add(new Card(August1Red, 81, CardType.BRIGHT, Month.AUGUST));
            GE.RED_DECK.Add(new Card(August2Red, 82, CardType.ANIMAL, Month.AUGUST));
            GE.RED_DECK.Add(new Card(August3Red, 83, CardType.CHAFF, Month.AUGUST));
            GE.RED_DECK.Add(new Card(August4Red, 84, CardType.CHAFF, Month.AUGUST));
            GE.RED_DECK.Add(new Card(September1Red, 91, CardType.ANIMAL, Month.SEPTEMBER));
            GE.RED_DECK.Add(new Card(September2Red, 92, CardType.RIBBON, Month.SEPTEMBER));
            GE.RED_DECK.Add(new Card(September3Red, 93, CardType.CHAFF, Month.SEPTEMBER));
            GE.RED_DECK.Add(new Card(September4Red, 94, CardType.CHAFF, Month.SEPTEMBER));
            GE.RED_DECK.Add(new Card(October1Red, 101, CardType.ANIMAL, Month.OCTOBER));
            GE.RED_DECK.Add(new Card(October2Red, 102, CardType.RIBBON, Month.OCTOBER));
            GE.RED_DECK.Add(new Card(October3Red, 103, CardType.CHAFF, Month.OCTOBER));
            GE.RED_DECK.Add(new Card(October4Red, 104, CardType.CHAFF, Month.OCTOBER));
            GE.RED_DECK.Add(new Card(November1Red, 111, CardType.BRIGHT, Month.NOVEMBER));
            GE.RED_DECK.Add(new Card(November2Red, 112, CardType.ANIMAL, Month.NOVEMBER));
            GE.RED_DECK.Add(new Card(November3Red, 113, CardType.RIBBON, Month.NOVEMBER));
            GE.RED_DECK.Add(new Card(November4Red, 114, CardType.CHAFF, Month.NOVEMBER));
            GE.RED_DECK.Add(new Card(December1Red, 121, CardType.BRIGHT, Month.DECEMBER));
            GE.RED_DECK.Add(new Card(December2Red, 122, CardType.CHAFF, Month.DECEMBER));
            GE.RED_DECK.Add(new Card(December3Red, 123, CardType.CHAFF, Month.DECEMBER));
            GE.RED_DECK.Add(new Card(December4Red, 124, CardType.CHAFF, Month.DECEMBER));

            GameEngine.FULL_DECK = GE.Deck;

            await GE.DealCards();
            DealCards();
        }
    }

    public void DealCards()
    {
        List<Card> playerCards = GE.State.CardsAtPlayer;
        List<Card> middleCards = GE.State.CardsInMiddle;
        List<Card> opponentCards = GE.State.CardsAtAI;
        for (int i = 0; i < 8; i++)
        {
            GameObject playerCard = Instantiate(CardPrefab, new Vector2(0, 0), Quaternion.identity);
            RawImage image = playerCard.GetComponent<RawImage>();
            image.texture = playerCards[i].FrontPic;
            playerCard.name = playerCards[i].Id.ToString();
            PlayerArea.GetComponent<PlayerAreaScript>().Receive(playerCard);

            GameObject middleCard = Instantiate(CardPrefab, new Vector2(0, 0), Quaternion.identity);
            image = middleCard.GetComponent<RawImage>();
            image.texture = middleCards[i].FrontPic;
            middleCard.name = middleCards[i].Id.ToString();
            MiddleArea.GetComponent<MiddleAreaScript>().Receive(middleCard);

            GameObject opponentCard = Instantiate(CardPrefab, new Vector2(0, 0), Quaternion.identity);
            image = opponentCard.GetComponent<RawImage>();
            image.texture = BlackBack;
            opponentCard.name = opponentCards[i].Id.ToString();
            OpponentArea.GetComponent<OpponentAreaScript>().Receive(opponentCard);
        }

        GameObject deck = Instantiate(CardPrefab, new Vector2(0, 0), Quaternion.identity);
        RawImage img = deck.GetComponent<RawImage>();
        img.texture = BlackBack;
        deck.name = "deck";
        deck.transform.SetParent(DeckArea.transform, false);
    }
}
