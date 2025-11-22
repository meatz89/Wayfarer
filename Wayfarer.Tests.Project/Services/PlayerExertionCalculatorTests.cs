using Xunit;

/// <summary>
/// Comprehensive tests for PlayerExertionCalculator service.
/// Tests Physical/Mental exertion calculations and Risk level determination.
/// Critical for game balance: wrong thresholds create death spirals or trivialize resource management.
///
/// Test Categories:
/// - Physical Exertion: Edge cases and threshold boundaries (13 tests)
/// - Mental Exertion: Edge cases and threshold boundaries (13 tests)
/// - Risk Level: Edge cases and threshold boundaries (14 tests)
/// - Integration: Complete state calculations (3 tests)
/// Total: 43 tests
/// </summary>
public class PlayerExertionCalculatorTests
{
    // ========== TEST INFRASTRUCTURE ==========

    private Player CreatePlayer(int health, int maxHealth, int stamina, int maxStamina)
    {
        Player player = new Player
        {
            Health = health,
            MaxHealth = maxHealth,
            Stamina = stamina,
            MaxStamina = maxStamina
        };
        return player;
    }

    private PlayerExertionCalculator CreateCalculator()
    {
        return new PlayerExertionCalculator();
    }

    // ========== CATEGORY 1: PHYSICAL EXERTION - EDGE CASES ==========

    [Fact]
    public void CalculatePhysicalExertion_MaxStaminaZero_ReturnsNormal()
    {
        // Arrange
        Player player = CreatePlayer(health: 100, maxHealth: 100, stamina: 50, maxStamina: 0);
        PlayerExertionCalculator calculator = CreateCalculator();

        // Act
        PlayerExertionState result = calculator.CalculateExertion(player);

        // Assert
        Assert.Equal(PlayerExertionLevel.Normal, result.Physical);
    }

    [Fact]
    public void CalculatePhysicalExertion_StaminaEqualsMax_ReturnsFresh()
    {
        // Arrange: 100/100 = 100% stamina
        Player player = CreatePlayer(health: 100, maxHealth: 100, stamina: 100, maxStamina: 100);
        PlayerExertionCalculator calculator = CreateCalculator();

        // Act
        PlayerExertionState result = calculator.CalculateExertion(player);

        // Assert
        Assert.Equal(PlayerExertionLevel.Fresh, result.Physical);
    }

    [Fact]
    public void CalculatePhysicalExertion_StaminaZero_ReturnsDesperate()
    {
        // Arrange: 0/100 = 0% stamina
        Player player = CreatePlayer(health: 100, maxHealth: 100, stamina: 0, maxStamina: 100);
        PlayerExertionCalculator calculator = CreateCalculator();

        // Act
        PlayerExertionState result = calculator.CalculateExertion(player);

        // Assert
        Assert.Equal(PlayerExertionLevel.Desperate, result.Physical);
    }

    // ========== CATEGORY 2: PHYSICAL EXERTION - THRESHOLD BOUNDARIES ==========

    [Fact]
    public void CalculatePhysicalExertion_Exactly81Percent_ReturnsFresh()
    {
        // Arrange: 81/100 = 81% (>80%)
        Player player = CreatePlayer(health: 100, maxHealth: 100, stamina: 81, maxStamina: 100);
        PlayerExertionCalculator calculator = CreateCalculator();

        // Act
        PlayerExertionState result = calculator.CalculateExertion(player);

        // Assert
        Assert.Equal(PlayerExertionLevel.Fresh, result.Physical);
    }

    [Fact]
    public void CalculatePhysicalExertion_Exactly80Percent_ReturnsNormal()
    {
        // Arrange: 80/100 = 80% (NOT >80%, so Normal)
        Player player = CreatePlayer(health: 100, maxHealth: 100, stamina: 80, maxStamina: 100);
        PlayerExertionCalculator calculator = CreateCalculator();

        // Act
        PlayerExertionState result = calculator.CalculateExertion(player);

        // Assert
        Assert.Equal(PlayerExertionLevel.Normal, result.Physical);
    }

