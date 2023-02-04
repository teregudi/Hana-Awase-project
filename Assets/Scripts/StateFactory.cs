using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    return CalculateStatesAccordingToMatches(cardsAndTheirMatches.First());
                }
                // kivesszük a listából azokat, amiket egyébként is csak az AI tudna begyűjteni, mert az ráér később is
                TruncateDictionaryForVariousReasons(cardsAndTheirMatches);
                if (cardsAndTheirMatches.Count == 1)
                {
                    return CalculateStatesAccordingToMatches(cardsAndTheirMatches.First());
                }
                // ha még így is több lehetőség van, akkor mindet sorra vesszük
                foreach (var cardAndMatch in cardsAndTheirMatches)
                {
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
                        clone = (StateSpace)InitialState.Clone();
                        clone.CardsCollectedByPlayer.Add(cardFromPlayerHand);
                        clone.CardsCollectedByPlayer.Add(matchingCard);
                        clone.CardsInMiddle.Remove(matchingCard);
                        clone.CardsAtPlayer.RemoveAt(0);
                        states.Add(clone);
                    }
                }
            }
            // egy kis trükközés, talán beválik
            return states.OrderByDescending(s => s.PlayerScoreCurrently).Take(5).ToList();
        }

        private List<StateSpace> CreateStatesByDrawingFromDeck()
        {
            var allUnknownCards = GetAllUnknownCards();

            List<StateSpace> states = new List<StateSpace>();
            foreach (var drawnCard in allUnknownCards)
            {
                StateSpace clone = (StateSpace)InitialState.Clone();
                // na ez itt még egyáltalán nem biztos hogy jó
                clone.Probability = 1 / allUnknownCards.Count();
                var matchingCards = InitialState.CardsInMiddle.Where(c => c.Month == drawnCard.Month);
                if (matchingCards.Count() == 0)
                {
                    clone.CardsInMiddle.Add(drawnCard);
                    states.Add(clone);
                    continue;
                }
                else if (matchingCards.Count() == 1 || matchingCards.Count() == 3)
                {
                    if (NodeType == NodeType.CHANCE_AFTER_MAX)
                    {
                        clone.CardsCollectedByAI.Add(drawnCard);
                        clone.CardsCollectedByAI.AddRange(matchingCards);
                    }
                    if (NodeType == NodeType.CHANCE_AFTER_MIN)
                    {
                        clone.CardsCollectedByPlayer.Add(drawnCard);
                        clone.CardsCollectedByPlayer.AddRange(matchingCards);
                    }
                    clone.CardsInMiddle.RemoveAll(m => m.Month == drawnCard.Month);
                    states.Add(clone);
                }
                else if (matchingCards.Count() == 2)
                {
                    foreach (var matchingCard in matchingCards)
                    {
                        clone = (StateSpace)InitialState.Clone();
                        if (NodeType == NodeType.CHANCE_AFTER_MAX)
                        {
                            clone.CardsCollectedByAI.Add(drawnCard);
                            clone.CardsCollectedByAI.Add(matchingCard);
                        }
                        else if (NodeType == NodeType.CHANCE_AFTER_MIN)
                        {
                            clone.CardsCollectedByPlayer.Add(drawnCard);
                            clone.CardsCollectedByPlayer.Add(matchingCard);
                        }
                        clone.CardsInMiddle.Remove(matchingCard);
                        states.Add(clone);
                    }
                }
            }
            // egy kis trükközés, talán beválik
            if (NodeType == NodeType.CHANCE_AFTER_MAX)
                return states.OrderBy(s => s.AiScoreCurrently).Take(5).ToList();
            else
                return states.OrderByDescending(s => s.PlayerScoreCurrently).Take(5).ToList();
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
            // itt még lehetne okosítani a választáson mindenféle logika szerint
            List<Card> lessValuableCards = new List<Card>();
            var cardsMinusBrights = cards.Where(c => c.Type != CardType.BRIGHT);
            if (!cardsMinusBrights.Any()) return cards;
            var cardsMinusAnimals = cardsMinusBrights.Where(c => c.Type != CardType.ANIMAL);
            if (!cardsMinusAnimals.Any()) return cardsMinusBrights.ToList();
            var cardsMinusRibbons = cardsMinusAnimals.Where(c => c.Type != CardType.RIBBON);
            if (!cardsMinusRibbons.Any()) return cardsMinusAnimals.ToList();
            return cardsMinusRibbons.ToList();
        }
    }
}
