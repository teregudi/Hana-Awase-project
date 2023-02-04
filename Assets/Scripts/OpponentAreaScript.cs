using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class OpponentAreaScript : MonoBehaviour
{
    private GameEngine GE;
    private MiddleAreaScript middleAreaScript;
    private OpponentBrightScript opponentBrightScript;
    private OpponentAnimalScript opponentAnimalScript;
    private OpponentRibbonScript opponentRibbonScript;
    private OpponentChaffScript opponentChaffScript;

    public List<GameObject> Cards { get; set; } = new List<GameObject>();

    void Start()
    {
        GE = GameEngine.getGE();
        middleAreaScript = GameObject.Find("MiddleArea").GetComponent<MiddleAreaScript>();
        opponentBrightScript = GameObject.Find("OpponentBright").GetComponent<OpponentBrightScript>();
        opponentAnimalScript = GameObject.Find("OpponentAnimal").GetComponent<OpponentAnimalScript>();
        opponentRibbonScript = GameObject.Find("OpponentRibbon").GetComponent<OpponentRibbonScript>();
        opponentChaffScript = GameObject.Find("OpponentChaff").GetComponent<OpponentChaffScript>();
    }

    void Update()
    {
        if (GE.Phase == Phase.AI_TURN_BEGIN)
        {
            GE.Phase = Phase.AI_TURN;
            HandleAiMoveFromHand();
        }
    }

    public void Receive(GameObject card)
    {
        Cards.Add(card);
        card.transform.SetParent(transform, false);
    }

    public async void HandleAiMoveFromHand()
    {
        await Task.Delay(1000);
        var collectedFromMiddle = GE.CalculateAiMoveFromHand();
        GameObject playedCard = null;
        foreach (var card1 in Cards)
        {
            bool played = true;
            foreach (var card2 in GE.State.CardsAtAI)
                if (card1.name == card2.Id.ToString())
                    played = false;
            if (played)
            {
                playedCard = card1;
                break;
            } 
        }
        RawImage playedCardImage = playedCard.GetComponent<RawImage>();
        playedCardImage.texture = GameEngine.FULL_DECK.First(c => c.Id == int.Parse(playedCard.name)).FrontPic;
        await Task.Delay(2000);
        if (!collectedFromMiddle.Any())
        {
            middleAreaScript.Receive(playedCard);
        }
        else
        {
            DeterminePlaceInCollection(playedCard, GameEngine.FULL_DECK.First(c => c.Id == int.Parse(playedCard.name)).Type);
            foreach (var cardFromMiddle in collectedFromMiddle)
            {
                GameObject card = middleAreaScript.Cards.First(c => c.name == cardFromMiddle.Id.ToString());
                middleAreaScript.Cards.Remove(card);
                DeterminePlaceInCollection(card, GameEngine.FULL_DECK.First(c => c.Id == cardFromMiddle.Id).Type);
            }
        }
        Cards.Remove(playedCard);

        await Task.Delay(1000);
        await HandleAIFlipTopCard();
    }

    public async Task HandleAIFlipTopCard()
    {
        GE.DrawCard();
        middleAreaScript.FlipTopCard();
        await Task.Delay(2000);
        GE.HandleDrawnCard();

        List<GameObject> collectedCards = new List<GameObject>();
        foreach (var card1 in middleAreaScript.MarkedCards)
        {
            bool collected = true;
            foreach (var card2 in GE.State.CardsInMiddle)
            {
                if (card1.name == card2.Id.ToString())
                    collected = false;
            }
            if (collected)
            {
                collectedCards.Add(card1);
            }  
        }
        middleAreaScript.HandleAiFlippedCard(collectedCards);

        GE.DrawnCard = null;
        GE.Phase = GE.State.CardsAtAI.Count > 0 ? Phase.PLAYER_FROM_HAND : (GameEngine.endGameAlreadyStarted ? Phase.PLAYER_MOVE_BLOCKED : Phase.ENDGAME);
    }

    private void DeterminePlaceInCollection(GameObject card, CardType type)
    {
        switch (type)
        {
            case CardType.BRIGHT:
                opponentBrightScript.Receive(card);
                break;
            case CardType.ANIMAL:
                opponentAnimalScript.Receive(card);
                break;
            case CardType.RIBBON:
                opponentRibbonScript.Receive(card);
                break;
            case CardType.CHAFF:
                opponentChaffScript.Receive(card);
                break;
        }
    }
}