    [Fact]
    public void CalculatePhysicalExertion_Exactly51Percent_ReturnsNormal()
    {
        // Arrange: 51/100 = 51% (>50%)
        Player player = CreatePlayer(health: 100, maxHealth: 100, stamina: 51, maxStamina: 100);
        PlayerExertionCalculator calculator = CreateCalculator();

        // Act
        PlayerExertionState result = calculator.CalculateExertion(player);

        // Assert
        Assert.Equal(PlayerExertionLevel.Normal, result.Physical);
    }

    [Fact]
    public void CalculatePhysicalExertion_Exactly50Percent_ReturnsFatigued()
    {
        // Arrange: 50/100 = 50% (NOT >50%, so Fatigued)
        Player player = CreatePlayer(health: 100, maxHealth: 100, stamina: 50, maxStamina: 100);
        PlayerExertionCalculator calculator = CreateCalculator();

        // Act
        PlayerExertionState result = calculator.CalculateExertion(player);

        // Assert
        Assert.Equal(PlayerExertionLevel.Fatigued, result.Physical);
    }

    [Fact]
    public void CalculatePhysicalExertion_Exactly31Percent_ReturnsFatigued()
    {
        // Arrange: 31/100 = 31% (>30%)
        Player player = CreatePlayer(health: 100, maxHealth: 100, stamina: 31, maxStamina: 100);
        PlayerExertionCalculator calculator = CreateCalculator();

        // Act
        PlayerExertionState result = calculator.CalculateExertion(player);

        // Assert
        Assert.Equal(PlayerExertionLevel.Fatigued, result.Physical);
    }

    [Fact]
    public void CalculatePhysicalExertion_Exactly30Percent_ReturnsExhausted()
    {
        // Arrange: 30/100 = 30% (NOT >30%, so Exhausted)
        Player player = CreatePlayer(health: 100, maxHealth: 100, stamina: 30, maxStamina: 100);
        PlayerExertionCalculator calculator = CreateCalculator();

        // Act
        PlayerExertionState result = calculator.CalculateExertion(player);

        // Assert
        Assert.Equal(PlayerExertionLevel.Exhausted, result.Physical);
    }

    [Fact]
    public void CalculatePhysicalExertion_Exactly11Percent_ReturnsExhausted()
    {
        // Arrange: 11/100 = 11% (>10%)
        Player player = CreatePlayer(health: 100, maxHealth: 100, stamina: 11, maxStamina: 100);
        PlayerExertionCalculator calculator = CreateCalculator();

        // Act
        PlayerExertionState result = calculator.CalculateExertion(player);

        // Assert
        Assert.Equal(PlayerExertionLevel.Exhausted, result.Physical);
    }

    [Fact]
    public void CalculatePhysicalExertion_Exactly10Percent_ReturnsDesperate()
    {
        // Arrange: 10/100 = 10% (NOT >10%, so Desperate)
        Player player = CreatePlayer(health: 100, maxHealth: 100, stamina: 10, maxStamina: 100);
        PlayerExertionCalculator calculator = CreateCalculator();

        // Act
        PlayerExertionState result = calculator.CalculateExertion(player);

        // Assert
        Assert.Equal(PlayerExertionLevel.Desperate, result.Physical);
    }

    [Fact]
    public void CalculatePhysicalExertion_65Percent_ReturnsNormal()
    {
        // Arrange: 65/100 = 65% (between 50% and 80%)
        Player player = CreatePlayer(health: 100, maxHealth: 100, stamina: 65, maxStamina: 100);
        PlayerExertionCalculator calculator = CreateCalculator();

        // Act
        PlayerExertionState result = calculator.CalculateExertion(player);

        // Assert
        Assert.Equal(PlayerExertionLevel.Normal, result.Physical);
    }

    [Fact]
    public void CalculatePhysicalExertion_40Percent_ReturnsFatigued()
    {
        // Arrange: 40/100 = 40% (between 30% and 50%)
        Player player = CreatePlayer(health: 100, maxHealth: 100, stamina: 40, maxStamina: 100);
        PlayerExertionCalculator calculator = CreateCalculator();

        // Act
        PlayerExertionState result = calculator.CalculateExertion(player);

        // Assert
        Assert.Equal(PlayerExertionLevel.Fatigued, result.Physical);
    }

    // ========== CATEGORY 3: MENTAL EXERTION - EDGE CASES ==========

