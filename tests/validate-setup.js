#!/usr/bin/env node

/**
 * Elena Strategic Test Setup Validator
 *
 * This script validates that the test environment is properly configured
 * for running Elena's strategic decision framework tests.
 */

const fs = require('fs');
const path = require('path');

console.log('🔍 Elena Strategic Test Setup Validator');
console.log('='.repeat(50));

let allChecksPass = true;

function checkFile(filePath, description) {
  const fullPath = path.join(__dirname, filePath);
  const exists = fs.existsSync(fullPath);
  const status = exists ? '✅' : '❌';
  console.log(`${status} ${description}: ${filePath}`);
  if (!exists) allChecksPass = false;
  return exists;
}

function checkDirectory(dirPath, description) {
  const fullPath = path.join(__dirname, dirPath);
  const exists = fs.existsSync(fullPath) && fs.statSync(fullPath).isDirectory();
  const status = exists ? '✅' : '❌';
  console.log(`${status} ${description}: ${dirPath}`);
  if (!exists) allChecksPass = false;
  return exists;
}

console.log('\n📋 Checking Test Files...');
checkFile('elena-strategic-decisions.spec.js', 'Main strategic test suite');
checkFile('elena-strategic-summary.spec.js', 'Strategic framework summary tests');
checkFile('elena-helpers.js', 'Test helper utilities');
checkFile('run-elena-tests.js', 'Test runner script');
checkFile('global-setup.js', 'Global test setup');
checkFile('global-teardown.js', 'Global test teardown');
checkFile('README.md', 'Test documentation');

console.log('\n📁 Checking Directory Structure...');
checkDirectory('../test-results', 'Test results output directory');
checkDirectory('..', 'Project root directory');

console.log('\n🛠️ Checking Configuration Files...');
checkFile('../playwright.config.js', 'Playwright configuration');
checkFile('../package.json', 'Node.js package configuration');

console.log('\n📦 Checking Dependencies...');
const nodeModulesExists = checkDirectory('../node_modules', 'Node.js dependencies');
if (nodeModulesExists) {
  const playwrightExists = checkDirectory('../node_modules/@playwright', 'Playwright framework');
  const playwrightTestExists = checkDirectory('../node_modules/@playwright/test', 'Playwright test runner');
}

console.log('\n🎮 Checking Game Content...');
checkFile('../src/Content/Core/02_cards.json', 'Card definitions');
checkFile('../src/Content/Core/03_npcs.json', 'NPC definitions (Elena)');
checkFile('../CARD_SYSTEM_BALANCE_IMPLEMENTATION.md', 'Strategic framework documentation');

console.log('\n🧪 Validating Test Content...');

// Check that test files contain expected content
const strategicTestContent = fs.existsSync(path.join(__dirname, 'elena-strategic-decisions.spec.js')) ?
  fs.readFileSync(path.join(__dirname, 'elena-strategic-decisions.spec.js'), 'utf8') : '';

const hasStrategicTests = strategicTestContent.includes('Turn 1 Constraint Testing') &&
                         strategicTestContent.includes('Resource Conversion Testing') &&
                         strategicTestContent.includes('burning_conviction') &&
                         strategicTestContent.includes('mental_reset');

console.log(`${hasStrategicTests ? '✅' : '❌'} Strategic test content validation`);
if (!hasStrategicTests) allChecksPass = false;

const helpersContent = fs.existsSync(path.join(__dirname, 'elena-helpers.js')) ?
  fs.readFileSync(path.join(__dirname, 'elena-helpers.js'), 'utf8') : '';

const hasHelperFunctions = helpersContent.includes('navigateToElena') &&
                          helpersContent.includes('startElenaConversation') &&
                          helpersContent.includes('getGameStateValues') &&
                          helpersContent.includes('validateFocusConstraint');

console.log(`${hasHelperFunctions ? '✅' : '❌'} Helper functions validation`);
if (!hasHelperFunctions) allChecksPass = false;

console.log('\n🎯 Test Scenario Coverage...');
const testScenarios = [
  'burning_conviction alone (5 focus required)',
  'mental_reset + burning_conviction combo',
  'passionate_plea + pause_reflect strategy',
  'clear_confusion requires 2 momentum',
  'establish_trust with 3 momentum',
  'moment_of_truth requires 4 momentum'
];

testScenarios.forEach(scenario => {
  const hasScenario = strategicTestContent.includes(scenario.split(' ')[0]);
  console.log(`${hasScenario ? '✅' : '❌'} ${scenario}`);
  if (!hasScenario) allChecksPass = false;
});

console.log('\n' + '='.repeat(50));

if (allChecksPass) {
  console.log('✅ Elena Strategic Test Setup Validation PASSED');
  console.log('\n🚀 Ready to run strategic decision framework tests!');
  console.log('\nNext steps:');
  console.log('1. Start Wayfarer application: cd src && dotnet run');
  console.log('2. Run tests: node tests/run-elena-tests.js');
  console.log('3. Review results in test-results/ directory');
} else {
  console.log('❌ Elena Strategic Test Setup Validation FAILED');
  console.log('\n🔧 Please fix the issues above before running tests');
  console.log('\nCommon fixes:');
  console.log('- npm install (to install dependencies)');
  console.log('- mkdir test-results (to create results directory)');
  console.log('- Check file paths and permissions');
}

console.log('\n💡 Framework Validation Focus:');
console.log('- Strategic tension: Multiple viable but mutually exclusive choices');
console.log('- Focus constraints: Cannot play all desired cards each turn');
console.log('- Resource conversion: Momentum spending creates tradeoffs');
console.log('- Power scaling: Cards reward different game states');
console.log('- Decision quality: Each turn presents meaningful planning');

process.exit(allChecksPass ? 0 : 1);