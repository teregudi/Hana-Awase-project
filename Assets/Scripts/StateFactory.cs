using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts
{
    public class StateFactory
    {
        public StateSpace InitialState { private get; set; }
        public NodeType NodeType { private get; set; }

        public List<StateSpace> CreatePossibleStates()
        {
            switch (NodeType)
            {
                case NodeType.MAX:
                    return CreateStatesForAI();
                case NodeType.MIN:
                    return CreateStatesForPlayer();
                case NodeType.CHANCE_AFTER_MAX:
                    return CreateStatesByDrawingFromDeck();
                case NodeType.CHANCE_AFTER_MIN:
                    return CreateStatesByDrawingFromDeck();
                default:
                    return new List<StateSpace>();
            }
        }

        private List<StateSpace> CreateStatesForAI()
        {
            // ha már elfogytak a lapok a kézből, akkor egyszerűen visszaadjuk a jelenlegi state-et, jöhet a húzás
            if (InitialState.CardsAtAI.Count == 0)
                return new List<StateSpace> { (StateSpace)InitialState.Clone() };

            List<StateSpace> states = new List<StateSpace>();

            // kiválogatjuk a lapokat a kézből, amiknek van párja középen
            Dictionary<Card, IEnumerable<Card>> cardsAndTheirMatches = SelectAiCardsThatHavePairInMiddle();

            if (cardsAndTheirMatches.Any())
            {
                // ha csak egy ilyen van, akkor mindenképp azt játszuk ki
                if (cardsAndTheirMatches.Count == 1)
                {
                    // persze külön kell kezelni azt az esetet, ha középről csak 1-et vagy 3-at tud elvinni, vagy még választania is kell 2 közül
                    Debug.Log("1. Ezt játsza ki: " + cardsAndTheirMatches.First());
                    return CalculateStatesAccordingToMatches(cardsAndTheirMatches.First());
                }
                // kivesszük a listából azokat, amiket egyébként is csak az AI tudna begyűjteni, mert az ráér később is
                TruncateDictionaryForVariousReasons(cardsAndTheirMatches);
                if (cardsAndTheirMatches.Count == 1)
                {
                    Debug.Log("2. Ezt játsza ki: " + cardsAndTheirMatches.First());
                    return CalculateStatesAccordingToMatches(cardsAndTheirMatches.First());
                }
                // ha még így is több lehetőség van, akkor mindet sorra vesszük
                foreach (var cardAndMatch in cardsAndTheirMatches)
                {
                    Debug.Log("3. Ezt játsza ki: " + cardAndMatch);
                    states.AddRange(CalculateStatesAccordingToMatches(cardAndMatch));
                }
                return states;
            }
            // ha nincs match, akkor jöhet a dobás, de az értékes lapokat lehetőleg megtartjuk
            List<Card> cardsToDrop = SelectLessValuableCardsToDrop(InitialState.CardsAtAI);
            foreach (var cardToDrop in cardsToDrop)
            {
                StateSpace clone = (StateSpace)InitialState.Clone();
                clone.CardsAtAI.Remove(cardToDrop);
                clone.CardsInMiddle.Add(cardToDrop);
                states.Add(clone);
            }
            return states;
        }

        private List<StateSpace> CreateStatesForPlayer()
        {
            // ha már elfogytak a lapok a kézből, akkor egyszerűen visszaadjuk a jelenlegi state-et, jöhet a húzás
            if (InitialState.CardsAtPlayer.Count == 0)
                return new List<StateSpace> { (StateSpace)InitialState.Clone() };

            var allUnknownCards = GetAllUnknownCards();
            // mivel MIN ágon úgyis a legrosszabb eshetőséget vesszük,
            // ezért csak azokat az eseteket vizsgáljuk, ahol a player el tud vinni valamit középről
            var playableCards = allUnknownCards.Where(u => InitialState.CardsInMiddle.Any(m => m.Month == u.Month));

            List<StateSpace> states = new List<StateSpace>();
            foreach (var cardFromPlayerHand in playableCards)
            {
                StateSpace clone = (StateSpace)InitialState.Clone();
                var matchingCards = InitialState.CardsInMiddle.Where(c => c.Month == cardFromPlayerHand.Month);
                if (matchingCards.Count() == 1 || matchingCards.Count() == 3)
                {
                    clone.CardsCollectedByPlayer.Add(cardFromPlayerHand);
                    clone.CardsCollectedByPlayer.AddRange(matchingCards);
                    clone.CardsInMiddle.RemoveAll(m => m.Month == cardFromPlayerHand.Month);
                    clone.CardsAtPlayer.RemoveAt(0); // csak hogy csökkenjen a lapjai száma
                    states.Add(clone);
                }
                else if (matchingCards.Count() == 2)
                {
                    foreach (var matchingCard in matchingCards)
                    {
                        StateSpace cloneForMatch = (StateSpace)InitialState.Clone();
                        cloneForMatch.CardsCollectedByPlayer.Add(cardFromPlayerHand);
                        cloneForMatch.CardsCollectedByPlayer.Add(matchingCard);
                        cloneForMatch.CardsInMiddle.Remove(matchingCard);
                        cloneForMatch.CardsAtPlayer.RemoveAt(0);
                        states.Add(cloneForMatch);
                    }
                }
            }
            // egy kis trükközés, talán beválik
            return states.OrderByDescending(s => s.PlayerScoreCurrently).Take(3).ToList();
        }

        private List<StateSpace> CreateStatesByDrawingFromDeck()
        {
            List<StateSpace> states = new List<StateSpace>();
            StateSpace clone = (StateSpace)InitialState.Clone();

            if (NodeType == NodeType.CHANCE_AFTER_MAX)
            {
                Card cardToDrop = CalculateLessFavourableCardForAi();
                clone.CardsInMiddle.Add(cardToDrop);
                states.Add(clone);
                return states;
            }

            if (NodeType == NodeType.CHANCE_AFTER_MIN)
            {
                (Card, List<Card>) cardAndMatches = CalculateMostFavourableCardForPlayer();
                if (cardAndMatches == (null, null))
                {
                    states.Add(clone);
                    return states;
                }
                clone.CardsCollectedByPlayer.Add(cardAndMatches.Item1);
                clone.CardsCollectedByPlayer.AddRange(cardAndMatches.Item2);
                foreach (var match in cardAndMatches.Item2)
                {
                    clone.CardsInMiddle.Remove(match);
                }
                states.Add(clone);
                return states;
            }

            states.Add(clone);
            return states;
        }

        private IEnumerable<Card> GetAllUnknownCards()
        {
            return GameEngine.FULL_DECK.Where(c =>
                !InitialState.CardsAtAI.Contains(c) &&
                !InitialState.CardsCollectedByAI.Contains(c) &&
                !InitialState.CardsCollectedByPlayer.Contains(c) &&
                !InitialState.CardsInMiddle.Contains(c));
        }

        private Dictionary<Card, IEnumerable<Card>> SelectAiCardsThatHavePairInMiddle()
        {
            Dictionary<Card, IEnumerable<Card>> cardsAndTheirMatches = new Dictionary<Card, IEnumerable<Card>>();
            foreach (Card card in InitialState.CardsAtAI)
            {
                var matchingCards = InitialState.CardsInMiddle.Where(c => c.Month == card.Month);
                if (matchingCards.Any())
                    cardsAndTheirMatches.Add(card, matchingCards);
            }
            return cardsAndTheirMatches;
        }

        private void TruncateDictionaryForVariousReasons(Dictionary<Card, IEnumerable<Card>> cardsAndTheirMatches)
        {
            List<Card> cardsToRemove = new List<Card>();
            foreach (var item in cardsAndTheirMatches)
            {
                if (item.Value.Count() == 3)
                {
                    cardsToRemove.Add(item.Key);
                }
                else if (InitialState.CardsCollectedByAI.Any(c => c.Month == item.Key.Month) ||
                    InitialState.CardsCollectedByPlayer.Any(c => c.Month == item.Key.Month))
                {
                    cardsToRemove.Add(item.Key);
                }
            }
            if (cardsAndTheirMatches.Count != cardsToRemove.Count)
            {
                foreach (var card in cardsToRemove)
                {
                    cardsAndTheirMatches.Remove(card);
                }
            }
        }

        private List<StateSpace> CalculateStatesAccordingToMatches(KeyValuePair<Card, IEnumerable<Card>> cardsAndTheirMatches)
        {
            List<StateSpace> states = new List<StateSpace>();
            if (cardsAndTheirMatches.Value.Count() == 2)
            {
                foreach (var match in cardsAndTheirMatches.Value)
                {
                    StateSpace clone = (StateSpace)InitialState.Clone();
                    clone.CardsAtAI.Remove(cardsAndTheirMatches.Key);
                    clone.CardsCollectedByAI.Add(cardsAndTheirMatches.Key);
                    clone.CardsInMiddle.Remove(match);
                    clone.CardsCollectedByAI.Add(match);
                    states.Add(clone);
                }
                return states;
            }
            else
            {
                StateSpace clone = (StateSpace)InitialState.Clone();
                clone.CardsAtAI.Remove(cardsAndTheirMatches.Key);
                clone.CardsCollectedByAI.Add(cardsAndTheirMatches.Key);
                clone.CardsInMiddle.RemoveAll(c => c.Month == cardsAndTheirMatches.Key.Month);
                clone.CardsCollectedByAI.AddRange(cardsAndTheirMatches.Value);
                states.Add(clone);
                return states;
            }
        }

        private List<Card> SelectLessValuableCardsToDrop(List<Card> cards)
        {
            // itt lehetne az, hogy sorra vesszük a lapjainkat, egyesével hozzáadjuk azt a player collectionhöz,
            // és azt a lapot dobjuk ki, amivel a legkevesebb nyereséget okozzuk a playernek

            // először megnézzük, van-e olyan pár a kézben, aminek a másik két lapja már be van gyűjtve
            foreach (var item in cards)
            {
                if (cards.Where(c => c.Month == item.Month).Count() == 2 &&
                    (InitialState.CardsCollectedByPlayer.Any(c => c.Month == item.Month) ||
                    InitialState.CardsCollectedByAI.Any(c => c.Month == item.Month)))
                {
                    return new List<Card> { item };
                }
            }
            List<Card> lessValuableCards = new List<Card>();
            var cardsMinusBrights = cards.Where(c => c.Type != CardType.BRIGHT);
            if (!cardsMinusBrights.Any()) return cards;
            var cardsMinusAnimals = cardsMinusBrights.Where(c => c.Type != CardType.ANIMAL);
            if (!cardsMinusAnimals.Any()) return cardsMinusBrights.ToList();
            var cardsMinusRibbons = cardsMinusAnimals.Where(c => c.Type != CardType.RIBBON);
            if (!cardsMinusRibbons.Any()) return cardsMinusAnimals.ToList();
            return cardsMinusRibbons.ToList();
        }

        private (Card, List<Card>) CalculateMostFavourableCardForPlayer()
        {
            var unknownCards = GetAllUnknownCards();
            if (!unknownCards.Any())
            {
                return (null, null);
            }

            int maxValue = int.MinValue;
            Card cardToPlay = unknownCards.ElementAt(0);
            List<Card> matches = new List<Card>();
            foreach (var unknownCard in unknownCards)
            {
                var matchingCards = InitialState.CardsInMiddle.Where(c => c.Month == unknownCard.Month);
                if (matchingCards.Count() == 0)
                {
                    continue;
                }
                else if (matchingCards.Count() == 1 || matchingCards.Count() == 3)
                {
                    StateSpace clone = (StateSpace)InitialState.Clone();
                    clone.CardsCollectedByPlayer.Add(unknownCard);
                    clone.CardsCollectedByPlayer.AddRange(matchingCards);
                    if (clone.PlayerScoreCurrently > maxValue)
                    {
                        maxValue = clone.PlayerScoreCurrently;
                        cardToPlay = unknownCard;
                        matches = matchingCards.ToList();
                    }
                }
                else if (matchingCards.Count() == 2)
                {
                    foreach (var matchingCard in matchingCards)
                    {
                        StateSpace clone = (StateSpace)InitialState.Clone();
                        clone.CardsCollectedByPlayer.Add(unknownCard);
                        clone.CardsCollectedByPlayer.Add(matchingCard);
                        if (clone.PlayerScoreCurrently > maxValue)
                        {
                            maxValue = clone.PlayerScoreCurrently;
                            cardToPlay = unknownCard;
                            matches = new List<Card> { matchingCard };
                        }
                    }
                }
            }
            return (cardToPlay, matches);
        }

        private Card CalculateLessFavourableCardForAi()
        {
            var unknownCards = GetAllUnknownCards();

            var unknownMinusMiddle = unknownCards.Where(u => !InitialState.CardsInMiddle.Any(m => m.Month == u.Month));
            if (!unknownMinusMiddle.Any())
            {
                return unknownCards.First();
            }
            var unknownMinusAiHand = unknownMinusMiddle.Where(u => !InitialState.CardsAtAI.Any(a => a.Month == u.Month));
            if (!unknownMinusAiHand.Any())
            {
                return unknownMinusMiddle.First();
            }
            return unknownMinusAiHand.First();
        }

        private void CsakTeszt(List<StateSpace> newStates)
        {
            Debug.Log("Na ezeket játszotta ki a human képzeletben:");
            foreach (var card1 in InitialState.CardsAtPlayer)
            {
                bool played = true;
                foreach (var card2 in newStates[0].CardsAtPlayer)
                {
                    if (card1 == card2) played = false;
                }
                if (played)
                {
                    Debug.Log(card1);
                }
            }
            foreach (var card1 in InitialState.CardsAtPlayer)
            {
                bool played = true;
                foreach (var card2 in newStates[1].CardsAtPlayer)
                {
                    if (card1 == card2)
                        played = false;
                }
                if (played)
                {
                    Debug.Log(card1);
                }
            }
        }
    }
}