    [Fact]
    public void CalculateMentalExertion_MaxHealthZero_ReturnsNormal()
    {
        // Arrange
        Player player = CreatePlayer(health: 50, maxHealth: 0, stamina: 100, maxStamina: 100);
        PlayerExertionCalculator calculator = CreateCalculator();

        // Act
        PlayerExertionState result = calculator.CalculateExertion(player);

        // Assert
        Assert.Equal(PlayerExertionLevel.Normal, result.Mental);
    }

    [Fact]
    public void CalculateMentalExertion_HealthEqualsMax_ReturnsFresh()
    {
        // Arrange: 100/100 = 100% health
        Player player = CreatePlayer(health: 100, maxHealth: 100, stamina: 100, maxStamina: 100);
        PlayerExertionCalculator calculator = CreateCalculator();

        // Act
        PlayerExertionState result = calculator.CalculateExertion(player);

        // Assert
        Assert.Equal(PlayerExertionLevel.Fresh, result.Mental);
    }

    [Fact]
    public void CalculateMentalExertion_HealthZero_ReturnsDesperate()
    {
        // Arrange: 0/100 = 0% health
        Player player = CreatePlayer(health: 0, maxHealth: 100, stamina: 100, maxStamina: 100);
        PlayerExertionCalculator calculator = CreateCalculator();

        // Act
        PlayerExertionState result = calculator.CalculateExertion(player);

        // Assert
        Assert.Equal(PlayerExertionLevel.Desperate, result.Mental);
    }

    // ========== CATEGORY 4: MENTAL EXERTION - THRESHOLD BOUNDARIES ==========

    [Fact]
    public void CalculateMentalExertion_Exactly81Percent_ReturnsFresh()
    {
        // Arrange: 81/100 = 81% (>80%)
        Player player = CreatePlayer(health: 81, maxHealth: 100, stamina: 100, maxStamina: 100);
        PlayerExertionCalculator calculator = CreateCalculator();

        // Act
        PlayerExertionState result = calculator.CalculateExertion(player);

        // Assert
        Assert.Equal(PlayerExertionLevel.Fresh, result.Mental);
    }

    [Fact]
    public void CalculateMentalExertion_Exactly80Percent_ReturnsNormal()
    {
        // Arrange: 80/100 = 80% (NOT >80%, so Normal)
        Player player = CreatePlayer(health: 80, maxHealth: 100, stamina: 100, maxStamina: 100);
        PlayerExertionCalculator calculator = CreateCalculator();

        // Act
        PlayerExertionState result = calculator.CalculateExertion(player);

        // Assert
        Assert.Equal(PlayerExertionLevel.Normal, result.Mental);
    }

    [Fact]
    public void CalculateMentalExertion_Exactly51Percent_ReturnsNormal()
    {
        // Arrange: 51/100 = 51% (>50%)
        Player player = CreatePlayer(health: 51, maxHealth: 100, stamina: 100, maxStamina: 100);
        PlayerExertionCalculator calculator = CreateCalculator();

        // Act
        PlayerExertionState result = calculator.CalculateExertion(player);

        // Assert
        Assert.Equal(PlayerExertionLevel.Normal, result.Mental);
    }

    [Fact]
    public void CalculateMentalExertion_Exactly50Percent_ReturnsFatigued()
    {
        // Arrange: 50/100 = 50% (NOT >50%, so Fatigued)
        Player player = CreatePlayer(health: 50, maxHealth: 100, stamina: 100, maxStamina: 100);
        PlayerExertionCalculator calculator = CreateCalculator();

        // Act
        PlayerExertionState result = calculator.CalculateExertion(player);

        // Assert
        Assert.Equal(PlayerExertionLevel.Fatigued, result.Mental);
    }

    [Fact]
    public void CalculateMentalExertion_Exactly31Percent_ReturnsFatigued()
    {
        // Arrange: 31/100 = 31% (>30%)
        Player player = CreatePlayer(health: 31, maxHealth: 100, stamina: 100, maxStamina: 100);
        PlayerExertionCalculator calculator = CreateCalculator();

        // Act
        PlayerExertionState result = calculator.CalculateExertion(player);

        // Assert
        Assert.Equal(PlayerExertionLevel.Fatigued, result.Mental);
    }

