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
            foreach (var card in InitialState.CardsAtAI)
            {
                StateSpace clone = (StateSpace)InitialState.Clone();
                // kiveszi az aktuális lapot az AI "kezéből"
                clone.CardsAtAI.Remove(card);

                var matchingCards = InitialState.CardsInMiddle.Where(c => c.Month == card.Month);
                // ha nincs match, bedobja középre
                if (matchingCards.Count() == 0)
                {
                    clone.CardsInMiddle.Add(card);
                    states.Add(clone);
                }
                // ha 1 vagy 3 match van, megy minden a collectionbe
                else if (matchingCards.Count() == 1 || matchingCards.Count() == 3)
                {
                    clone.CardsCollectedByAI.Add(card);
                    clone.CardsCollectedByAI.AddRange(matchingCards);
                    states.Add(clone);
                }
                // ha 2 match van, akkor két külön state keletkezik a választástól függően
                else if (matchingCards.Count() == 2)
                {
                    foreach (var matchingCard in matchingCards)
                    {
                        StateSpace cloneVariant = (StateSpace)clone.Clone();
                        cloneVariant.CardsCollectedByAI.Add(card);
                        cloneVariant.CardsCollectedByAI.Add(matchingCard);
                        states.Add(cloneVariant);
                    }
                }
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
            return states;
        }

        private List<StateSpace> CreateStatesByDrawingFromDeck()
        {
            var allUnknownCards = GetAllUnknownCards();

            List<StateSpace> states = new List<StateSpace>();
            foreach (var drawnCard in allUnknownCards)
            {
                StateSpace clone = (StateSpace)InitialState.Clone();
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
    }
}
