const fs = require('fs');

// Read the JSON file
const content = fs.readFileSync('/mnt/c/git/wayfarer/src/Content/Core/core_game_package.json', 'utf8');
const data = JSON.parse(content);

console.log('\n=== CONVERSATION DECK VALIDATION REPORT ===\n');

// 1. Check that all cards have personalityTypes array
console.log('1. CHECKING personalityTypes PROPERTY:');
let missingPersonalityTypes = [];
let hasPersonalityTypes = 0;

data.content.cards.forEach(card => {
  if (!card.personalityTypes || !Array.isArray(card.personalityTypes)) {
    missingPersonalityTypes.push(card.id);
  } else {
    hasPersonalityTypes++;
  }
});

if (missingPersonalityTypes.length === 0) {
  console.log('✅ PASS: All ' + hasPersonalityTypes + ' cards have personalityTypes array');
} else {
  console.log('❌ FAIL: ' + missingPersonalityTypes.length + ' cards missing personalityTypes:');
  missingPersonalityTypes.forEach(id => console.log('  - ' + id));
}

// 2. Check card type distribution
console.log('\n2. CARD TYPE DISTRIBUTION:');
const connectionTypes = {};
data.content.cards.forEach(card => {
  const type = card.connectionType || 'Unknown';
  connectionTypes[type] = (connectionTypes[type] || 0) + 1;
});

Object.entries(connectionTypes).forEach(([type, count]) => {
  console.log('  - ' + type + ': ' + count + ' cards');
});

// Verify we have Trust and Commerce cards
if (connectionTypes['Trust'] && connectionTypes['Commerce']) {
  console.log('✅ PASS: Both Trust and Commerce cards present');
} else {
  console.log('❌ FAIL: Missing required connection types');
}

// 3. Check personality type representation
console.log('\n3. PERSONALITY TYPE REPRESENTATION:');
const personalityTypeUsage = {
  'DEVOTED': 0,
  'MERCANTILE': 0,
  'STEADFAST': 0,
  'PROUD': 0,
  'CUNNING': 0,
  'ALL': 0
};

data.content.cards.forEach(card => {
  if (card.personalityTypes) {
    card.personalityTypes.forEach(pType => {
      if (personalityTypeUsage.hasOwnProperty(pType)) {
        personalityTypeUsage[pType]++;
      }
    });
  }
});

Object.entries(personalityTypeUsage).forEach(([type, count]) => {
  console.log('  - ' + type + ': ' + count + ' cards');
});

// Check specific NPCs
const npcs = data.content.npcs;
const elena = npcs.find(n => n.id === 'elena');
const marcus = npcs.find(n => n.id === 'marcus');
const bertram = npcs.find(n => n.id === 'bertram');

console.log('\n4. NPC PERSONALITY TYPES:');
if (elena && elena.personalityType === 'DEVOTED') {
  console.log('✅ Elena: DEVOTED');
} else {
  console.log('❌ Elena: ' + (elena ? elena.personalityType : 'NOT FOUND'));
}

if (marcus && marcus.personalityType === 'MERCANTILE') {
  console.log('✅ Marcus: MERCANTILE');
} else {
  console.log('❌ Marcus: ' + (marcus ? marcus.personalityType : 'NOT FOUND'));
}

if (bertram && bertram.personalityType === 'STEADFAST') {
  console.log('✅ Bertram: STEADFAST');
} else {
  console.log('❌ Bertram: ' + (bertram ? bertram.personalityType : 'NOT FOUND'));
}

// 5. Check card mechanics
console.log('\n5. CARD MECHANICS VALIDATION:');
let mechanicsIssues = [];

data.content.cards.forEach(card => {
  // Check weight
  if (typeof card.weight !== 'number' || card.weight < 0) {
    mechanicsIssues.push(card.id + ': invalid weight');
  }
  
  // Check difficulty
  const validDifficulties = ['VeryEasy', 'Easy', 'Medium', 'Hard', 'VeryHard'];
  if (!validDifficulties.includes(card.difficulty)) {
    mechanicsIssues.push(card.id + ': invalid difficulty "' + card.difficulty + '"');
  }
  
  // Check effects
  if (!card.successEffect || !card.successEffect.type) {
    mechanicsIssues.push(card.id + ': missing successEffect');
  }
});

if (mechanicsIssues.length === 0) {
  console.log('✅ PASS: All cards have valid mechanics');
} else {
  console.log('❌ FAIL: Found ' + mechanicsIssues.length + ' mechanics issues:');
  mechanicsIssues.slice(0, 10).forEach(issue => console.log('  - ' + issue));
  if (mechanicsIssues.length > 10) {
    console.log('  ... and ' + (mechanicsIssues.length - 10) + ' more');
  }
}

// 6. Check card variety by type
console.log('\n6. CARD VARIETY BY TYPE:');
const cardsByType = {};
data.content.cards.forEach(card => {
  const type = card.type || card.cardType || 'Unknown';
  if (!cardsByType[type]) cardsByType[type] = [];
  cardsByType[type].push(card.id);
});

Object.entries(cardsByType).forEach(([type, cards]) => {
  console.log('  - ' + type + ': ' + cards.length + ' cards');
});

// Final summary
console.log('\n=== VALIDATION SUMMARY ===');
const totalCards = data.content.cards.length;
const hasAllPersonalityTypes = missingPersonalityTypes.length === 0;
const hasRequiredTypes = connectionTypes['Trust'] && connectionTypes['Commerce'];
const hasCorrectNPCs = elena?.personalityType === 'DEVOTED' && 
                       marcus?.personalityType === 'MERCANTILE' && 
                       bertram?.personalityType === 'STEADFAST';
const hasValidMechanics = mechanicsIssues.length === 0;

if (hasAllPersonalityTypes && hasRequiredTypes && hasCorrectNPCs && hasValidMechanics) {
  console.log('✅ ALL CHECKS PASSED');
} else {
  console.log('❌ VALIDATION FAILED - Issues found');
}

console.log('\nTotal cards in deck: ' + totalCards);
console.log('JSON structure: Valid');
