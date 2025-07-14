using System.IO;
using Xunit;

namespace Wayfarer.Tests;

/// <summary>
/// Tests for the new unified ContractStep system
/// </summary>
public class ContractStepSystemTests
{
    [Fact]
    public void ContractParser_ShouldParseStepBasedContract_WithAllStepTypes()
    {
        // Arrange - Create a comprehensive test contract JSON
        string contractJson = """
        {
          "id": "test_step_contract",
          "description": "Test contract with multiple step types",
          "startDay": 1,
          "dueDay": 5,
          "payment": 25,
          "failurePenalty": "Test penalty",
          "isCompleted": false,
          "isFailed": false,
          "unlocksContractIds": [],
          "locksContractIds": [],
          "completionSteps": [
            {
              "type": "TravelStep",
              "id": "travel_step",
              "description": "Travel to destination",
              "isRequired": true,
              "orderHint": 1,
              "isCompleted": false,
              "requiredLocationId": "town_square"
            },
            {
              "type": "TransactionStep",
              "id": "transaction_step",
              "description": "Buy required item",
              "isRequired": true,
              "orderHint": 2,
              "isCompleted": false,
              "itemId": "herbs",
              "locationId": "town_square",
              "transactionType": "Buy",
              "quantity": 2,
              "maxPrice": 10
            },
            {
              "type": "ConversationStep",
              "id": "conversation_step",
              "description": "Speak with NPC",
              "isRequired": false,
              "orderHint": 3,
              "isCompleted": false,
              "requiredNPCId": "elder",
              "requiredLocationId": "millbrook"
            },
            {
              "type": "EquipmentStep",
              "id": "equipment_step",
              "description": "Obtain climbing gear",
              "isRequired": true,
              "orderHint": 0,
              "isCompleted": false,
              "requiredEquipmentCategories": ["Climbing_Equipment", "Weather_Protection"]
            }
          ],
          "requiredDestinations": [],
          "requiredTransactions": [],
          "requiredNPCConversations": [],
          "requiredLocationActions": []
        }
        """;

        // Act - Parse the contract
        Contract contract = ContractParser.ParseContract(contractJson);

        // Assert - Verify basic properties
        Assert.Equal("test_step_contract", contract.Id);
        Assert.Equal("Test contract with multiple step types", contract.Description);
        Assert.Equal(25, contract.Payment);
        Assert.Equal(1, contract.StartDay);
        Assert.Equal(5, contract.DueDay);

        // Assert - Verify completion steps
        Assert.Equal(4, contract.CompletionSteps.Count);

        // Verify TravelStep
        TravelStep travelStep = Assert.IsType<TravelStep>(contract.CompletionSteps[0]);
        Assert.Equal("travel_step", travelStep.Id);
        Assert.Equal("Travel to destination", travelStep.Description);
        Assert.True(travelStep.IsRequired);
        Assert.Equal(1, travelStep.OrderHint);
        Assert.False(travelStep.IsCompleted);
        Assert.Equal("town_square", travelStep.RequiredLocationId);

        // Verify TransactionStep
        TransactionStep transactionStep = Assert.IsType<TransactionStep>(contract.CompletionSteps[1]);
        Assert.Equal("transaction_step", transactionStep.Id);
        Assert.Equal("Buy required item", transactionStep.Description);
        Assert.True(transactionStep.IsRequired);
        Assert.Equal(2, transactionStep.OrderHint);
        Assert.Equal("herbs", transactionStep.ItemId);
        Assert.Equal("town_square", transactionStep.LocationId);
        Assert.Equal(TransactionType.Buy, transactionStep.TransactionType);
        Assert.Equal(2, transactionStep.Quantity);
        Assert.Equal(10, transactionStep.MaxPrice);

        // Verify ConversationStep
        ConversationStep conversationStep = Assert.IsType<ConversationStep>(contract.CompletionSteps[2]);
        Assert.Equal("conversation_step", conversationStep.Id);
        Assert.False(conversationStep.IsRequired); // Optional step
        Assert.Equal("elder", conversationStep.RequiredNPCId);
        Assert.Equal("millbrook", conversationStep.RequiredLocationId);

        // Verify EquipmentStep
        EquipmentStep equipmentStep = Assert.IsType<EquipmentStep>(contract.CompletionSteps[3]);
        Assert.Equal("equipment_step", equipmentStep.Id);
        Assert.True(equipmentStep.IsRequired);
        Assert.Equal(0, equipmentStep.OrderHint); // No order requirement
        Assert.Equal(2, equipmentStep.RequiredEquipmentCategories.Count);
        Assert.Contains(EquipmentCategory.Climbing_Equipment, equipmentStep.RequiredEquipmentCategories);
        Assert.Contains(EquipmentCategory.Weather_Protection, equipmentStep.RequiredEquipmentCategories);
    }


    [Fact]
    public void ContractStep_GetRequirement_ShouldProvideCorrectInformation()
    {
        // Arrange - Create different step types
        TravelStep travelStep = new TravelStep
        {
            Id = "travel_test",
            Description = "Go to location",
            RequiredLocationId = "destination",
            IsCompleted = false
        };

        TransactionStep transactionStep = new TransactionStep
        {
            Id = "transaction_test",
            Description = "Buy item",
            ItemId = "sword",
            LocationId = "shop",
            TransactionType = TransactionType.Buy,
            Quantity = 1,
            MaxPrice = 50,
            IsCompleted = true
        };

        // Act - Get requirements
        ContractStepRequirement travelReq = travelStep.GetRequirement();
        ContractStepRequirement transactionReq = transactionStep.GetRequirement();

        // Assert - Verify travel requirement
        Assert.Equal(ContractStepType.Travel, travelReq.Type);
        Assert.Equal("Go to location", travelReq.Description);
        Assert.Equal("destination", travelReq.TargetLocationId);
        Assert.False(travelReq.IsCompleted);

        // Assert - Verify transaction requirement
        Assert.Equal(ContractStepType.Transaction, transactionReq.Type);
        Assert.Equal("Buy item", transactionReq.Description);
        Assert.Equal("sword", transactionReq.ItemId);
        Assert.Equal("shop", transactionReq.TargetLocationId);
        Assert.Equal(TransactionType.Buy, transactionReq.TransactionType);
        Assert.Equal(1, transactionReq.Quantity);
        Assert.Equal(50, transactionReq.MaxPrice);
        Assert.True(transactionReq.IsCompleted);
    }
}