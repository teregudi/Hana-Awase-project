using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ClickOnCardManager : MonoBehaviour
{
    private GameEngine GE;
    private GameObject playerArea;
    private GameObject middleArea;
    private PlayerAreaScript playerAreaScript;
    private MiddleAreaScript middleAreaScript;

    void Start()
    {
        GE = GameEngine.getGE();
        playerArea = GameObject.Find("PlayerArea");
        playerAreaScript = playerArea.GetComponent<PlayerAreaScript>();
        middleArea = GameObject.Find("MiddleArea");
        middleAreaScript = middleArea.GetComponent<MiddleAreaScript>();
    }

    // következõ refaktor:
    // elõbb mindig GE, csak utána UI
    // a GE Update-en keresztül hívja a UI-t?

    public void OnClick()
    {
        if (GE.Phase == Phase.PLAYER_CLICK_BLOCKED)
            return;
        // when player choose a card from hand
        if (transform.parent.name == playerArea.name && GE.Phase == Phase.PLAYER_FROM_HAND)
        {
            playerAreaScript.MarkCardInHand(gameObject);
        }
        // when player wants to drop chosen card to the middle
        else if (transform.parent.name == middleArea.name && playerAreaScript.MarkedCard != null && !middleAreaScript.MarkedCards.Any())
        {
            GE.MoveCardFromPlayerToMiddle(gameObject);

            playerAreaScript.PassToMiddle();
            HandleFlippingTopCard();
        }
        // when player wants to collect matching cards from hand
        else if (transform.parent.name == middleArea.name && GE.Phase == Phase.PLAYER_FROM_HAND && middleAreaScript.MarkedCards.Contains(gameObject))
        {
            GE.MoveCardFromPlayerToCollection(gameObject);
            GE.MoveCardFromMiddleToPlayerCollection(gameObject);

            playerAreaScript.PassToCollection();
            middleAreaScript.PassToCollection(gameObject);
            HandleFlippingTopCard();
        }
        // when player wants to collect matching cards from deck
        else if (transform.parent.name == middleArea.name && GE.Phase != Phase.PLAYER_FROM_DECK && middleAreaScript.MarkedCards.Contains(gameObject))
        {
            middleAreaScript.HandleChosenCard(gameObject);
        }
    }

    private async void HandleFlippingTopCard()
    {
        GE.DrawCard();
        GE.Phase = Phase.PLAYER_CLICK_BLOCKED;

        middleAreaScript.FlipTopCard();
        if (middleAreaScript.MarkedCards.Count == 2)
        {
            GE.Phase = Phase.PLAYER_FROM_DECK;
        }
        else
        {
            await Task.Delay(3000);
            GE.HandleDrawnCard();
            middleAreaScript.HandleFlippedCard();
            GE.DrawnCard = null;
            GE.Phase = Phase.AI_TURN;
        }
    }
}
