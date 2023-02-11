using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAreaScript : MonoBehaviour
{
    public GameEngine GE;
    public MiddleAreaScript middleAreaScript;
    public PlayerBrightScript playerBrightScript;
    public PlayerAnimalScript playerAnimalScript;
    public PlayerRibbonScript playerRibbonScript;
    public PlayerChaffScript playerChaffScript;

    public List<GameObject> Cards { get; set; } = new List<GameObject>();
    public GameObject MarkedCard { get; set; } = null;

    void Start()
    {
        GE = GameEngine.getGE();
        middleAreaScript = GameObject.Find("MiddleArea").GetComponent<MiddleAreaScript>();
        playerBrightScript = GameObject.Find("PlayerBright").GetComponent<PlayerBrightScript>();
        playerAnimalScript = GameObject.Find("PlayerAnimal").GetComponent<PlayerAnimalScript>();
        playerRibbonScript = GameObject.Find("PlayerRibbon").GetComponent<PlayerRibbonScript>();
        playerChaffScript = GameObject.Find("PlayerChaff").GetComponent<PlayerChaffScript>();
    }

    public void Receive(GameObject card)
    {
        Cards.Add(card);
        card.transform.SetParent(transform, false);
    }

    public void MarkCardInHand(GameObject card)
    {
        if (MarkedCard != null)
        {
            UnmarkMarkedCard();
            Cards.Add(MarkedCard);
            MarkedCard = null;
            middleAreaScript.ResetMarkedCards();
        }
        Cards.Remove(card);
        MarkedCard = card;
        RawImage image = card.transform.GetComponent<RawImage>();
        image.texture = GE.RED_DECK.First(c => c.Id == int.Parse(card.transform.name)).FrontPic;
        middleAreaScript.FindAndMarkPossibleMatches(card.name);
    }

    public void PassToMiddle()
    {
        UnmarkMarkedCard();
        middleAreaScript.Receive(MarkedCard);
        MarkedCard = null;
    }

    private void UnmarkMarkedCard()
    {
        RawImage image = MarkedCard.GetComponent<RawImage>();
        image.texture = GameEngine.FULL_DECK.First(c => c.Id == int.Parse(MarkedCard.name)).FrontPic;
    }

    public void PassToCollection()
    {
        UnmarkMarkedCard();
        SelectPlaceForCard();
    }

    private void SelectPlaceForCard()
    {
        switch (GameEngine.FULL_DECK.First(c => c.Id == int.Parse(MarkedCard.name)).Type)
        {
            case CardType.BRIGHT:
                playerBrightScript.Receive(MarkedCard);
                break;
            case CardType.ANIMAL:
                playerAnimalScript.Receive(MarkedCard);
                break;
            case CardType.RIBBON:
                playerRibbonScript.Receive(MarkedCard);
                break;
            case CardType.CHAFF:
                playerChaffScript.Receive(MarkedCard);
                break;
        }
        MarkedCard = null;
        middleAreaScript.RefreshScore();
    }
}
