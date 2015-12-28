using Microsoft.VisualStudio.TestTools.UnitTesting;
using pectoludus;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace pectoludus.Tests
{
    [TestClass]
    public class TripleTriadGameContainerTests
    {
        [TestMethod]
        public void EmptyRulesetTripleTriadGameContainerTest()
        {
            TripleTriadGameContainer testContainer = new TripleTriadGameContainer(null);
        }

        [TestMethod]
        public void RulesetTripleTriadGameContainerTest() {
            var gameruleTypes =
                Enum.GetValues(typeof (TripleTriadGameContainer.GameruleType))
                    .Cast<TripleTriadGameContainer.GameruleType>()
                    .ToList();

            TripleTriadGameContainer testContainer = new TripleTriadGameContainer(gameruleTypes);
        }

        private static TripleTriadCard CreateUniformValuedCard(int value, TripleTriadCard.Category category = TripleTriadCard.Category.None) {
            return new TripleTriadCard("TestCard_" + value, new[] {value, value, value, value}, category);
        }

        private void CreateContainerForGame(List<TripleTriadGameContainer.GameruleType> rules, out TripleTriadGameContainer container, out TripleTriadHand npcHand, out TripleTriadHand playerHand) {
            container = new TripleTriadGameContainer(rules);

            container.AddPlayer(TripleTriadCard.Ownership.NPC);
            container.AddPlayer(TripleTriadCard.Ownership.Player);

            container.GetPlayerHand(TripleTriadCard.Ownership.NPC, out npcHand);
            container.GetPlayerHand(TripleTriadCard.Ownership.Player, out playerHand);

            Assert.AreEqual(0, container.GetCardCountForOwner(TripleTriadCard.Ownership.NPC));
            Assert.AreEqual(0, container.GetCardCountForOwner(TripleTriadCard.Ownership.Player));
        }



        [TestMethod]
        public void GetCardCountByOwnerTest() {
            TripleTriadGameContainer testContainer;
            TripleTriadHand npcHand, playerHand;
            
            CreateContainerForGame(null, out testContainer, out npcHand, out playerHand);

            Random rnd = new Random();
            int maxPossibleInsertions = TripleTriadGamegrid.FieldWidth*TripleTriadGamegrid.FieldHeight;
            int insertCount = rnd.Next(1, maxPossibleInsertions -1);
            int insertedCount = 0;

            for (int i = 0; i < TripleTriadGamegrid.FieldHeight; i++) {
                for (int j = 0; j < TripleTriadGamegrid.FieldWidth; j++) {
                    if (insertedCount < insertCount) {
                        npcHand.PlayCard(CreateUniformValuedCard(1, TripleTriadCard.Category.None), j, i);
                        insertedCount++;
                    }
                    else {
                        playerHand.PlayCard(CreateUniformValuedCard(1, TripleTriadCard.Category.None), j, i);
                    }
                }
            }

            Assert.AreEqual(insertCount, testContainer.GetCardCountForOwner(TripleTriadCard.Ownership.NPC));
            Assert.AreEqual(maxPossibleInsertions - insertCount, testContainer.GetCardCountForOwner(TripleTriadCard.Ownership.Player));
        }





        [TestMethod]
        public void GreaterThanGameRulePlayCardTest()
        {
            TripleTriadGameContainer testContainer;
            TripleTriadHand npcHand, playerHand;

            CreateContainerForGame(
                new List<TripleTriadGameContainer.GameruleType> {TripleTriadGameContainer.GameruleType.GreaterThan},
                out testContainer, out npcHand, out playerHand);

            int npcCount =0;
            int playerCount = 0;


            #region No Ownership Change Test

            for (int i = 0; i < TripleTriadGamegrid.FieldHeight; i+=2) {
                for (int j = 0; j < TripleTriadGamegrid.FieldWidth; j+=2) {
                    Assert.IsTrue(playerHand.PlayCard(CreateUniformValuedCard(1, TripleTriadCard.Category.None), j, i));
                    playerCount++;

                    if (j + 1 >= TripleTriadGamegrid.FieldWidth) continue;

                    Assert.IsTrue(npcHand.PlayCard(CreateUniformValuedCard(1, TripleTriadCard.Category.None), j + 1, i));
                    npcCount++;
                }

                if (i + 1 >= TripleTriadGamegrid.FieldHeight) continue;

                for (int j = 0; j < TripleTriadGamegrid.FieldWidth; j += 2) {
                    Assert.IsTrue(npcHand.PlayCard(CreateUniformValuedCard(1, TripleTriadCard.Category.None), j, i + 1));
                    npcCount++;

                    if (j + 1 >= TripleTriadGamegrid.FieldWidth) continue;

                    Assert.IsTrue(playerHand.PlayCard(CreateUniformValuedCard(1, TripleTriadCard.Category.None), j + 1, i + 1));
                    playerCount++;
                }
            }

            Assert.AreEqual(npcCount, testContainer.GetCardCountForOwner(TripleTriadCard.Ownership.NPC));
            Assert.AreEqual(playerCount, testContainer.GetCardCountForOwner(TripleTriadCard.Ownership.Player));

            // TODO: Check each card location to verify the cards have not changed ownership

            #endregion

            testContainer.ResetForNewGame();

            #region All Card Change to NPC Test

            npcCount = 9;
            playerCount = 0;

            // Place all of the Player cards before the NPC cards.
            for (int i = 0; i < TripleTriadGamegrid.FieldHeight; i += 2)
            {
                for (int j = 0; j < TripleTriadGamegrid.FieldWidth; j += 2)
                {
                    Assert.IsTrue(playerHand.PlayCard(CreateUniformValuedCard(1, TripleTriadCard.Category.None), j, i));

                }

                if (i + 1 >= TripleTriadGamegrid.FieldHeight) continue;

                for (int j = 0; j < TripleTriadGamegrid.FieldWidth; j += 2)
                {
                    if (j + 1 >= TripleTriadGamegrid.FieldWidth) continue;

                    Assert.IsTrue(playerHand.PlayCard(CreateUniformValuedCard(1, TripleTriadCard.Category.None), j + 1, i + 1));
                }
            }
            // Place all the NPC cards after the Player cards.
            for (int i = 0; i < TripleTriadGamegrid.FieldHeight; i += 2) {
                for (int j = 0; j < TripleTriadGamegrid.FieldWidth; j += 2) {

                    if (j + 1 >= TripleTriadGamegrid.FieldWidth) continue;

                    Assert.IsTrue(npcHand.PlayCard(CreateUniformValuedCard(2, TripleTriadCard.Category.None), j + 1, i));
                }
                if (i + 1 >= TripleTriadGamegrid.FieldHeight) continue;

                for (int j = 0; j < TripleTriadGamegrid.FieldWidth; j += 2) {
                    Assert.IsTrue(npcHand.PlayCard(CreateUniformValuedCard(2, TripleTriadCard.Category.None), j, i + 1));
                }
            }

            Assert.AreEqual(npcCount, testContainer.GetCardCountForOwner(TripleTriadCard.Ownership.NPC));
            Assert.AreEqual(playerCount, testContainer.GetCardCountForOwner(TripleTriadCard.Ownership.Player));

            #endregion
        }

        [TestMethod]
        public void PlusGameRulePlayCardTest() {
            TripleTriadGameContainer testContainer;
            TripleTriadHand npcHand, playerHand;

            CreateContainerForGame(
                new List<TripleTriadGameContainer.GameruleType> { TripleTriadGameContainer.GameruleType.Plus },
                out testContainer, out npcHand, out playerHand);
            Assert.Inconclusive("Not Implemented");
        }

        [TestMethod]
        public void ResetForNewGameTest() {
            Assert.Inconclusive("Not Implemented");
        }

        [TestMethod]
        public void CardCategoryPlayCardTest() {
            TripleTriadGameContainer testContainer;
            TripleTriadHand npcHand, playerHand;

            CreateContainerForGame(null, out testContainer, out npcHand, out playerHand);

            Random rnd = new Random();

            int max = TripleTriadGamegrid.FieldWidth*TripleTriadGamegrid.FieldHeight;
            var categories = Enum.GetValues(typeof (TripleTriadCard.Category));
            // Account for at least 1 per category
            int remaining = max-categories.Length;
            Assert.IsTrue(remaining > 0);

            var expectedCardCountByCategory = new Dictionary<TripleTriadCard.Category, int>();
            
            foreach (TripleTriadCard.Category family in categories) {
                int count = rnd.Next(1, Math.Max(remaining, 1));
                // Re-add the one per category
                remaining -= (count-1);
                expectedCardCountByCategory.Add(family,count);
            }

            var cardCountByCategory = new Dictionary<TripleTriadCard.Category, int>(expectedCardCountByCategory);
            int categoryIndex = 0;
            for (int i = 0; i < TripleTriadGamegrid.FieldHeight; i++) {
                if (categoryIndex >= categories.Length)
                    break;
                for (int j = 0; j < TripleTriadGamegrid.FieldWidth; j++) {
                    
                    if (cardCountByCategory[(TripleTriadCard.Category) categoryIndex] < 1)
                        categoryIndex++;
                    if(categoryIndex >= categories.Length)
                        break;

                    cardCountByCategory[(TripleTriadCard.Category) categoryIndex]--;
                    npcHand.PlayCard(CreateUniformValuedCard(1, (TripleTriadCard.Category) categoryIndex), j, i);
                }
            }

            foreach (var category in expectedCardCountByCategory) {
                Assert.AreEqual(category.Value, testContainer.GetCardCountForCategory(category.Key));
            }
        }
        [TestMethod]
        public void AddPlayerTest()
        {
            TripleTriadGameContainer testContainer = new TripleTriadGameContainer(null);
            var players = Enum.GetValues(typeof (TripleTriadCard.Ownership)).Cast<TripleTriadCard.Ownership>().ToList();

            foreach (var player in players) {
                testContainer.AddPlayer(player);
                var attachedPlayer = testContainer.GetPlayers();
                Assert.IsTrue(attachedPlayer.Contains(player));
            }
        }

        [TestMethod]
        public void GetPlayerHandTest()
        {
            TripleTriadGameContainer testContainer = new TripleTriadGameContainer(null);
            var players = Enum.GetValues(typeof(TripleTriadCard.Ownership)).Cast<TripleTriadCard.Ownership>().ToList();

            foreach (var player in players)
            {
                testContainer.AddPlayer(player);

                TripleTriadHand playerHand;
                testContainer.GetPlayerHand(player, out playerHand);

                Assert.IsNotNull(playerHand);
                Assert.AreEqual(playerHand.Owner, player);
            }

        }

        [TestMethod]
        public void DrawCurrentGameTest()
        {
            Assert.Inconclusive("[Visual] Method is not computer verifyable");
        }
    }
}