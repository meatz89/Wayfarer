using System;
using System.Collections.Generic;
using Xunit;

public class CardPropertyTest
{
    [Fact]
    public void TestCardPropertyHelpers()
    {
        // Test default card (no properties)
        var defaultCard = new ConversationCard();
        Assert.True(defaultCard.IsPersistent);
        Assert.False(defaultCard.IsFleeting);
        Assert.False(defaultCard.IsOpportunity);
        Assert.False(defaultCard.IsGoal);
        
        // Test fleeting card
        var fleetingCard = new ConversationCard();
        fleetingCard.Properties.Add(CardProperty.Fleeting);
        Assert.True(fleetingCard.IsFleeting);
        Assert.False(fleetingCard.IsPersistent);
        Assert.False(fleetingCard.IsOpportunity);
        Assert.False(fleetingCard.IsGoal);
        
        // Test opportunity card
        var opportunityCard = new ConversationCard();
        opportunityCard.Properties.Add(CardProperty.Opportunity);
        Assert.False(opportunityCard.IsFleeting);
        Assert.False(opportunityCard.IsPersistent);
        Assert.True(opportunityCard.IsOpportunity);
        Assert.False(opportunityCard.IsGoal);
        
        // Test goal card (both fleeting AND opportunity)
        var goalCard = new ConversationCard();
        goalCard.Properties.Add(CardProperty.Fleeting);
        goalCard.Properties.Add(CardProperty.Opportunity);
        Assert.True(goalCard.IsFleeting);
        Assert.False(goalCard.IsPersistent);
        Assert.True(goalCard.IsOpportunity);
        Assert.True(goalCard.IsGoal);
        
        // Test skeleton card
        var skeletonCard = new ConversationCard();
        skeletonCard.Properties.Add(CardProperty.Skeleton);
        skeletonCard.Properties.Add(CardProperty.Persistent);
        Assert.True(skeletonCard.IsSkeleton);
        Assert.True(skeletonCard.IsPersistent);
        
        // Test burden card
        var burdenCard = new ConversationCard();
        burdenCard.Properties.Add(CardProperty.Burden);
        burdenCard.Properties.Add(CardProperty.Persistent);
        Assert.True(burdenCard.IsBurden);
        Assert.True(burdenCard.IsPersistent);
        
        // Test observation card
        var observationCard = new ConversationCard();
        observationCard.Properties.Add(CardProperty.Observable);
        observationCard.Properties.Add(CardProperty.Persistent);
        Assert.True(observationCard.IsObservable);
        Assert.True(observationCard.IsPersistent);
    }
    
    [Fact]
    public void TestDeepClone()
    {
        var originalCard = new ConversationCard
        {
            Id = "test_card",
            Name = "Test Card",
            Properties = new List<CardProperty> 
            { 
                CardProperty.Fleeting, 
                CardProperty.Opportunity 
            }
        };
        
        var clonedCard = originalCard.DeepClone();
        
        // Verify clone has same properties
        Assert.Equal(originalCard.Id, clonedCard.Id);
        Assert.Equal(originalCard.Name, clonedCard.Name);
        Assert.Equal(originalCard.Properties.Count, clonedCard.Properties.Count);
        Assert.True(clonedCard.IsGoal);
        
        // Verify it's a deep clone (modifying original doesn't affect clone)
        originalCard.Properties.Add(CardProperty.Burden);
        Assert.NotEqual(originalCard.Properties.Count, clonedCard.Properties.Count);
        Assert.False(clonedCard.IsBurden);
    }
    
    [Fact]
    public void TestGoalCardClass()
    {
        var goalCard = new GoalCard();
        
        // Verify goal cards have both properties
        Assert.Contains(CardProperty.Fleeting, goalCard.Properties);
        Assert.Contains(CardProperty.Opportunity, goalCard.Properties);
        Assert.True(goalCard.IsFleeting);
        Assert.True(goalCard.IsOpportunity);
    }
}