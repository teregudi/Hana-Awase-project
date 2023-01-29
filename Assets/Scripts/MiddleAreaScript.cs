using System.Collections;
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
    private PlayerBrightScript playerBrightScript;
    private PlayerAnimalScript playerAnimalScript;
    private PlayerRibbonScript playerRibbonScript;
    private PlayerChaffScript playerChaffScript;
    private Texture blackBack;

    public GameObject cardPrefab;

    private GameObject OpponentBright;
    private GameObject OpponentAnimal;
    private GameObject OpponentRibbon;
    private GameObject OpponentChaff;

    private Text PlayerScore;
    private Text AiScore;

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


        OpponentBright = GameObject.Find("OpponentBright");
        OpponentAnimal = GameObject.Find("OpponentAnimal");
        OpponentRibbon = GameObject.Find("OpponentRibbon");
        OpponentChaff = GameObject.Find("OpponentChaff");


    }

    void Update()
    {
        //PlayerScore.text = GE.State.PlayerScoreCurrently.ToString();
        //AiScore.text = GE.State.AiScoreCurrently.ToString();
        if (GE.Phase == Phase.AI_TURN)
            HandleAiMove();
        //if (GE.Deck.Count == 0 && GE.State.CardsInMiddle.Count == 0)
        //    PlayerScore.text = "game over";
    }

    public void Receive(GameObject card)
    {
        Cards.Add(card);
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
                DeterminePlaceInCollection(card, type);
            }
            MarkedCards.Clear();
        }
    }

    public void PassAllMarkedToCollection()
    {
        foreach (var card in MarkedCards)
        {
            Unmark(card);
            DeterminePlaceInCollection(card, GE.DrawnCard.Type);
        }
        MarkedCards.Clear();
    }

    public void DeterminePlaceInCollection(GameObject card, CardType type)
    {
        switch (type)
        {
            case CardType.BRIGHT:
                playerBrightScript.Receive(card);
                break;
            case CardType.ANIMAL:
                playerAnimalScript.Receive(card);
                break;
            case CardType.RIBBON:
                playerRibbonScript.Receive(card);
                break;
            case CardType.CHAFF:
                playerChaffScript.Receive(card);
                break;
        }
    }

    public void OnClick()
    {
        if (playerAreaScript.MarkedCard != null && !MarkedCards.Any())
        {
            playerAreaScript.PassToMiddle();
            HandleFlippedCard();
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

    public void HandleChosenCard(GameObject chosenCard)
    {
        DeterminePlaceInCollection(GameObject.Find(GE.DrawnCard.Id.ToString()), GE.DrawnCard.Type);
        DeterminePlaceInCollection(chosenCard, GE.State.CardsInMiddle.First(c => c.Id == int.Parse(chosenCard.name)).Type);
        MarkedCards.Remove(chosenCard);
        ResetMarkedCards();
        SetNewDeckPlaceholder();
        GE.DrawnCard = null;
        GE.Phase = Phase.AI_TURN;
    }

    private void SetNewDeckPlaceholder()
    {
        GameObject newDeckTopCard = Instantiate(cardPrefab, new Vector2(0, 0), Quaternion.identity);
        RawImage backOfCard = newDeckTopCard.GetComponent<RawImage>();
        backOfCard.texture = blackBack;
        newDeckTopCard.name = "deck";
        newDeckTopCard.transform.SetParent(deckArea.transform, false);
    }

    // AI rész

    public void HandleAiMove()
    {
        (IEnumerable<Card> fromMiddleToCollection, IEnumerable<Card> newToMiddle, IEnumerable<Card> fromHandToCollection) = GE.CalculateAiMove();
        foreach (var card in fromMiddleToCollection)
        {
            SetAiCardAside(GameObject.Find(card.Id.ToString()), card.Type);
        }
        foreach (var card in newToMiddle)
        {
            AddCardToMiddle(card);
        }
        foreach (var card in fromHandToCollection)
        {
            SetCardFromHandToAside(fromHandToCollection.First());
        }
        GE.Phase = Phase.PLAYER_FROM_HAND;
    }

    private void AddCardToMiddle(Card card)
    {
        GameObject middleCard = Instantiate(cardPrefab, new Vector2(0, 0), Quaternion.identity);
        RawImage image = middleCard.GetComponent<RawImage>();
        image.texture = card.FrontPic;
        middleCard.name = card.Id.ToString();
        middleCard.transform.SetParent(transform, false);
    }

    private void SetCardFromHandToAside(Card card)
    {
        GameObject asideCard = Instantiate(cardPrefab, new Vector2(0, 0), Quaternion.identity);
        RawImage image = asideCard.GetComponent<RawImage>();
        image.texture = card.FrontPic;
        asideCard.name = card.Id.ToString();
        asideCard.transform.SetParent(transform, false);
        SetAiCardAside(asideCard, card.Type);
    }

    private void SetAiCardAside(GameObject card, CardType type)
    {
        switch (type)
        {
            case CardType.BRIGHT:
                card.transform.SetParent(OpponentBright.transform, false);
                break;
            case CardType.ANIMAL:
                card.transform.SetParent(OpponentAnimal.transform, false);
                break;
            case CardType.RIBBON:
                card.transform.SetParent(OpponentRibbon.transform, false);
                break;
            case CardType.CHAFF:
                card.transform.SetParent(OpponentChaff.transform, false);
                break;
        }
    }
}
