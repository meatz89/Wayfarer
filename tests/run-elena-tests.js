#!/usr/bin/env node

/**
 * Elena Strategic Decision Framework Test Runner
 *
 * This script runs the comprehensive Elena conversation tests and generates
 * a detailed report on the strategic decision-making framework.
 */

const { spawn } = require('child_process');
const path = require('path');
const fs = require('fs');

console.log('🎮 Elena Strategic Decision Framework Test Suite');
console.log('='.repeat(50));

// Ensure test results directory exists
const testResultsDir = path.join(__dirname, '..', 'test-results');
if (!fs.existsSync(testResultsDir)) {
  fs.mkdirSync(testResultsDir, { recursive: true });
}

// Run Playwright tests with specific configuration
const playwrightArgs = [
  'test',
  '--config=playwright.config.js',
  '--project=chromium',
  'tests/elena-strategic-decisions.spec.js',
  '--reporter=list',
  '--reporter=html',
  '--max-failures=5'
];

console.log('🚀 Starting Elena conversation strategic tests...');
console.log('📁 Test results will be saved to:', testResultsDir);

const testProcess = spawn('npx', ['playwright', ...playwrightArgs], {
  stdio: 'inherit',
  cwd: path.join(__dirname, '..')
});

testProcess.on('close', (code) => {
  console.log('\n' + '='.repeat(50));

  if (code === 0) {
    console.log('✅ Elena strategic decision tests completed successfully!');
    console.log('\n📊 Test Results Summary:');
    console.log('- Turn 1 Constraint Testing: Focus limitation scenarios');
    console.log('- Resource Conversion Testing: Momentum spending mechanics');
    console.log('- Scaling Formula Validation: Dynamic card calculations');
    console.log('- Focus Constraint Validation: Strategic tension verification');
    console.log('- Decision Framework Testing: Multi-phase strategic choices');

    console.log('\n📸 Screenshots captured demonstrating:');
    console.log('- Strategic choice points at each turn');
    console.log('- Resource constraint scenarios');
    console.log('- Card interaction mechanics');
    console.log('- Focus management strategies');

    console.log('\n📁 Detailed results available in:');
    console.log('- HTML Report: test-results/index.html');
    console.log('- Screenshots: test-results/*.png');
    console.log('- JSON Results: test-results/results.json');

  } else {
    console.log('❌ Elena strategic decision tests failed!');
    console.log('🔍 Check the HTML report for detailed failure information');
  }

  console.log('\n💡 Strategic Framework Validation Status:');
  console.log('- Strategic tension exists: ' + (code === 0 ? '✅' : '❓'));
  console.log('- Focus constraints matter: ' + (code === 0 ? '✅' : '❓'));
  console.log('- Resource conversion works: ' + (code === 0 ? '✅' : '❓'));
  console.log('- Multiple viable strategies: ' + (code === 0 ? '✅' : '❓'));

  process.exit(code);
});

testProcess.on('error', (error) => {
  console.error('❌ Failed to start test process:', error);
  process.exit(1);
});