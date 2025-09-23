/**
 * Game Helper Utilities for Wayfarer Card System Testing
 * Provides abstraction for common game interactions
 */

class GameHelpers {
  constructor(page) {
    this.page = page;
  }

  /**
   * Wait for Blazor to fully hydrate and game to load
   */
  async waitForGameReady() {
    await this.page.waitForLoadState('networkidle');

    // Wait for game container to be visible
    await this.page.waitForSelector('.game-container', { timeout: 10000 });

    // Wait for any loading indicators to disappear
    await this.page.waitForSelector('.loading', { state: 'hidden', timeout: 5000 }).catch(() => {
      // Loading indicator might not exist, that's okay
    });
  }

  /**
   * Start a new game session
   */
  async startNewGame() {
    await this.waitForGameReady();

    // Look for start game button or similar
    const startButton = this.page.locator('button:has-text("Start Game"), button:has-text("New Game"), .start-button');
    if (await startButton.count() > 0) {
      await startButton.click();
      await this.waitForGameReady();
    }
  }

  /**
   * Navigate to a conversation with an NPC
   */
  async startConversation(npcName = 'Elena') {
    // Find and click on the NPC
    const npcElement = this.page.locator(`.npc-option:has-text("${npcName}"), .location-npc:has-text("${npcName}")`);

    if (await npcElement.count() > 0) {
      await npcElement.click();
      await this.waitForGameReady();

      // Wait for conversation interface to load
      await this.page.waitForSelector('.conversation-content', { timeout: 10000 });
      return true;
    }

    return false;
  }

  /**
   * Get current game state information
   */
  async getGameState() {
    const state = {
      currentScreen: null,
      resources: {},
      conversationState: null
    };

    // Determine current screen
    if (await this.page.locator('.conversation-content').count() > 0) {
      state.currentScreen = 'conversation';
      state.conversationState = await this.getConversationState();
    } else if (await this.page.locator('.location-content').count() > 0) {
      state.currentScreen = 'location';
    }

    // Get resource information
    state.resources = await this.getResources();

    return state;
  }

  /**
   * Get conversation-specific state
   */
  async getConversationState() {
    const conversationState = {
      momentum: 0,
      doubt: 0,
      focus: { available: 0, capacity: 0 },
      handSize: 0,
      cards: [],
      flowState: null,
      npcName: null
    };

    // Get momentum
    const momentumElement = this.page.locator('.momentum-counter');
    if (await momentumElement.count() > 0) {
      conversationState.momentum = parseInt(await momentumElement.textContent()) || 0;
    }

    // Get doubt
    const doubtSlots = this.page.locator('.doubt-slot.filled');
    conversationState.doubt = await doubtSlots.count();

    // Get focus
    const focusAvailable = this.page.locator('.focus-dot.available');
    const focusTotal = this.page.locator('.focus-dot');
    conversationState.focus.available = await focusAvailable.count();
    conversationState.focus.capacity = await focusTotal.count();

    // Get hand size
    const handCards = this.page.locator('.card-grid .card:not(.disabled)');
    conversationState.handSize = await handCards.count();

    // Get card information
    conversationState.cards = await this.getHandCards();

    // Get NPC name
    const npcNameElement = this.page.locator('.npc-name');
    if (await npcNameElement.count() > 0) {
      conversationState.npcName = await npcNameElement.textContent();
    }

    // Get flow state
    const currentStateElement = this.page.locator('.current-state .state-name');
    if (await currentStateElement.count() > 0) {
      conversationState.flowState = await currentStateElement.textContent();
    }

    return conversationState;
  }

  /**
   * Get information about cards in hand
   */
  async getHandCards() {
    const cards = [];
    const cardElements = this.page.locator('.card-grid .card:not(.disabled)');
    const cardCount = await cardElements.count();

    for (let i = 0; i < cardCount; i++) {
      const card = cardElements.nth(i);

      const cardInfo = {
        name: await card.locator('.card-name').textContent(),
        focus: parseInt(await card.locator('.card-focus').textContent()) || 0,
        category: await card.locator('.category-tag').textContent(),
        effect: await card.locator('.card-effect').textContent(),
        dialogueFragment: await card.locator('.card-text').textContent(),
        isSelectable: !await card.getAttribute('class').then(c => c?.includes('disabled'))
      };

      // Check for special properties
      cardInfo.isImpulse = await card.locator('.persistence-tag.impulse').count() > 0;
      cardInfo.isThought = await card.locator('.persistence-tag.thought').count() > 0;
      cardInfo.hasScaling = await card.locator('.card-scaling').count() > 0;

      cards.push(cardInfo);
    }

    return cards;
  }

