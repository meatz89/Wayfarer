using System;
using NUnit.Framework;

namespace Wayfarer.Tests
{
    [TestFixture]
    public class AtmospherePersistenceTests
    {
        private AtmosphereManager CreateAtmosphereManager()
        {
            return new AtmosphereManager();
        }

        [Test]
        public void TestAtmospherePersistsThroughListen()
        {
            // Arrange
            var manager = CreateAtmosphereManager();
            
            // Act - Set atmosphere
            manager.SetAtmosphere(AtmosphereType.Prepared);
            Assert.AreEqual(AtmosphereType.Prepared, manager.CurrentAtmosphere);
            
            // Act - LISTEN should NOT change atmosphere
            manager.OnListenAction();
            
            // Assert - Atmosphere persists
            Assert.AreEqual(AtmosphereType.Prepared, manager.CurrentAtmosphere);
            Assert.AreEqual(1, manager.GetFocusCapacityBonus());
        }

        [Test]
        public void TestFailureClearsAtmosphere()
        {
            // Arrange
            var manager = CreateAtmosphereManager();
            
            // Act - Set atmosphere
            manager.SetAtmosphere(AtmosphereType.Volatile);
            Assert.AreEqual(AtmosphereType.Volatile, manager.CurrentAtmosphere);
            
            // Act - Failure clears atmosphere
            manager.ClearAtmosphereOnFailure();
            
            // Assert - Atmosphere reset to Neutral
            Assert.AreEqual(AtmosphereType.Neutral, manager.CurrentAtmosphere);
        }

        [Test]
        public void TestInformedAtmosphereAutoSuccess()
        {
            // Arrange
            var manager = CreateAtmosphereManager();
            
            // Act - Set Informed atmosphere
            manager.SetAtmosphere(AtmosphereType.Informed);
            
            // Assert - Should auto-succeed
            Assert.IsTrue(manager.ShouldAutoSucceed());
            
            // Act - Consume the effect after success
            manager.OnCardSuccess();
            
            // Assert - Auto-success consumed and atmosphere cleared
            Assert.IsFalse(manager.ShouldAutoSucceed());
            Assert.AreEqual(AtmosphereType.Neutral, manager.CurrentAtmosphere);
        }

        [Test]
        public void TestSynchronizedAtmosphereDoubleEffect()
        {
            // Arrange
            var manager = CreateAtmosphereManager();
            
            // Act - Set Synchronized atmosphere
            manager.SetAtmosphere(AtmosphereType.Synchronized);
            
            // Assert - Should double next effect
            Assert.IsTrue(manager.ShouldDoubleNextEffect());
            
            // Act - Consume the effect after success
            manager.OnCardSuccess();
            
            // Assert - Double effect consumed and atmosphere cleared
            Assert.IsFalse(manager.ShouldDoubleNextEffect());
            Assert.AreEqual(AtmosphereType.Neutral, manager.CurrentAtmosphere);
        }

        [Test]
        public void TestPreparedAtmosphereFocusBonus()
        {
            // Arrange
            var manager = CreateAtmosphereManager();
            
            // Act - Set Prepared atmosphere
            manager.SetAtmosphere(AtmosphereType.Prepared);
            
            // Assert - Focus capacity bonus
            Assert.AreEqual(1, manager.GetFocusCapacityBonus());
            
            // Act - LISTEN doesn't affect it
            manager.OnListenAction();
            Assert.AreEqual(1, manager.GetFocusCapacityBonus());
            
            // Act - Success doesn't clear it (persistent atmosphere)
            manager.OnCardSuccess();
            Assert.AreEqual(AtmosphereType.Prepared, manager.CurrentAtmosphere);
            Assert.AreEqual(1, manager.GetFocusCapacityBonus());
        }

        [Test]
        public void TestReceptiveAtmosphereDrawBonus()
        {
            // Arrange
            var manager = CreateAtmosphereManager();
            
            // Act - Set Receptive atmosphere
            manager.SetAtmosphere(AtmosphereType.Receptive);
            
            // Assert - Draw count modifier
            Assert.AreEqual(1, manager.GetDrawCountModifier());
            
            // Act - Persists through multiple listens
            manager.OnListenAction();
            Assert.AreEqual(1, manager.GetDrawCountModifier());
            manager.OnListenAction();
            Assert.AreEqual(1, manager.GetDrawCountModifier());
        }

        [Test]
        public void TestPressuredAtmosphereDrawPenalty()
        {
            // Arrange
            var manager = CreateAtmosphereManager();
            
            // Act - Set Pressured atmosphere
            manager.SetAtmosphere(AtmosphereType.Pressured);
            
            // Assert - Draw count penalty
            Assert.AreEqual(-1, manager.GetDrawCountModifier());
        }

        [Test]
        public void TestPatientAtmosphereWaivesPatience()
        {
            // Arrange
            var manager = CreateAtmosphereManager();
            
            // Act - Set Patient atmosphere
            manager.SetAtmosphere(AtmosphereType.Patient);
            
            // Assert - Should waive patience cost
            Assert.IsTrue(manager.ShouldWaivePatienceCost());
            
            // Act - Still waives on next check (persistent)
            Assert.IsTrue(manager.ShouldWaivePatienceCost());
        }

