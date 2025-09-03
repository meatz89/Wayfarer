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
        Assert.False(defaultCard.IsImpulse);
        Assert.False(defaultCard.IsOpening);
        Assert.False(defaultCard.IsRequest);
        
        // Test impulse card
        var impulseCard = new ConversationCard();
        impulseCard.Properties.Add(CardProperty.Impulse);
        Assert.True(impulseCard.IsImpulse);
        Assert.False(impulseCard.IsPersistent);
        Assert.False(impulseCard.IsOpening);
        Assert.False(impulseCard.IsRequest);
        
        // Test opening card
        var openingCard = new ConversationCard();
        openingCard.Properties.Add(CardProperty.Opening);
        Assert.False(openingCard.IsImpulse);
        Assert.False(openingCard.IsPersistent);
        Assert.True(openingCard.IsOpening);
        Assert.False(openingCard.IsRequest);
        
        // Test request card (both impulse AND opening)
        var requestCard = new ConversationCard();
        requestCard.Properties.Add(CardProperty.Impulse);
        requestCard.Properties.Add(CardProperty.Opening);
        Assert.True(requestCard.IsImpulse);
        Assert.False(requestCard.IsPersistent);
        Assert.True(requestCard.IsOpening);
        Assert.True(requestCard.IsRequest);
        
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
                CardProperty.Impulse, 
                CardProperty.Opening 
            }
        };
        
        var clonedCard = originalCard.DeepClone();
        
        // Verify clone has same properties
        Assert.Equal(originalCard.Id, clonedCard.Id);
        Assert.Equal(originalCard.Name, clonedCard.Name);
        Assert.Equal(originalCard.Properties.Count, clonedCard.Properties.Count);
        Assert.True(clonedCard.IsRequest);
        
        // Verify it's a deep clone (modifying original doesn't affect clone)
        originalCard.Properties.Add(CardProperty.Burden);
        Assert.NotEqual(originalCard.Properties.Count, clonedCard.Properties.Count);
        Assert.False(clonedCard.IsBurden);
    }
    
    [Fact]
    public void TestRequestCardClass()
    {
        var requestCard = new RequestCard();
        
        // Verify request cards have both properties
        Assert.Contains(CardProperty.Impulse, requestCard.Properties);
        Assert.Contains(CardProperty.Opening, requestCard.Properties);
        Assert.True(requestCard.IsImpulse);
        Assert.True(requestCard.IsOpening);
    }
}