  /**
   * Get resource information (attention, coins, etc.)
   */
  async getResources() {
    const resources = {};

    // Check for attention
    const attentionElement = this.page.locator('.attention-value, .resource-attention');
    if (await attentionElement.count() > 0) {
      resources.attention = parseInt(await attentionElement.textContent()) || 0;
    }

    // Check for coins
    const coinsElement = this.page.locator('.coins-value, .resource-coins');
    if (await coinsElement.count() > 0) {
      resources.coins = parseInt(await coinsElement.textContent()) || 0;
    }

    return resources;
  }

  /**
   * Execute LISTEN action
   */
  async executeListen() {
    const listenButton = this.page.locator('.action-button:has-text("LISTEN")');

    if (await listenButton.count() > 0 && !await listenButton.getAttribute('class').then(c => c?.includes('disabled'))) {
      await listenButton.click();

      // Wait for action to complete
      await this.page.waitForSelector('.action-button:has-text("LISTEN"):not(.disabled)', { timeout: 15000 });
      await this.waitForGameReady();
      return true;
    }

    return false;
  }

  /**
   * Execute SPEAK action with selected cards
   */
  async executeSpeak() {
    const speakButton = this.page.locator('.action-button:has-text("SPEAK")');

    if (await speakButton.count() > 0 && !await speakButton.getAttribute('class').then(c => c?.includes('disabled'))) {
      await speakButton.click();

      // Wait for action to complete
      await this.page.waitForSelector('.action-button:has-text("SPEAK"):not(.disabled)', { timeout: 15000 });
      await this.waitForGameReady();
      return true;
    }

    return false;
  }

  /**
   * Select a card by name or index
   */
  async selectCard(cardIdentifier) {
    let cardElement;

    if (typeof cardIdentifier === 'string') {
      // Select by card name
      cardElement = this.page.locator(`.card:has(.card-name:has-text("${cardIdentifier}"))`);
    } else {
      // Select by index
      cardElement = this.page.locator('.card-grid .card').nth(cardIdentifier);
    }

    if (await cardElement.count() > 0 && !await cardElement.getAttribute('class').then(c => c?.includes('disabled'))) {
      await cardElement.click();
      return true;
    }

    return false;
  }

  /**
   * Take a screenshot with descriptive filename
   */
  async takeScreenshot(description) {
    const timestamp = new Date().toISOString().replace(/[:.]/g, '-');
    const filename = `${description}_${timestamp}.png`;
    await this.page.screenshot({
      path: `test-results/screenshots/${filename}`,
      fullPage: true
    });
    return filename;
  }

  /**
   * Wait for narrative generation to complete
   */
  async waitForNarrativeGeneration() {
    // Wait for any "Generating narrative..." indicators to disappear
    await this.page.waitForSelector(':text("Generating narrative...")', { state: 'hidden', timeout: 30000 }).catch(() => {
      // Might not exist, that's okay
    });

    // Wait for processing indicators to disappear
    await this.page.waitForSelector(':text("Processing...")', { state: 'hidden', timeout: 15000 }).catch(() => {
      // Might not exist, that's okay
    });
  }

  /**
   * Check if conversation is exhausted
   */
  async isConversationExhausted() {
    return await this.page.locator('.conversation-exhausted').count() > 0;
  }

  /**
   * Get exhaustion reason if conversation is exhausted
   */
  async getExhaustionReason() {
    const exhaustionElement = this.page.locator('.exhaustion-message');
    if (await exhaustionElement.count() > 0) {
      return await exhaustionElement.textContent();
    }
    return null;
  }
}

module.exports = { GameHelpers };