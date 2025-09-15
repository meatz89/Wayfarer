using System;
using System.Collections.Generic;
using Wayfarer.GameState.Enums;
using NUnit.Framework;

namespace Wayfarer.Tests
{
    [TestFixture]
    public class CardLevelingSystemTest
    {
        [Test]
        public void CardLevel_CalculatesCorrectlyFromXP()
        {
            // Create a card instance
            var card = new ConversationCard
            {
                Id = "test_card",
                Description = "Test Card",
                Persistence = PersistenceType.Impulse,
                Difficulty = Difficulty.Medium,
                Focus = 2
            };

            var instance = new CardInstance(card);

            // Test level progression
            Assert.Equal(1, instance.Level); // 0 XP = Level 1

            instance.XP = 2;
            Assert.Equal(1, instance.Level); // 2 XP = Level 1

            instance.XP = 3;
            Assert.Equal(2, instance.Level); // 3 XP = Level 2

            instance.XP = 7;
            Assert.Equal(3, instance.Level); // 7 XP = Level 3

            instance.XP = 15;
            Assert.Equal(4, instance.Level); // 15 XP = Level 4

            instance.XP = 30;
            Assert.Equal(5, instance.Level); // 30 XP = Level 5

            instance.XP = 50;
            Assert.Equal(5, instance.Level); // 50 XP = Level 5

            instance.XP = 75;
            Assert.Equal(6, instance.Level); // 75 XP = Level 6

            instance.XP = 100;
            Assert.Equal(7, instance.Level); // 100 XP = Level 7
        }

        [Test]
        public void Level3Card_GainsThoughtPersistence()
        {
            // Create an Impulse card
            var card = new ConversationCard
            {
                Id = "test_card",
                Description = "Test Card",
                Persistence = PersistenceType.Impulse,
                Difficulty = Difficulty.Medium,
                Focus = 2
            };

            var instance = new CardInstance(card);

            // At level 1, it should be Impulse
            Assert.Equal(PersistenceType.Impulse, instance.Persistence);

            // At level 3, it should become Thought
            instance.XP = 7;
            Assert.Equal(3, instance.Level);
            Assert.Equal(PersistenceType.Thought, instance.Persistence);
        }

        [Test]
        public void Level5Card_IgnoresFailureListen()
        {
            var card = new ConversationCard
            {
                Id = "test_card",
                Description = "Test Card",
                Persistence = PersistenceType.Impulse,
                Difficulty = Difficulty.Medium,
                Focus = 2
            };

            var instance = new CardInstance(card);

            // Below level 5, should not ignore failure listen
            instance.XP = 29;
            Assert.Equal(4, instance.Level);
            Assert.False(instance.IgnoresFailureListen);

            // At level 5, should ignore failure listen
            instance.XP = 30;
            Assert.Equal(5, instance.Level);
            Assert.True(instance.IgnoresFailureListen);
        }

        [Test]
        public void CardXP_IncrementsOnSuccess()
        {
            var card = new ConversationCard
            {
                Id = "test_card",
                Description = "Test Card",
                Persistence = PersistenceType.Impulse,
                Difficulty = Difficulty.Medium,
                Focus = 2
            };

            var instance = new CardInstance(card);

            // Start at 0 XP
            Assert.Equal(0, instance.XP);

            // Simulate successful play
            instance.XP += 1;
            Assert.Equal(1, instance.XP);

            // Multiple successes
            instance.XP += 1;
            instance.XP += 1;
            Assert.Equal(3, instance.XP);
            Assert.Equal(2, instance.Level); // Should be level 2 at 3 XP
        }

        [Test]
        public void LevelBonuses_ParseFromDTO()
        {
            var dto = new ConversationCardDTO
            {
                Id = "test_card",
                Description = "Test Card",
                Persistence = "Impulse",
                Difficulty = "Medium",
                Focus = 2,
                LevelBonuses = new List<CardLevelBonusDTO>
                {
                    new CardLevelBonusDTO
                    {
                        Level = 2,
                        SuccessBonus = 10
                    },
                    new CardLevelBonusDTO
                    {
                        Level = 3,
                        AddPersistence = "Thought"
                    },
                    new CardLevelBonusDTO
                    {
                        Level = 5,
                        IgnoreFailureListen = true
                    }
                }
            };

            var card = ConversationCardParser.ConvertDTOToCard(dto);

            Assert.Equal(3, card.LevelBonuses.Count);
            Assert.Equal(10, card.LevelBonuses[0].SuccessBonus);
            Assert.Equal(PersistenceType.Thought, card.LevelBonuses[1].AddPersistence);
            Assert.True(card.LevelBonuses[2].IgnoreFailureListen);
        }
    }
}