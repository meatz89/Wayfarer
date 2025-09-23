#!/usr/bin/env node

/**
 * Wayfarer Card System Quality Assessment Runner
 * Executes comprehensive player experience evaluation tests
 */

const { spawn } = require('child_process');
const fs = require('fs');
const path = require('path');

console.log('🎮 Wayfarer Card System Quality Assessment');
console.log('=========================================');
console.log('Evaluating player experience quality metrics...\n');

// Ensure test-results directory exists
const testResultsDir = path.join(__dirname, 'test-results');
if (!fs.existsSync(testResultsDir)) {
  fs.mkdirSync(testResultsDir, { recursive: true });
}

const screenshotsDir = path.join(testResultsDir, 'screenshots');
if (!fs.existsSync(screenshotsDir)) {
  fs.mkdirSync(screenshotsDir, { recursive: true });
}

// Test execution order for comprehensive evaluation
const testSuites = [
  {
    name: 'Strategic Depth Assessment',
    file: 'tests/strategic-depth-assessment.spec.js',
    description: 'Evaluates decision complexity, planning depth, and strategic variety'
  },
  {
    name: 'Learning Curve Validation',
    file: 'tests/learning-curve-validation.spec.js',
    description: 'Tests card clarity, scaling transparency, and progressive complexity'
  },
  {
    name: 'Engagement Testing',
    file: 'tests/engagement-testing.spec.js',
    description: 'Validates meaningful choices, comeback mechanics, and tension management'
  },
  {
    name: 'UI/UX Quality Assessment',
    file: 'tests/ui-ux-quality-assessment.spec.js',
    description: 'Checks information clarity, visual feedback, and performance'
  },
  {
    name: 'Balance Validation',
    file: 'tests/balance-validation.spec.js',
    description: 'Ensures power consistency, resource economy, and game length balance'
  },
  {
    name: 'Comprehensive Quality Report',
    file: 'tests/comprehensive-quality-report.spec.js',
    description: 'Aggregates all results into final transformation assessment'
  }
];

async function runTest(testSuite) {
  return new Promise((resolve, reject) => {
    console.log(`\n🧪 Running: ${testSuite.name}`);
    console.log(`📝 ${testSuite.description}`);
    console.log('─'.repeat(60));

    const npx = process.platform === 'win32' ? 'npx.cmd' : 'npx';
    const child = spawn(npx, ['playwright', 'test', testSuite.file, '--reporter=list'], {
      stdio: 'inherit',
      cwd: __dirname
    });

    child.on('close', (code) => {
      if (code === 0) {
        console.log(`✅ ${testSuite.name} completed successfully`);
        resolve();
      } else {
        console.log(`⚠️  ${testSuite.name} completed with issues (exit code: ${code})`);
        // Continue execution even if tests have assertions failures
        // The goal is to collect all data for comprehensive analysis
        resolve();
      }
    });

    child.on('error', (error) => {
      console.error(`❌ Failed to run ${testSuite.name}:`, error);
      reject(error);
    });
  });
}

async function runAllTests() {
  console.log('🚀 Starting comprehensive quality assessment...\n');

  try {
    for (const testSuite of testSuites) {
      await runTest(testSuite);
    }

    console.log('\n' + '='.repeat(60));
    console.log('🏁 Quality Assessment Complete!');
    console.log('='.repeat(60));
    console.log('📊 Check test-results/html-report/index.html for detailed results');
    console.log('📸 Screenshots saved to test-results/screenshots/');
    console.log('📋 Comprehensive report available in test output above');

  } catch (error) {
    console.error('\n❌ Quality assessment failed:', error);
    process.exit(1);
  }
}

// Handle command line arguments
const args = process.argv.slice(2);

if (args.includes('--help') || args.includes('-h')) {
  console.log(`
Usage: node run-quality-assessment.js [options]

Options:
  --help, -h     Show this help message
  --single TEST  Run only a specific test suite (by name)

Available test suites:
${testSuites.map((suite, i) => `  ${i + 1}. ${suite.name}`).join('\n')}

Examples:
  node run-quality-assessment.js
  node run-quality-assessment.js --single "Strategic Depth Assessment"
`);
  process.exit(0);
}

if (args.includes('--single')) {
  const testName = args[args.indexOf('--single') + 1];
  const testSuite = testSuites.find(suite => suite.name === testName);

  if (!testSuite) {
    console.error(`❌ Test suite "${testName}" not found.`);
    console.log('Available test suites:');
    testSuites.forEach((suite, i) => console.log(`  ${i + 1}. ${suite.name}`));
    process.exit(1);
  }

  runTest(testSuite).then(() => {
    console.log(`\n✅ Single test "${testName}" completed.`);
  }).catch(error => {
    console.error(`❌ Test failed:`, error);
    process.exit(1);
  });
} else {
  runAllTests();
}