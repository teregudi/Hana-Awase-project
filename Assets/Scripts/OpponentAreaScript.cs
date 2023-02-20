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

    public List<GameObject> cards = new List<GameObject>();

    void Start()
    {
        GE = GameEngine.GetGameEngine();
        middleAreaScript = GameObject.Find("MiddleArea").GetComponent<MiddleAreaScript>();
        opponentBrightScript = GameObject.Find("OpponentBright").GetComponent<OpponentBrightScript>();
        opponentAnimalScript = GameObject.Find("OpponentAnimal").GetComponent<OpponentAnimalScript>();
        opponentRibbonScript = GameObject.Find("OpponentRibbon").GetComponent<OpponentRibbonScript>();
        opponentChaffScript = GameObject.Find("OpponentChaff").GetComponent<OpponentChaffScript>();
    }

    void Update()
    {
        if (GE.currentPhase == Phase.AI_TURN_BEGIN)
        {
            GE.currentPhase = Phase.AI_TURN;
            
            HandleAiMoveFromHand();
        }
    }

    public void Receive(GameObject card)
    {
        cards.Add(card);
        card.transform.SetParent(transform, false);
    }

    public async void HandleAiMoveFromHand()
    {
        float calcStart = Time.time;
        var result = GE.CalculateAiMoveFromHand();
        float calcEnd = Time.time;
        float timePassed = calcEnd - calcStart;
        if (timePassed < 1)
        {
            int milisecondsToWait = 1000 - (int)(timePassed * 1000);
            await Task.Delay(milisecondsToWait);
        }
        GameObject playedCard = cards.First(c => c.name == result.Item1.Id.ToString());
        RawImage playedCardImage = playedCard.GetComponent<RawImage>();
        playedCardImage.texture = GameEngine.FULL_DECK.First(c => c.Id == int.Parse(playedCard.name)).FrontPic;
        await Task.Delay(2000);
        if (!result.Item2.Any())
        {
            middleAreaScript.Receive(playedCard);
        }
        else
        {
            SelectRowInCollection(playedCard, GameEngine.FULL_DECK.First(c => c.Id == int.Parse(playedCard.name)).Type);
            foreach (var cardFromMiddle in result.Item2)
            {
                GameObject card = middleAreaScript.cards.First(c => c.name == cardFromMiddle.Id.ToString());
                middleAreaScript.cards.Remove(card);
                SelectRowInCollection(card, GameEngine.FULL_DECK.First(c => c.Id == cardFromMiddle.Id).Type);
            }
        }
        cards.Remove(playedCard);

        await HandleAIFlipTopCard();
    }

    public async Task HandleAIFlipTopCard()
    {
        await Task.Delay(1000);
        GE.FlipTopCard();
        middleAreaScript.FlipTopCard();

        float calcStart = Time.time;
        GE.HandleFlippedCard();
        float calcEnd = Time.time;
        float timePassed = calcEnd - calcStart;
        if (timePassed < 2)
        {
            int milisecondsToWait = 2000 - (int)(timePassed * 1000);
            await Task.Delay(milisecondsToWait);
        }

        // ezen még lehetne szépíteni
        List<GameObject> collectedCards = new List<GameObject>();
        foreach (var card1 in middleAreaScript.markedCards)
        {
            bool collected = true;
            foreach (var card2 in GE.currentState.CardsInMiddle)
            {
                if (card1.name == card2.Id.ToString())
                    collected = false;
            }
            if (collected)
            {
                collectedCards.Add(card1);
            }  
        }
        GameObject cardToCollectFromMiddle = null;
        if (collectedCards.Count == 1)
            cardToCollectFromMiddle = collectedCards.First();
        middleAreaScript.HandleFlippedCard(cardToCollectFromMiddle);

        GE.flippedCard = null;
        
        if (GE.currentState.CardsAtAI.Any())
        {
            GE.currentPhase = Phase.PLAYER_FROM_HAND;
        }
        else
        {
            if (GameEngine.isZeroSum)
            {
                GE.currentPhase = GameEngine.endGameAlreadyStarted ? Phase.PLAYER_MOVE_BLOCKED : Phase.ENDGAME;
            }
            else
            {
                GE.currentPhase = Phase.GAME_OVER;
            }
        }
    }

    private void SelectRowInCollection(GameObject card, CardType type)
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
        middleAreaScript.RefreshScore();
    }
}
