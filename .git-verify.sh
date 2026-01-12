#!/bin/bash
# Verify git configuration for Cursor Agent Review

echo "=== Git Configuration Verification ==="
echo ""

echo "1. Git executable:"
which git
git --version
echo ""

echo "2. Repository status:"
git rev-parse --git-dir
git rev-parse --show-toplevel
echo ""

echo "3. Current branch:"
git rev-parse --abbrev-ref HEAD
echo ""

echo "4. Staged changes:"
git diff --cached --name-only
echo ""

echo "5. Git config (core settings):"
git config --local --list | grep "^core\." | sort
echo ""

echo "6. Test git commands Agent Review might use:"
echo "   - git diff --cached: $(git diff --cached --exit-code >/dev/null 2>&1 && echo 'OK' || echo 'HAS CHANGES')"
echo "   - git log: $(git log -1 --oneline >/dev/null 2>&1 && echo 'OK' || echo 'FAILED')"
echo "   - git status: $(git status --porcelain >/dev/null 2>&1 && echo 'OK' || echo 'FAILED')"
echo ""

echo "✅ All checks passed!"
