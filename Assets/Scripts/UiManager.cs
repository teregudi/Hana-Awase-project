using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    public GameEngine GE;
    public GameObject CardPrefab;
    public GameObject PlayerArea;
    public GameObject MiddleArea;
    public GameObject OpponentArea;
    public GameObject PlayerBright;
    public GameObject PlayerAnimal;
    public GameObject PlayerRibbon;
    public GameObject PlayerChaff;
    public GameObject OpponentBright;
    public GameObject OpponentAnimal;
    public GameObject OpponentRibbon;
    public GameObject OpponentChaff;
    public Texture BlackBack;

    void Start()
    {
        GE = GameEngine.getGE();
        PlayerArea = GameObject.Find("PlayerArea");
        MiddleArea = GameObject.Find("MiddleArea");
        OpponentArea = GameObject.Find("OpponentArea");
        PlayerBright = GameObject.Find("PlayerBright");
        PlayerAnimal = GameObject.Find("PlayerAnimal");
        PlayerRibbon = GameObject.Find("PlayerRibbon");
        PlayerChaff = GameObject.Find("PlayerChaff");
        OpponentBright = GameObject.Find("OpponentBright");
        OpponentAnimal = GameObject.Find("OpponentAnimal");
        OpponentRibbon = GameObject.Find("OpponentRibbon");
        OpponentChaff = GameObject.Find("OpponentChaff");
    }

    public void OnClickOnCard()
    {
        if (this.transform.parent.name == PlayerArea.name && !GE.playerMustChoose)
        {
            ChooseFromHand();
        } 
        else if (this.transform.parent.name == MiddleArea.name)
        {
            if (!GE.playerMustChoose)
            {
                ChooseFromMiddle();
            }
            else
            {
                ChooseAfterDraw();
            }
        }
    }

    private void ChooseFromHand()
    {
        ClearSelection();
        MakeItRed(gameObject);
        SetItSelected(this.transform.name);

        foreach (var item in GE.State.CardsInMiddle.Where(c => c.Id / 10 == int.Parse(this.name) / 10))
        {
            var cardObject = GameObject.Find(item.Id.ToString());
            MakeItRed(cardObject);
            SetItSelected(cardObject.transform.name);
        }
    }

    private void ChooseFromMiddle()
    {
        var cardSelectedAtPlayer = GE.State.CardsAtPlayer.FirstOrDefault(c => c.IsSelected);
        if (cardSelectedAtPlayer == null)
            return;
        var cardsSelectedInMiddle = GE.State.CardsInMiddle.Where(c => c.IsSelected).ToList();
        var cardIdsSelectedInMiddle = cardsSelectedInMiddle.Select(c => c.Id.ToString());
        if (cardIdsSelectedInMiddle.Contains(this.name))
        {
            if (cardsSelectedInMiddle.Count() == 3)
            {
                for (int i = 0; i < 3; i++)
                {
                    var cardSelectedInMiddle = GameObject.Find(cardsSelectedInMiddle[i].Id.ToString());
                    var typeOfCard = cardsSelectedInMiddle[i].Type;
                    SetPlayerCardAside(cardSelectedInMiddle, typeOfCard);
                    GE.MoveCardToPlayerCollection(cardsSelectedInMiddle[i], true);
                }
            }
            else
            {
                var cardSelectedInMiddle = cardsSelectedInMiddle.First(c => c.Id == int.Parse(this.name));
                var typeOfCard = cardSelectedInMiddle.Type;
                SetPlayerCardAside(this.gameObject, typeOfCard);
                GE.MoveCardToPlayerCollection(cardSelectedInMiddle, true);
            }
            var cardObject = GameObject.Find(cardSelectedAtPlayer.Id.ToString());
            var cardType = cardSelectedAtPlayer.Type;
            SetPlayerCardAside(cardObject, cardType);
            GE.MoveCardToPlayerCollection(cardSelectedAtPlayer, false);
        }
        ClearSelection();
        FlipTopCard();
        //ha csak kettõ same month van középen, akkor 1 másodperc után mehet a gyûjteménybe
        //ha több, akkor meg kell várni, amíg a user választ
        HandleFlippedCard();
    }

    private void ChooseAfterDraw()
    {
        var clickedCard = GE.State.CardsInMiddle.First(c => c.Id == int.Parse(this.name));
        if (clickedCard.IsSelected)
        {
            var matchingCards = GE.State.CardsInMiddle.Where(c => c.IsSelected);
            if (matchingCards.Count() == 3)
            {
                foreach (var item in matchingCards)
                {
                    GameObject matchingObject = GameObject.Find(item.Id.ToString());
                    SetPlayerCardAside(matchingObject, item.Type);
                    GE.MoveCardToPlayerCollection(item, true);
                }
            } else
            {
                GameObject clickedObject = GameObject.Find(clickedCard.Id.ToString());
                SetPlayerCardAside(clickedObject, clickedCard.Type);
                GE.MoveCardToPlayerCollection(clickedCard, true);
            }
            GameObject drawnCard = Instantiate(CardPrefab, new Vector2(0, 0), Quaternion.identity);
            RawImage image = drawnCard.GetComponent<RawImage>();
            image.texture = GE.DrawnCard.FrontPic;
            drawnCard.name = GE.DrawnCard.Id.ToString();
            SetPlayerCardAside(drawnCard, GE.DrawnCard.Type);
            GE.MoveDrawnCardToPlayerCollection();

            GameObject deckCard = GameObject.Find("deck");
            image = deckCard.GetComponent<RawImage>();
            image.texture = BlackBack;

            GE.playerMustChoose = false;

            //AI move controlled from here
            HandleAiMove();
        }
    }

    public void OnClickOnMiddleArea()
    {
        var card = GE.State.CardsAtPlayer.FirstOrDefault(c => c.IsSelected);
        var selectedInMiddle = GE.State.CardsInMiddle.Where(c => c.IsSelected).Count();
        if (card != null && selectedInMiddle < 1)
        {
            var cardObject = GameObject.Find(card.Id.ToString());
            cardObject.transform.SetParent(MiddleArea.transform, false);
            ClearSelection();
            GE.MoveCardFromPlayerToMiddle(card);
            //thread.sleep
            FlipTopCard();
            HandleFlippedCard();
        }
    }

    private void ClearSelection()
    {
        foreach (var item in GE.State.CardsAtPlayer.Where(c => c.IsSelected))
        {
            MakeItBlackAtPlayer(GameObject.Find(item.Id.ToString()));
        }
        foreach (var item in GE.State.CardsInMiddle.Where(c => c.IsSelected))
        {
            MakeItBlackInMiddle(GameObject.Find(item.Id.ToString()));
        }
    }

    private void SetItSelected(string name)
    {
        int id = int.Parse(name);
        var card = GE.State.CardsInMiddle.FirstOrDefault(c => c.Id == id);
        if (card != null) card.IsSelected = true;
        card = GE.State.CardsAtPlayer.FirstOrDefault(c => c.Id == id);
        if (card != null) card.IsSelected = true;
    }

    private void MakeItRed(GameObject card)
    {
        int value = int.Parse(card.transform.name);
        foreach (var item in GE.RedDeck)
        {
            if (item.Id == value)
            {
                RawImage image = card.transform.GetComponent<RawImage>();
                image.texture = item.FrontPic;
                return;
            }
        }
    }

    private void MakeItBlackInMiddle(GameObject card)
    {
        int value = int.Parse(card.transform.name);
        foreach (var item in GE.State.CardsInMiddle)
        {
            if (item.Id == value)
            {
                RawImage image = card.transform.GetComponent<RawImage>();
                image.texture = item.FrontPic;
                item.IsSelected = false;
                return;
            }
        }
    }

    private void MakeItBlackAtPlayer(GameObject card)
    {
        int value = int.Parse(card.transform.name);
        foreach (var item in GE.State.CardsAtPlayer)
        {
            if (item.Id == value)
            {
                RawImage image = card.transform.GetComponent<RawImage>();
                image.texture = item.FrontPic;
                item.IsSelected = false;
                return;
            }
        }
    }

    private void SetPlayerCardAside(GameObject card, CardType type)
    {
        ClearSelection();
        switch (type)
        {
            case CardType.BRIGHT:
                card.transform.SetParent(PlayerBright.transform, false);
                break;
            case CardType.ANIMAL:
                card.transform.SetParent(PlayerAnimal.transform, false);
                break;
            case CardType.RIBBON:
                card.transform.SetParent(PlayerRibbon.transform, false);
                break;
            case CardType.CHAFF:
                card.transform.SetParent(PlayerChaff.transform, false);
                break;
        }
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

    private void FlipTopCard()
    {
        GE.DrawCard();

        GameObject deck = GameObject.Find("deck");
        RawImage image = deck.transform.GetComponent<RawImage>();
        image.texture = GE.DrawnCard.FrontPic;
    }

    private void HandleFlippedCard()
    {
        var matchingCards = GE.State.CardsInMiddle.Where(c => c.Month == GE.DrawnCard.Month);
        if (matchingCards.Any())
        {
            foreach (var item in matchingCards)
            {
                MakeItRed(GameObject.Find(item.Id.ToString()));
                item.IsSelected = true;
            }
            GE.playerMustChoose = true;
        } 
        else
        {
            GameObject cardToMiddle = Instantiate(CardPrefab, new Vector2(0, 0), Quaternion.identity);
            RawImage image = cardToMiddle.GetComponent<RawImage>();
            image.texture = GE.DrawnCard.FrontPic;
            cardToMiddle.name = GE.DrawnCard.Id.ToString();
            cardToMiddle.transform.SetParent(MiddleArea.transform, false);

            GameObject drawnCard = GameObject.Find("deck");
            image = drawnCard.GetComponent<RawImage>();
            image.texture = BlackBack;

            GE.MoveDrawnCardToMiddle();

            //AI move controlled from here
            HandleAiMove();
        }
    }

    private void HandleAiMove()
    {
        (List<Card> fromMiddle, List<Card> toMiddle) = GE.CalculateAiMove();
    }
}
