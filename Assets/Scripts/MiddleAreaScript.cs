using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

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

    public Text PlayerScore;
    public Text AiScore;

    public List<GameObject> Cards { get; set; } = new List<GameObject>();
    public List<GameObject> MarkedCards { get; set; } = new List<GameObject>();
    public GameObject FlippedCard { get; set; }

    void Start()
    {
        GE = GameEngine.getGE();
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
        PlayerScore = GameObject.Find("PlayerScore").GetComponent<Text>();
        AiScore = GameObject.Find("AiScore").GetComponent<Text>();
    }

    void Update()
    {
        if (GE.Phase == Phase.ENDGAME)
        {
            GE.Phase = Phase.PLAYER_MOVE_BLOCKED;
            GameEngine.endGameAlreadyStarted = true;
            EndGame();
        }

        if (GE.Phase == Phase.GAME_OVER)
        {
            GE.DebugLog();
            Debug.Log("GAME OVER");
        }
    }

    public void Receive(GameObject card)
    {
        Cards.Add(card);
        if (Cards.Count > 12)
        {
            GridLayoutGroup glg = GetComponent<GridLayoutGroup>();
            glg.spacing = new Vector2(0, 40);
        }
        //if (Cards.Count > 8)
        //{
        //    GameObject p = GameObject.Find("TT Canvas");
        //    card.transform.SetParent(p.transform, true);
        //    Transform target = opponentAreaScript.Cards.First().transform;
        //    card.transform.position = new Vector2(0, 0);
        //    card.transform.position = Vector3.Lerp(card.transform.position, new Vector3(0, 0), Time.deltaTime * 5);
        //}
        //else

        card.transform.SetParent(transform, false);
    }

    public void FindAndMarkPossibleMatches(string idString)
    {
        int id = int.Parse(idString);
        foreach (var card in Cards)
        {
            if (int.Parse(card.name) / 10 == id / 10)
            {
                RawImage image = card.GetComponent<RawImage>();
                image.texture = GE.RED_DECK.First(c => c.Id == int.Parse(card.name)).FrontPic;
                MarkedCards.Add(card);
            }
        }
        foreach (var card in MarkedCards)
        {
            Cards.Remove(card);
        }
    }

    public void ResetMarkedCards()
    {
        foreach (var card in MarkedCards)
        {
            Unmark(card);
            Cards.Add(card);
        }
        MarkedCards.Clear();
    }

    private void Unmark(GameObject card)
    {
        RawImage image = card.GetComponent<RawImage>();
        image.texture = GameEngine.FULL_DECK.First(c => c.Id == int.Parse(card.name)).FrontPic;
    }

    public void PassToCollection(GameObject chosenCard)
    {
        CardType type = GameEngine.FULL_DECK.First(c => c.Id == int.Parse(chosenCard.name)).Type;
        if (MarkedCards.Count == 2)
        {
            Unmark(chosenCard);
            DeterminePlaceInCollection(chosenCard, type);
            MarkedCards.Remove(chosenCard);
            ResetMarkedCards();
        }
        else
        {
            foreach (var card in MarkedCards)
            {
                Unmark(card);
                DeterminePlaceInCollection(card, GameEngine.FULL_DECK.First(c => c.Id == int.Parse(card.name)).Type);
            }
            MarkedCards.Clear();
        }
    }

    public void PassAllMarkedToCollection()
    {
        foreach (var card in MarkedCards)
        {
            Unmark(card);
            DeterminePlaceInCollection(card, GameEngine.FULL_DECK.First(c => c.Id == int.Parse(card.name)).Type);
        }
        MarkedCards.Clear();
    }

    public void DeterminePlaceInCollection(GameObject card, CardType type)
    {
        switch (type)
        {
            case CardType.BRIGHT:
                if (GE.Phase == Phase.AI_TURN) opponentBrightScript.Receive(card);
                else playerBrightScript.Receive(card);
                break;
            case CardType.ANIMAL:
                if (GE.Phase == Phase.AI_TURN) opponentAnimalScript.Receive(card);
                else playerAnimalScript.Receive(card);
                break;
            case CardType.RIBBON:
                if (GE.Phase == Phase.AI_TURN) opponentRibbonScript.Receive(card);
                else playerRibbonScript.Receive(card);
                break;
            case CardType.CHAFF:
                if (GE.Phase == Phase.AI_TURN) opponentChaffScript.Receive(card);
                else playerChaffScript.Receive(card);
                break;
        }
        if (Cards.Count <= 12)
        {
            GridLayoutGroup glg = GetComponent<GridLayoutGroup>();
            glg.spacing = new Vector2(20, 40);
        }
        RefreshScore();
    }

    public void OnClick()
    {
        if (playerAreaScript.MarkedCard != null && !MarkedCards.Any())
        {
            GE.MoveCardFromPlayerToMiddle(playerAreaScript.MarkedCard);
            playerAreaScript.PassToMiddle();
            cardPrefab.GetComponent<ClickOnCardManager>().Start();
            cardPrefab.GetComponent<ClickOnCardManager>().HandleFlippingTopCard();
        }
    }

    public void FlipTopCard()
    {
        FlippedCard = GameObject.Find("deck");
        FlippedCard.name = GE.DrawnCard.Id.ToString();
        RawImage flippedImage = FlippedCard.GetComponent<RawImage>();
        flippedImage.texture = GE.DrawnCard.FrontPic;

        FindAndMarkPossibleMatches(FlippedCard.name);
    }

    public void HandleFlippedCard()
    {
        if (MarkedCards.Count == 0)
        {
            Receive(FlippedCard);
            FlippedCard = null;
            SetNewDeckPlaceholder();
        }
        else if (MarkedCards.Count == 1 || MarkedCards.Count == 3)
        {
            DeterminePlaceInCollection(FlippedCard, GE.DrawnCard.Type);
            FlippedCard = null;
            PassAllMarkedToCollection();
            SetNewDeckPlaceholder();
        }
    }

    public void HandleAiFlippedCard(IEnumerable<GameObject> collectedCards)
    {
        if (!collectedCards.Any())
        {
            Receive(FlippedCard);
            FlippedCard = null;
            SetNewDeckPlaceholder();
        }
        else
        {
            DeterminePlaceInCollection(FlippedCard, GE.DrawnCard.Type);
            FlippedCard = null;
            foreach (var card in collectedCards)
            {
                Unmark(card);
                DeterminePlaceInCollection(card, GameEngine.FULL_DECK.First(c => c.Id == int.Parse(card.name)).Type);
                MarkedCards.Remove(card);
            }
            ResetMarkedCards();
            SetNewDeckPlaceholder();
        }
    }

    public void HandleChoiceAfterDraw(GameObject chosenCard)
    {
        DeterminePlaceInCollection(FlippedCard, GE.DrawnCard.Type);
        Unmark(chosenCard);
        DeterminePlaceInCollection(chosenCard, GameEngine.FULL_DECK.First(c => c.Id == int.Parse(chosenCard.name)).Type);
        MarkedCards.Remove(chosenCard);
        ResetMarkedCards();
        SetNewDeckPlaceholder();
    }

    public void RefreshScore()
    {
        if (GE.isZeroSum)
        {
            PlayerScore.text = "Player score:\n" + GE.State.GetPlayerZeroSumScore().ToString();
            AiScore.text = "Opponent score:\n" + GE.State.GetAiZeroSumScore().ToString();
        }
        else
        {
            PlayerScore.text = "Player score:\n" + GE.State.GetPlayerAdditiveScore().ToString();
            AiScore.text = "Opponent score:\n" + GE.State.GetAiAdditiveScore().ToString();
        }
    }

    private void SetNewDeckPlaceholder()
    {
        if (GE.Deck.Count == 0 && GE.State.CardsInMiddle.Count == 0) return;
        GameObject newDeckTopCard = Instantiate(cardPrefab, new Vector2(0, 0), Quaternion.identity);
        RawImage backOfCard = newDeckTopCard.GetComponent<RawImage>();
        backOfCard.texture = blackBack;
        newDeckTopCard.name = "deck";
        newDeckTopCard.transform.SetParent(deckArea.transform, false);
    }

    private async void EndGame()
    {
        while (GE.Deck.Count > 0 && GE.State.CardsInMiddle.Count > 0)
        {
            await Task.Delay(1000);
            GE.DrawCard();
            FlipTopCard();
            if (MarkedCards.Count == 2)
            {
                GE.Phase = Phase.PLAYER_FROM_DECK;
                await WaitForPlayerToChoose();
            }
            else
            {
                await Task.Delay(2000);
                GE.HandleDrawnCard();
                HandleFlippedCard();
                GE.DrawnCard = null;
            }

            if (GE.Deck.Count == 0 && GE.State.CardsInMiddle.Count == 0)
            {
                break;
            }

            GE.Phase = Phase.AI_TURN;
            await Task.Delay(1000);
            await opponentAreaScript.HandleAIFlipTopCard();
        }
        GE.Phase = Phase.GAME_OVER;
    }

    private async Task WaitForPlayerToChoose()
    {
        while (GE.Phase != Phase.PLAYER_MOVE_BLOCKED);
        await Task.Delay(1000);
    }
}
