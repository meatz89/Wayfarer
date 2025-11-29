#!/bin/bash
# Install DDR-007 pre-commit hook
#
# Usage: ./scripts/hooks/install.sh

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
GIT_HOOKS_DIR="$REPO_ROOT/.git/hooks"

echo "Installing DDR-007 pre-commit hook..."

if [ ! -d "$GIT_HOOKS_DIR" ]; then
    echo "ERROR: .git/hooks directory not found at $GIT_HOOKS_DIR"
    echo "Are you running this from within a git repository?"
    exit 1
fi

# Copy pre-commit hook
cp "$SCRIPT_DIR/pre-commit" "$GIT_HOOKS_DIR/pre-commit"
chmod +x "$GIT_HOOKS_DIR/pre-commit"

echo "Pre-commit hook installed successfully!"
echo ""
echo "The hook will now check for DDR-007 violations before each commit:"
echo "  - Decimal multipliers (* 0.X, * 1.X)"
echo "  - Basis point patterns (BasisPoint, BP)"
echo "  - Percentage calculations (* 100 /, / 100)"
echo ""
echo "To uninstall: rm $GIT_HOOKS_DIR/pre-commit"
