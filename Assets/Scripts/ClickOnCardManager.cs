using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class ClickOnCardManager : MonoBehaviour
{
    private GameEngine GE;
    private GameObject playerArea;
    private GameObject middleArea;
    private PlayerAreaScript playerAreaScript;
    private MiddleAreaScript middleAreaScript;

    public void Start()
    {
        GE = GameEngine.GetGameEngine();
        playerArea = GameObject.Find("PlayerArea");
        playerAreaScript = playerArea.GetComponent<PlayerAreaScript>();
        middleArea = GameObject.Find("MiddleArea");
        middleAreaScript = middleArea.GetComponent<MiddleAreaScript>();
    }

    public void OnClick()
    {
        if (GE.currentPhase == Phase.PLAYER_MOVE_BLOCKED)
            return;
        // when player choose a card from hand
        if (transform.parent.name == playerArea.name && GE.currentPhase == Phase.PLAYER_FROM_HAND)
        {
            playerAreaScript.MarkCardInHand(gameObject);
        }
        // when player wants to drop chosen card to the middle
        else if (transform.parent.name == middleArea.name && playerAreaScript.markedCard != null && !middleAreaScript.markedCards.Any())
        {
            GE.MoveCardFromPlayerToMiddle(playerAreaScript.markedCard);

            playerAreaScript.PassToMiddle();
            HandleFlippingTopCard();
        }
        // when player wants to collect matching cards from hand
        else if (transform.parent.name == middleArea.name && GE.currentPhase == Phase.PLAYER_FROM_HAND && middleAreaScript.markedCards.Contains(gameObject))
        {
            GE.MoveCardFromPlayerToCollection(playerAreaScript.markedCard);
            GE.MoveCardFromMiddleToPlayerCollection(gameObject);

            playerAreaScript.PassToCollection();
            middleAreaScript.PassToCollection(gameObject);
            HandleFlippingTopCard();
        }
        // when player wants to collect matching cards from deck
        else if (transform.parent.name == middleArea.name && GE.currentPhase == Phase.PLAYER_FROM_DECK && middleAreaScript.markedCards.Contains(gameObject))
        {
            GE.HandleChoiceAfterFlipByPlayer(gameObject);
            middleAreaScript.HandleFlippedCard(gameObject);
            GE.flippedCard = null;
            GE.currentPhase = GE.currentState.CardsAtPlayer.Count > 0 ? Phase.AI_TURN_BEGIN : Phase.PLAYER_MOVE_BLOCKED;
        }
    }

    public async void HandleFlippingTopCard()
    {
        GE.FlipTopCard();
        middleAreaScript.FlipTopCard();

        if (middleAreaScript.markedCards.Count == 2)
        {
            GE.currentPhase = Phase.PLAYER_FROM_DECK;
        }
        else
        {
            GE.currentPhase = Phase.PLAYER_MOVE_BLOCKED;
            await Task.Delay(2000);
            GE.HandleFlippedCard();
            middleAreaScript.HandleFlippedCard();
            GE.flippedCard = null;
            GE.currentPhase = Phase.AI_TURN_BEGIN;
        }
    }
}