    [Fact]
    public void CalculateMentalExertion_Exactly30Percent_ReturnsExhausted()
    {
        // Arrange: 30/100 = 30% (NOT >30%, so Exhausted)
        Player player = CreatePlayer(health: 30, maxHealth: 100, stamina: 100, maxStamina: 100);
        PlayerExertionCalculator calculator = CreateCalculator();

        // Act
        PlayerExertionState result = calculator.CalculateExertion(player);

        // Assert
        Assert.Equal(PlayerExertionLevel.Exhausted, result.Mental);
    }

    [Fact]
    public void CalculateMentalExertion_Exactly11Percent_ReturnsExhausted()
    {
        // Arrange: 11/100 = 11% (>10%)
        Player player = CreatePlayer(health: 11, maxHealth: 100, stamina: 100, maxStamina: 100);
        PlayerExertionCalculator calculator = CreateCalculator();

        // Act
        PlayerExertionState result = calculator.CalculateExertion(player);

        // Assert
        Assert.Equal(PlayerExertionLevel.Exhausted, result.Mental);
    }

    [Fact]
    public void CalculateMentalExertion_Exactly10Percent_ReturnsDesperate()
    {
        // Arrange: 10/100 = 10% (NOT >10%, so Desperate)
        Player player = CreatePlayer(health: 10, maxHealth: 100, stamina: 100, maxStamina: 100);
        PlayerExertionCalculator calculator = CreateCalculator();

        // Act
        PlayerExertionState result = calculator.CalculateExertion(player);

        // Assert
        Assert.Equal(PlayerExertionLevel.Desperate, result.Mental);
    }

    [Fact]
    public void CalculateMentalExertion_65Percent_ReturnsNormal()
    {
        // Arrange: 65/100 = 65% (between 50% and 80%)
        Player player = CreatePlayer(health: 65, maxHealth: 100, stamina: 100, maxStamina: 100);
        PlayerExertionCalculator calculator = CreateCalculator();

        // Act
        PlayerExertionState result = calculator.CalculateExertion(player);

        // Assert
        Assert.Equal(PlayerExertionLevel.Normal, result.Mental);
    }

    [Fact]
    public void CalculateMentalExertion_40Percent_ReturnsFatigued()
    {
        // Arrange: 40/100 = 40% (between 30% and 50%)
        Player player = CreatePlayer(health: 40, maxHealth: 100, stamina: 100, maxStamina: 100);
        PlayerExertionCalculator calculator = CreateCalculator();

        // Act
        PlayerExertionState result = calculator.CalculateExertion(player);

        // Assert
        Assert.Equal(PlayerExertionLevel.Fatigued, result.Mental);
    }

    // ========== CATEGORY 5: RISK LEVEL - EDGE CASES ==========

    [Fact]
    public void CalculateRiskLevel_BothMaxZero_ReturnsModerate()
    {
        // Arrange
        Player player = CreatePlayer(health: 50, maxHealth: 0, stamina: 50, maxStamina: 0);
        PlayerExertionCalculator calculator = CreateCalculator();

        // Act
        PlayerExertionState result = calculator.CalculateExertion(player);

        // Assert
        Assert.Equal(EnvironmentalRiskLevel.Moderate, result.CurrentRisk);
    }

    [Fact]
    public void CalculateRiskLevel_OnlyMaxStaminaZero_ReturnsModerate()
    {
        // Arrange
        Player player = CreatePlayer(health: 100, maxHealth: 100, stamina: 50, maxStamina: 0);
        PlayerExertionCalculator calculator = CreateCalculator();

        // Act
        PlayerExertionState result = calculator.CalculateExertion(player);

        // Assert
        Assert.Equal(EnvironmentalRiskLevel.Moderate, result.CurrentRisk);
    }

    [Fact]
    public void CalculateRiskLevel_OnlyMaxHealthZero_ReturnsModerate()
    {
        // Arrange
        Player player = CreatePlayer(health: 50, maxHealth: 0, stamina: 100, maxStamina: 100);
        PlayerExertionCalculator calculator = CreateCalculator();

        // Act
        PlayerExertionState result = calculator.CalculateExertion(player);

        // Assert
        Assert.Equal(EnvironmentalRiskLevel.Moderate, result.CurrentRisk);
    }

    // ========== CATEGORY 6: RISK LEVEL - THRESHOLD BOUNDARIES ==========

