using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class MiddleAreaScript : MonoBehaviour
{
    private GameEngine GE;
    private GameObject deckArea;
    private PlayerAreaScript playerAreaScript;
    private OpponentAreaScript opponentAreaScript;
    private PlayerBrightScript playerBrightScript;
    private PlayerAnimalScript playerAnimalScript;
    private PlayerRibbonScript playerRibbonScript;
    private PlayerChaffScript playerChaffScript;
    private OpponentBrightScript opponentBrightScript;
    private OpponentAnimalScript opponentAnimalScript;
    private OpponentRibbonScript opponentRibbonScript;
    private OpponentChaffScript opponentChaffScript;
    
    public Texture blackBack;
    public GameObject cardPrefab;
    
    public Text playerScore;
    public Text aiScore;

    public List<GameObject> cards = new List<GameObject>();
    public List<GameObject> markedCards = new List<GameObject>();
    public GameObject flippedCard = null;

    void Start()
    {
        GE = GameEngine.GetGameEngine();
        deckArea = GameObject.Find("DeckArea");
        playerAreaScript = GameObject.Find("PlayerArea").GetComponent<PlayerAreaScript>();
        playerBrightScript = GameObject.Find("PlayerBright").GetComponent<PlayerBrightScript>();
        playerAnimalScript = GameObject.Find("PlayerAnimal").GetComponent<PlayerAnimalScript>();
        playerRibbonScript = GameObject.Find("PlayerRibbon").GetComponent<PlayerRibbonScript>();
        playerChaffScript = GameObject.Find("PlayerChaff").GetComponent<PlayerChaffScript>();
        opponentAreaScript = GameObject.Find("OpponentArea").GetComponent<OpponentAreaScript>();
        opponentBrightScript = GameObject.Find("OpponentBright").GetComponent<OpponentBrightScript>();
        opponentAnimalScript = GameObject.Find("OpponentAnimal").GetComponent<OpponentAnimalScript>();
        opponentRibbonScript = GameObject.Find("OpponentRibbon").GetComponent<OpponentRibbonScript>();
        opponentChaffScript = GameObject.Find("OpponentChaff").GetComponent<OpponentChaffScript>();
        playerScore = GameObject.Find("PlayerScore").GetComponent<Text>();
        aiScore = GameObject.Find("AiScore").GetComponent<Text>();
    }

    void Update()
    {
        if (GE.currentPhase == Phase.ENDGAME)
        {
            GE.currentPhase = Phase.PLAYER_MOVE_BLOCKED;
            GameEngine.endGameAlreadyStarted = true;
            EndGame();
        }
    }

    public void Receive(GameObject card)
    {
        cards.Add(card);
        if (cards.Count > 12)
        {
            GridLayoutGroup glg = GetComponent<GridLayoutGroup>();
            glg.spacing = new Vector2(0, 40);
        }
        card.transform.SetParent(transform, false);
    }

    public void FindAndMarkPossibleMatches(string idString)
    {
        int id = int.Parse(idString);
        foreach (var card in cards)
        {
            if (int.Parse(card.name) / 10 == id / 10)
            {
                RawImage image = card.GetComponent<RawImage>();
                image.texture = GameEngine.RED_DECK.First(c => c.Id == int.Parse(card.name)).FrontPic;
                markedCards.Add(card);
            }
        }
        foreach (var card in markedCards)
        {
            cards.Remove(card);
        }
    }

    public void ResetMarkedCards()
    {
        foreach (var card in markedCards)
        {
            Unmark(card);
            cards.Add(card);
        }
        markedCards.Clear();
    }

    private void Unmark(GameObject card)
    {
        RawImage image = card.GetComponent<RawImage>();
        image.texture = GameEngine.FULL_DECK.First(c => c.Id == int.Parse(card.name)).FrontPic;
    }

    public void PassToCollection(GameObject chosenCard)
    {
        if (markedCards.Count == 2)
        {
            Unmark(chosenCard);
            DeterminePlaceInCollection(chosenCard, GameEngine.FULL_DECK.First(c => c.Id == int.Parse(chosenCard.name)).Type);
            markedCards.Remove(chosenCard);
            ResetMarkedCards();
        }
        else
        {
            PassAllMarkedToCollection();
        }
    }

    public void PassAllMarkedToCollection()
    {
        foreach (var card in markedCards)
        {
            Unmark(card);
            DeterminePlaceInCollection(card, GameEngine.FULL_DECK.First(c => c.Id == int.Parse(card.name)).Type);
        }
        markedCards.Clear();
    }

    public void DeterminePlaceInCollection(GameObject card, CardType type)
    {
        switch (type)
        {
            case CardType.BRIGHT:
                if (GE.currentPhase == Phase.AI_TURN) opponentBrightScript.Receive(card);
                else playerBrightScript.Receive(card);
                break;
            case CardType.ANIMAL:
                if (GE.currentPhase == Phase.AI_TURN) opponentAnimalScript.Receive(card);
                else playerAnimalScript.Receive(card);
                break;
            case CardType.RIBBON:
                if (GE.currentPhase == Phase.AI_TURN) opponentRibbonScript.Receive(card);
                else playerRibbonScript.Receive(card);
                break;
            case CardType.CHAFF:
                if (GE.currentPhase == Phase.AI_TURN) opponentChaffScript.Receive(card);
                else playerChaffScript.Receive(card);
                break;
        }
        if (cards.Count <= 12)
        {
            GridLayoutGroup glg = GetComponent<GridLayoutGroup>();
            glg.spacing = new Vector2(20, 40);
        }
        RefreshScore();
    }

    public void OnClick()
    {
        if (playerAreaScript.markedCard != null && !markedCards.Any())
        {
            GE.MoveCardFromPlayerToMiddle(playerAreaScript.markedCard);
            playerAreaScript.PassToMiddle();
            cardPrefab.GetComponent<ClickOnCardManager>().Start();
            cardPrefab.GetComponent<ClickOnCardManager>().HandleFlippingTopCard();
        }
    }

    public void FlipTopCard()
    {
        flippedCard = GameObject.Find("deck");
        flippedCard.name = GE.flippedCard.Id.ToString();
        RawImage flippedImage = flippedCard.GetComponent<RawImage>();
        flippedImage.texture = GE.flippedCard.FrontPic;

        FindAndMarkPossibleMatches(flippedCard.name);
    }

    public void HandleFlippedCard(GameObject chosenCard = null)
    {
        if (markedCards.Count == 0)
        {
            Receive(flippedCard);
        }
        else if (markedCards.Count == 1 || markedCards.Count == 3)
        {
            DeterminePlaceInCollection(flippedCard, GE.flippedCard.Type);
            PassAllMarkedToCollection();
        }
        else
        {
            DeterminePlaceInCollection(flippedCard, GE.flippedCard.Type);
            Unmark(chosenCard);
            DeterminePlaceInCollection(chosenCard, GameEngine.FULL_DECK.First(c => c.Id == int.Parse(chosenCard.name)).Type);
            markedCards.Remove(chosenCard);
            ResetMarkedCards();
        }
        flippedCard = null;
        SetNewDeckPlaceholder();
    }

    public void RefreshScore()
    {
        playerScore.text = "Player score\n" + GE.currentState.GetPlayerScore().ToString();
        aiScore.text = "Opponent score\n" + GE.currentState.GetAiScore().ToString();
    }

    private void SetNewDeckPlaceholder()
    {
        if (GE.deck.Count == 0 && GE.currentState.CardsInMiddle.Count == 0) return;
        GameObject newDeckTopCard = Instantiate(cardPrefab, new Vector2(0, 0), Quaternion.identity);
        RawImage backOfCard = newDeckTopCard.GetComponent<RawImage>();
        backOfCard.texture = blackBack;
        newDeckTopCard.name = "deck";
        newDeckTopCard.transform.SetParent(deckArea.transform, false);
    }

    private async void EndGame()
    {
        while (GE.deck.Count > 0 && GE.currentState.CardsInMiddle.Count > 0)
        {
            await Task.Delay(1000);
            GE.FlipTopCard();
            FlipTopCard();
            if (markedCards.Count == 2)
            {
                GE.currentPhase = Phase.PLAYER_FROM_DECK;
                await WaitForPlayerToChoose();
            }
            else
            {
                await Task.Delay(2000);
                GE.HandleFlippedCard();
                HandleFlippedCard();
                GE.flippedCard = null;
            }

            if (GE.deck.Count == 0 && GE.currentState.CardsInMiddle.Count == 0)
            {
                break;
            }

            GE.currentPhase = Phase.AI_TURN;
            await Task.Delay(1000);
            await opponentAreaScript.HandleAIFlipTopCard();
        }
        GE.currentPhase = Phase.GAME_OVER;
    }

    private async Task WaitForPlayerToChoose()
    {
        while (GE.currentPhase != Phase.PLAYER_MOVE_BLOCKED);
        await Task.Delay(1000);
    }
}
