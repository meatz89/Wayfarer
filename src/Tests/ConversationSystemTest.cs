using Xunit;
using System.Threading.Tasks;
using Wayfarer.ViewModels;

namespace Wayfarer.Tests
{
    public class ConversationSystemTest
    {
        [Fact]
        public async Task StartConversation_Should_Initialize_Conversation()
        {
            // Arrange
            var serviceProvider = ServiceConfiguration.CreateServiceProvider();
            var gameFacade = serviceProvider.GetRequiredService<GameFacade>();
            
            // Act - Start conversation with Elena who should be present at the Copper Kettle
            var conversation = await gameFacade.StartConversationAsync("elena");
            
            // Assert
            Assert.NotNull(conversation);
            Assert.Equal("elena", conversation.NpcId);
            Assert.Equal("Elena", conversation.NpcName);
            Assert.NotNull(conversation.CurrentText);
            Assert.True(conversation.Choices.Count > 0, "Should have conversation choices");
            
            // Verify conversation is now pending
            var currentConversation = gameFacade.GetCurrentConversation();
            Assert.NotNull(currentConversation);
            Assert.Equal("elena", currentConversation.NpcId);
        }
        
        [Fact]
        public async Task LocationScreen_Should_Pass_NpcId_When_Starting_Conversation()
        {
            // This test verifies that NPCPresenceViewModel has Id field populated
            var serviceProvider = ServiceConfiguration.CreateServiceProvider();
            var gameFacade = serviceProvider.GetRequiredService<GameFacade>();
            
            // Get location screen which should show NPCs
            var locationScreen = gameFacade.GetLocationScreen();
            
            Assert.NotNull(locationScreen);
            Assert.NotNull(locationScreen.NPCsPresent);
            Assert.True(locationScreen.NPCsPresent.Count > 0, "Should have NPCs present");
            
            // Verify each NPC has an ID
            foreach (var npc in locationScreen.NPCsPresent)
            {
                Assert.NotNull(npc.Id);
                Assert.NotEmpty(npc.Id);
                Assert.NotNull(npc.Name);
            }
        }
    }
}