    [Fact]
    public void CalculateRiskLevel_Both100Percent_ReturnsMinimal()
    {
        // Arrange: (100/100 + 100/100) / 2 = 100% average
        Player player = CreatePlayer(health: 100, maxHealth: 100, stamina: 100, maxStamina: 100);
        PlayerExertionCalculator calculator = CreateCalculator();

        // Act
        PlayerExertionState result = calculator.CalculateExertion(player);

        // Assert
        Assert.Equal(EnvironmentalRiskLevel.Minimal, result.CurrentRisk);
    }

    [Fact]
    public void CalculateRiskLevel_Both81Percent_ReturnsMinimal()
    {
        // Arrange: (81/100 + 81/100) / 2 = 81% average (>80%)
        Player player = CreatePlayer(health: 81, maxHealth: 100, stamina: 81, maxStamina: 100);
        PlayerExertionCalculator calculator = CreateCalculator();

        // Act
        PlayerExertionState result = calculator.CalculateExertion(player);

        // Assert
        Assert.Equal(EnvironmentalRiskLevel.Minimal, result.CurrentRisk);
    }

    [Fact]
    public void CalculateRiskLevel_Both80Percent_ReturnsLow()
    {
        // Arrange: (80/100 + 80/100) / 2 = 80% average (NOT >80%, so Low)
        Player player = CreatePlayer(health: 80, maxHealth: 100, stamina: 80, maxStamina: 100);
        PlayerExertionCalculator calculator = CreateCalculator();

        // Act
        PlayerExertionState result = calculator.CalculateExertion(player);

        // Assert
        Assert.Equal(EnvironmentalRiskLevel.Low, result.CurrentRisk);
    }

    [Fact]
    public void CalculateRiskLevel_Both61Percent_ReturnsLow()
    {
        // Arrange: (61/100 + 61/100) / 2 = 61% average (>60%)
        Player player = CreatePlayer(health: 61, maxHealth: 100, stamina: 61, maxStamina: 100);
        PlayerExertionCalculator calculator = CreateCalculator();

        // Act
        PlayerExertionState result = calculator.CalculateExertion(player);

        // Assert
        Assert.Equal(EnvironmentalRiskLevel.Low, result.CurrentRisk);
    }

    [Fact]
    public void CalculateRiskLevel_Both60Percent_ReturnsModerate()
    {
        // Arrange: (60/100 + 60/100) / 2 = 60% average (NOT >60%, so Moderate)
        Player player = CreatePlayer(health: 60, maxHealth: 100, stamina: 60, maxStamina: 100);
        PlayerExertionCalculator calculator = CreateCalculator();

        // Act
        PlayerExertionState result = calculator.CalculateExertion(player);

        // Assert
        Assert.Equal(EnvironmentalRiskLevel.Moderate, result.CurrentRisk);
    }

    [Fact]
    public void CalculateRiskLevel_Both41Percent_ReturnsModerate()
    {
        // Arrange: (41/100 + 41/100) / 2 = 41% average (>40%)
        Player player = CreatePlayer(health: 41, maxHealth: 100, stamina: 41, maxStamina: 100);
        PlayerExertionCalculator calculator = CreateCalculator();

        // Act
        PlayerExertionState result = calculator.CalculateExertion(player);

        // Assert
        Assert.Equal(EnvironmentalRiskLevel.Moderate, result.CurrentRisk);
    }

    [Fact]
    public void CalculateRiskLevel_Both40Percent_ReturnsHigh()
    {
        // Arrange: (40/100 + 40/100) / 2 = 40% average (NOT >40%, so High)
        Player player = CreatePlayer(health: 40, maxHealth: 100, stamina: 40, maxStamina: 100);
        PlayerExertionCalculator calculator = CreateCalculator();

        // Act
        PlayerExertionState result = calculator.CalculateExertion(player);

        // Assert
        Assert.Equal(EnvironmentalRiskLevel.High, result.CurrentRisk);
    }

    [Fact]
    public void CalculateRiskLevel_Both21Percent_ReturnsHigh()
    {
        // Arrange: (21/100 + 21/100) / 2 = 21% average (>20%)
        Player player = CreatePlayer(health: 21, maxHealth: 100, stamina: 21, maxStamina: 100);
        PlayerExertionCalculator calculator = CreateCalculator();

        // Act
        PlayerExertionState result = calculator.CalculateExertion(player);

        // Assert
        Assert.Equal(EnvironmentalRiskLevel.High, result.CurrentRisk);
    }

