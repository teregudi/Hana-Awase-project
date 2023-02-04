using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class ClickOnCardManager : MonoBehaviour
{
    private GameEngine GE = GameEngine.getGE();
    private GameObject playerArea;
    private GameObject middleArea;
    private PlayerAreaScript playerAreaScript;
    private MiddleAreaScript middleAreaScript;

    public void Start()
    {
        GE = GameEngine.getGE();
        playerArea = GameObject.Find("PlayerArea");
        playerAreaScript = playerArea.GetComponent<PlayerAreaScript>();
        middleArea = GameObject.Find("MiddleArea");
        middleAreaScript = middleArea.GetComponent<MiddleAreaScript>();
    }

    public void OnClick()
    {
        //Debug.Log(GameEngine.FULL_DECK.First(c => c.Id == int.Parse(gameObject.name)));

        if (GE.Phase == Phase.PLAYER_MOVE_BLOCKED)
            return;
        // when player choose a card from hand
        if (transform.parent.name == playerArea.name && GE.Phase == Phase.PLAYER_FROM_HAND)
        {
            playerAreaScript.MarkCardInHand(gameObject);
        }
        // when player wants to drop chosen card to the middle
        else if (transform.parent.name == middleArea.name && playerAreaScript.MarkedCard != null && !middleAreaScript.MarkedCards.Any())
        {
            GE.MoveCardFromPlayerToMiddle(playerAreaScript.MarkedCard);

            playerAreaScript.PassToMiddle();
            HandleFlippingTopCard();
        }
        // when player wants to collect matching cards from hand
        else if (transform.parent.name == middleArea.name && GE.Phase == Phase.PLAYER_FROM_HAND && middleAreaScript.MarkedCards.Contains(gameObject))
        {
            GE.MoveCardFromPlayerToCollection(playerAreaScript.MarkedCard);
            GE.MoveCardFromMiddleToPlayerCollection(gameObject);

            playerAreaScript.PassToCollection();
            middleAreaScript.PassToCollection(gameObject);
            HandleFlippingTopCard();
        }
        // when player wants to collect matching cards from deck
        else if (transform.parent.name == middleArea.name && GE.Phase == Phase.PLAYER_FROM_DECK && middleAreaScript.MarkedCards.Contains(gameObject))
        {
            GE.HandleChoiceAfterDraw(gameObject);
            middleAreaScript.HandleChoiceAfterDraw(gameObject);
            GE.DrawnCard = null;
            GE.Phase = GE.State.CardsAtPlayer.Count > 0 ? Phase.AI_TURN_BEGIN : Phase.PLAYER_MOVE_BLOCKED;
        }
    }

    public async void HandleFlippingTopCard()
    {
        GE.DrawCard();

        middleAreaScript.FlipTopCard();

        if (middleAreaScript.MarkedCards.Count == 2)
        {
            GE.Phase = Phase.PLAYER_FROM_DECK;
        }
        else
        {
            GE.Phase = Phase.PLAYER_MOVE_BLOCKED;
            await Task.Delay(2000);
            GE.HandleDrawnCard();
            middleAreaScript.HandleFlippedCard();
            GE.DrawnCard = null;
            GE.Phase = Phase.AI_TURN_BEGIN;
        }
    }
}
