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
echo "The hook will now check for architecture violations before each commit:"
echo "  - DDR-007: Decimal multipliers, basis points, float/double types"
echo "  - TYPE: Dictionary, HashSet, var keyword"
echo "  - HIGHLANDER: Entity instance ID properties"
echo "  - FAIL-FAST: Null coalescing (??), TryGetValue/TryParse"
echo "  - SEPARATION: CssClass/IconName in backend services"
echo "  - QUALITY: TODO/FIXME comments, .Wait()/.Result, extension methods"
echo "  - NAMESPACE: Namespace declarations in domain code"
echo "  - DETERMINISM: Random/GetHashCode outside Pile.cs"
echo "  - CATALOGUE: Catalogue calls in Services/Subsystems"
echo "  - DOC-PURITY: Code blocks, JSON structures in arc42/gdd"
echo "  - ICONS: Emojis in .razor files"
echo "  - EXPLICIT: String-based property modification patterns"
echo "  - FILE-SIZE: Files exceeding 1000 lines (requires holistic refactoring)"
echo ""
echo "To uninstall: rm $GIT_HOOKS_DIR/pre-commit"