    [Fact]
    public void CalculateRiskLevel_Both20Percent_ReturnsExtreme()
    {
        // Arrange: (20/100 + 20/100) / 2 = 20% average (NOT >20%, so Extreme)
        Player player = CreatePlayer(health: 20, maxHealth: 100, stamina: 20, maxStamina: 100);
        PlayerExertionCalculator calculator = CreateCalculator();

        // Act
        PlayerExertionState result = calculator.CalculateExertion(player);

        // Assert
        Assert.Equal(EnvironmentalRiskLevel.Extreme, result.CurrentRisk);
    }

    [Fact]
    public void CalculateRiskLevel_BothZeroPercent_ReturnsExtreme()
    {
        // Arrange: (0/100 + 0/100) / 2 = 0% average
        Player player = CreatePlayer(health: 0, maxHealth: 100, stamina: 0, maxStamina: 100);
        PlayerExertionCalculator calculator = CreateCalculator();

        // Act
        PlayerExertionState result = calculator.CalculateExertion(player);

        // Assert
        Assert.Equal(EnvironmentalRiskLevel.Extreme, result.CurrentRisk);
    }

    [Fact]
    public void CalculateRiskLevel_MixedResources_CalculatesCorrectAverage()
    {
        // Arrange: (100/100 + 60/100) / 2 = 80% average (NOT >80%, so Low)
        Player player = CreatePlayer(health: 100, maxHealth: 100, stamina: 60, maxStamina: 100);
        PlayerExertionCalculator calculator = CreateCalculator();

        // Act
        PlayerExertionState result = calculator.CalculateExertion(player);

        // Assert
        Assert.Equal(EnvironmentalRiskLevel.Low, result.CurrentRisk);
    }

    // ========== CATEGORY 7: INTEGRATION TESTS ==========

    [Fact]
    public void CalculateExertion_FreshState_ReturnsAllFreshMinimal()
    {
        // Arrange: Both resources >80% = Fresh physical + Fresh mental + Minimal risk
        Player player = CreatePlayer(health: 90, maxHealth: 100, stamina: 85, maxStamina: 100);
        PlayerExertionCalculator calculator = CreateCalculator();

        // Act
        PlayerExertionState result = calculator.CalculateExertion(player);

        // Assert
        Assert.Equal(PlayerExertionLevel.Fresh, result.Physical);
        Assert.Equal(PlayerExertionLevel.Fresh, result.Mental);
        Assert.Equal(EnvironmentalRiskLevel.Minimal, result.CurrentRisk);
    }

    [Fact]
    public void CalculateExertion_DesperateState_ReturnsAllDesperateExtreme()
    {
        // Arrange: Both resources <10% = Desperate physical + Desperate mental + Extreme risk
        Player player = CreatePlayer(health: 5, maxHealth: 100, stamina: 8, maxStamina: 100);
        PlayerExertionCalculator calculator = CreateCalculator();

        // Act
        PlayerExertionState result = calculator.CalculateExertion(player);

        // Assert
        Assert.Equal(PlayerExertionLevel.Desperate, result.Physical);
        Assert.Equal(PlayerExertionLevel.Desperate, result.Mental);
        Assert.Equal(EnvironmentalRiskLevel.Extreme, result.CurrentRisk);
    }

    [Fact]
    public void CalculateExertion_MixedExertion_ReturnsAppropriateRisk()
    {
        // Arrange: Physical Fresh (90%), Mental Desperate (5%), average 47.5% = Moderate risk
        Player player = CreatePlayer(health: 5, maxHealth: 100, stamina: 90, maxStamina: 100);
        PlayerExertionCalculator calculator = CreateCalculator();

        // Act
        PlayerExertionState result = calculator.CalculateExertion(player);

        // Assert
        Assert.Equal(PlayerExertionLevel.Fresh, result.Physical);
        Assert.Equal(PlayerExertionLevel.Desperate, result.Mental);
        Assert.Equal(EnvironmentalRiskLevel.Moderate, result.CurrentRisk);
    }
}
