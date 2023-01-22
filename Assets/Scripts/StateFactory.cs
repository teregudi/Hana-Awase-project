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
                    return CreateStatesForAI(InitialState.CardsAtAI);
                case NodeType.MIN:
                    return CreateStatesForPlayer();
                    XXXXXXXXXXXXX
                    // szar az egész, mert a gép nem tudhatja, milyen lapjai vannak a playernek
                case NodeType.CHANCE_AFTER_MAX:
                    return CreateStatesByDrawingFromDeck();
                case NodeType.CHANCE_AFTER_MIN:
                    return CreateStatesByDrawingFromDeck();
                default:
                    return new List<StateSpace>();
            }
        }

        private List<StateSpace> CreateStatesForAI(List<Card> cardsInHand)
        {
            // ha már elfogytak a lapok a kézből
            if (cardsInHand.Count == 0)
                return new List<StateSpace> { (StateSpace)InitialState.Clone() };

            List<StateSpace> states = new List<StateSpace>();
            foreach (var card in cardsInHand)
            {
                StateSpace clone = (StateSpace)InitialState.Clone();
                // kiveszi a lapot a kézből
                if (NodeType == NodeType.MAX)
                    clone.CardsAtAI.Remove(card);
                else if (NodeType == NodeType.MIN)
                    clone.CardsAtPlayer.Remove(card);

                var matchingCards = InitialState.CardsInMiddle.Where(c => c.Month == card.Month);
                // ha nincs match, bedobja középre
                if (matchingCards.Count() == 0)
                {
                    clone.CardsInMiddle.Add(card);
                    states.Add(clone);
                }
                // ha 1 vagy 3 match van, megy mind a collectionbe
                else if (matchingCards.Count() == 1 || matchingCards.Count() == 3)
                {
                    if (NodeType == NodeType.MAX)
                    {
                        clone.CardsCollectedByAI.Add(card);
                        clone.CardsCollectedByAI.AddRange(matchingCards);
                    }
                    else if (NodeType == NodeType.MIN)
                    {
                        clone.CardsCollectedByPlayer.Add(card);
                        clone.CardsCollectedByPlayer.AddRange(matchingCards);
                    }
                    states.Add(clone);
                }
                // ha 2 match van, akkor két külön state keletkezik a választástól függően
                else if (matchingCards.Count() == 2)
                {
                    foreach (var matchingCard in matchingCards)
                    {
                        StateSpace cloneVariant = (StateSpace)clone.Clone();
                        if (NodeType == NodeType.MAX)
                        {
                            cloneVariant.CardsCollectedByAI.Add(card);
                            cloneVariant.CardsCollectedByAI.Add(matchingCard);
                        }
                        else if (NodeType == NodeType.MIN)
                        {
                            cloneVariant.CardsCollectedByPlayer.Add(card);
                            cloneVariant.CardsCollectedByPlayer.Add(matchingCard);
                        }
                        states.Add(cloneVariant);
                    }
                }
            }
            return states;
        }

        private List<StateSpace> CreateStatesForPlayer()
        {
            List<Card> allUnknownCards = GetAllUnknownCards();
            //pesszimista algoritmus
            //csak azokkal az esetekkel számol, amikor a player el tud vinni egy-egy lapot középről
            //és ehhez még probabilyt kellene számolni
            var playableCards = allUnknownCards.Where(u => InitialState.CardsInMiddle.Any(m => m.Month == u.Month));
            List<StateSpace> states = new List<StateSpace>();
            foreach (var card in playableCards)
            {
                StateSpace clone = (StateSpace)InitialState.Clone();
                var matchingCards = InitialState.CardsInMiddle.Where(c => c.Month == card.Month);
                clone.CardsCollectedByPlayer.Add(card);
                clone.CardsCollectedByPlayer.AddRange(matchingCards);
                clone.Probability = 1 / allUnknownCards.Count();
                states.Add(clone);
            }
            return states;
        }

        private List<StateSpace> CreateStatesByDrawingFromDeck()
        {
            List<Card> allUnknownCards = GetAllUnknownCards();

            List<StateSpace> states = new List<StateSpace>();
            foreach (var drawnCard in allUnknownCards)
            {
                StateSpace clone = (StateSpace)InitialState.Clone();
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
                        states.Add(clone);
                    }
                }
            }
            return states;
        }

        private List<Card> GetAllUnknownCards()
        {
            List<Card> allUnknownCards = new List<Card>();
            foreach (var card in GameEngine.FULL_DECK)
                allUnknownCards.Add(card);
            foreach (var card in InitialState.CardsAtAI)
                allUnknownCards.Remove(card);
            foreach (var card in InitialState.CardsCollectedByAI)
                allUnknownCards.Remove(card);
            foreach (var card in InitialState.CardsCollectedByPlayer)
                allUnknownCards.Remove(card);
            foreach (var card in InitialState.CardsInMiddle)
                allUnknownCards.Remove(card);
            return allUnknownCards;
        }
    }
}