        [Test]
        public void TestVolatileAtmosphereModifiesFlow()
        {
            // Arrange
            var manager = CreateAtmosphereManager();
            manager.SetAtmosphere(AtmosphereType.Volatile);
            
            // Assert - Positive flow increased
            Assert.AreEqual(3, manager.ModifyFlowChange(2));
            
            // Assert - Negative flow decreased
            Assert.AreEqual(-3, manager.ModifyFlowChange(-2));
            
            // Assert - Zero stays zero
            Assert.AreEqual(0, manager.ModifyFlowChange(0));
        }

        [Test]
        public void TestExposedAtmosphereDoublesFlow()
        {
            // Arrange
            var manager = CreateAtmosphereManager();
            manager.SetAtmosphere(AtmosphereType.Exposed);
            
            // Assert - Doubles all flow changes
            Assert.AreEqual(4, manager.ModifyFlowChange(2));
            Assert.AreEqual(-4, manager.ModifyFlowChange(-2));
            Assert.AreEqual(0, manager.ModifyFlowChange(0));
        }

        [Test]
        public void TestFinalAtmosphereEndsOnFailure()
        {
            // Arrange
            var manager = CreateAtmosphereManager();
            
            // Act - Set Final atmosphere
            manager.SetAtmosphere(AtmosphereType.Final);
            
            // Assert - Should end on failure
            Assert.IsTrue(manager.ShouldEndOnFailure());
            
            // Act - Persists until actual failure
            manager.OnListenAction();
            Assert.IsTrue(manager.ShouldEndOnFailure());
        }

        [Test]
        public void TestFocusedAtmosphereSuccessBonus()
        {
            // Arrange
            var manager = CreateAtmosphereManager();
            
            // Act - Set Focused atmosphere
            manager.SetAtmosphere(AtmosphereType.Focused);
            
            // Assert - Success percentage bonus
            Assert.AreEqual(20, manager.GetSuccessPercentageBonus());
            
            // Act - Persists through actions
            manager.OnListenAction();
            Assert.AreEqual(20, manager.GetSuccessPercentageBonus());
            manager.OnCardSuccess();
            Assert.AreEqual(20, manager.GetSuccessPercentageBonus());
        }

        [Test]
        public void TestSpecialObservationEffects()
        {
            // Arrange
            var manager = CreateAtmosphereManager();
            
            // Act - Set next speak free
            manager.SetNextSpeakFree();
            Assert.IsTrue(manager.IsNextSpeakFree());
            
            // Assert - Consumed after use
            Assert.IsFalse(manager.IsNextSpeakFree());
            
            // Act - Set next action free patience
            manager.SetNextActionFreePatience();
            Assert.IsTrue(manager.ShouldWaivePatienceCost());
            
            // Assert - Consumed after use
            Assert.IsFalse(manager.ShouldWaivePatienceCost());
        }

        [Test]
        public void TestAtmosphereChangeOverridesExisting()
        {
            // Arrange
            var manager = CreateAtmosphereManager();
            
            // Act - Set initial atmosphere
            manager.SetAtmosphere(AtmosphereType.Prepared);
            Assert.AreEqual(AtmosphereType.Prepared, manager.CurrentAtmosphere);
            
            // Act - Change to different atmosphere
            manager.SetAtmosphere(AtmosphereType.Focused);
            
            // Assert - New atmosphere replaces old
            Assert.AreEqual(AtmosphereType.Focused, manager.CurrentAtmosphere);
            Assert.AreEqual(0, manager.GetFocusCapacityBonus()); // No longer Prepared
            Assert.AreEqual(20, manager.GetSuccessPercentageBonus()); // Now Focused
        }

        [Test]
        public void TestReset()
        {
            // Arrange
            var manager = CreateAtmosphereManager();
            manager.SetAtmosphere(AtmosphereType.Informed);
            manager.SetNextSpeakFree();
            
            // Act - Reset
            manager.Reset();
            
            // Assert - Everything cleared
            Assert.AreEqual(AtmosphereType.Neutral, manager.CurrentAtmosphere);
            Assert.IsFalse(manager.ShouldAutoSucceed());
            Assert.IsFalse(manager.IsNextSpeakFree());
            Assert.IsFalse(manager.HasTemporaryEffects());
        }

        [Test]
        public void TestTemporaryEffectsTracking()
        {
            // Arrange
            var manager = CreateAtmosphereManager();
            
            // Assert - No effects initially
            Assert.IsFalse(manager.HasTemporaryEffects());
            Assert.AreEqual("", manager.GetTemporaryEffectsDescription());
            
            // Act - Set Informed atmosphere
            manager.SetAtmosphere(AtmosphereType.Informed);
            
            // Assert - Has temporary effect
            Assert.IsTrue(manager.HasTemporaryEffects());
            Assert.IsTrue(manager.GetTemporaryEffectsDescription().Contains("guaranteed success"));
            
            // Act - Consume effect
            manager.OnCardSuccess();
            
            // Assert - No longer has effects
            Assert.IsFalse(manager.HasTemporaryEffects());
            Assert.AreEqual("", manager.GetTemporaryEffectsDescription());
        }
    }
}