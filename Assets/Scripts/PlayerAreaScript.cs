using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAreaScript : MonoBehaviour
{
    private MiddleAreaScript middleAreaScript;
    private PlayerBrightScript playerBrightScript;
    private PlayerAnimalScript playerAnimalScript;
    private PlayerRibbonScript playerRibbonScript;
    private PlayerChaffScript playerChaffScript;

    public List<GameObject> cards = new List<GameObject>();
    public GameObject markedCard = null;

    void Start()
    {
        middleAreaScript = GameObject.Find("MiddleArea").GetComponent<MiddleAreaScript>();
        playerBrightScript = GameObject.Find("PlayerBright").GetComponent<PlayerBrightScript>();
        playerAnimalScript = GameObject.Find("PlayerAnimal").GetComponent<PlayerAnimalScript>();
        playerRibbonScript = GameObject.Find("PlayerRibbon").GetComponent<PlayerRibbonScript>();
        playerChaffScript = GameObject.Find("PlayerChaff").GetComponent<PlayerChaffScript>();
    }

    public void Receive(GameObject card)
    {
        cards.Add(card);
        card.transform.SetParent(transform, false);
    }

    public void MarkCardInHand(GameObject card)
    {
        if (markedCard != null)
        {
            UnmarkMarkedCard();
            cards.Add(markedCard);
            markedCard = null;
            middleAreaScript.ResetMarkedCards();
        }
        cards.Remove(card);
        markedCard = card;
        RawImage image = card.transform.GetComponent<RawImage>();
        image.texture = GameEngine.RED_DECK.First(c => c.Id == int.Parse(card.transform.name)).FrontPic;
        middleAreaScript.FindAndMarkPossibleMatches(card.name);
    }

    public void PassToMiddle()
    {
        UnmarkMarkedCard();
        middleAreaScript.Receive(markedCard);
        markedCard = null;
    }

    public void PassToCollection()
    {
        UnmarkMarkedCard();
        SelectRowInCollection();
    }

    private void UnmarkMarkedCard()
    {
        RawImage image = markedCard.GetComponent<RawImage>();
        image.texture = GameEngine.FULL_DECK.First(c => c.Id == int.Parse(markedCard.name)).FrontPic;
    }    

    private void SelectRowInCollection()
    {
        switch (GameEngine.FULL_DECK.First(c => c.Id == int.Parse(markedCard.name)).Type)
        {
            case CardType.BRIGHT:
                playerBrightScript.Receive(markedCard);
                break;
            case CardType.ANIMAL:
                playerAnimalScript.Receive(markedCard);
                break;
            case CardType.RIBBON:
                playerRibbonScript.Receive(markedCard);
                break;
            case CardType.CHAFF:
                playerChaffScript.Receive(markedCard);
                break;
        }
        markedCard = null;
        middleAreaScript.RefreshScore();
    }
}
