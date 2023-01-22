using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Assets.Scripts.Tests
{
    [TestFixture]
    public class ExpectiminimaxTest
    {
        [Test]
        public void Test()
        {
            // ARRANGE
            List<Card> cardsAtAI = new List<Card>()
            {
                new Card(null, 122, CardType.CHAFF, Month.DECEMBER),
                new Card(null, 123, CardType.CHAFF, Month.DECEMBER),
                new Card(null, 124, CardType.CHAFF, Month.DECEMBER),
                new Card(null, 13, CardType.CHAFF, Month.JANUARY),

            };
            List<Card> cardsInMiddle = new List<Card>()
            {
                new Card(null, 42, CardType.RIBBON, Month.APRIL),
                new Card(null, 43, CardType.CHAFF, Month.APRIL),
                new Card(null, 44, CardType.CHAFF, Month.APRIL),
                new Card(null, 11, CardType.BRIGHT, Month.JANUARY),

            };
            StateSpace initialState = new StateSpace()
            {
                CardsAtAI = cardsAtAI,
                CardsInMiddle = cardsInMiddle,
            };
            Node initialNode = new Node(initialState, NodeType.MAX);

            int expectedValue = 21;

            // ACT
            int actualValue = Expectiminimax.CalculateNodeValue(initialNode, 1);

            // ASSERT
            Assert.Equals(expectedValue, actualValue);
        }
    }
}
