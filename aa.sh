#!/usr/bin/env bash
set -euo pipefail

REPO="${1:-/home/tor/wkspaces/mo2}"
REMOTE="${REMOTE:-origin}"
MAIN_BRANCH="${MAIN_BRANCH:-main}"
DEV_BRANCH="${DEV_BRANCH:-dev}"
WF_PATH=".github/workflows/auto-improve.yml"

log(){ echo -e "[$(date '+%Y-%m-%d %H:%M:%S')] $*"; }
die(){ log "❌ $*"; exit 1; }

log "📌 Repo: $REPO"
cd "$REPO" || die "Repo introuvable"

git rev-parse --is-inside-work-tree >/dev/null 2>&1 || die "Pas un dépôt git"
git remote get-url "$REMOTE" >/dev/null 2>&1 || die "Remote '$REMOTE' introuvable"

log "⬇️ Fetch + prune"
git fetch --all --prune

# Abort any pending merge/rebase
if [ -d .git/rebase-apply ] || [ -d .git/rebase-merge ]; then
  log "🧹 Abort rebase en cours"
  git rebase --abort || true
fi
if [ -f .git/MERGE_HEAD ]; then
  log "🧹 Abort merge en cours"
  git merge --abort || true
fi

# Checkout main clean
log "🔁 Checkout/reset $MAIN_BRANCH"
git checkout "$MAIN_BRANCH" 2>/dev/null || git checkout -b "$MAIN_BRANCH" "$REMOTE/$MAIN_BRANCH"
git reset --hard "$REMOTE/$MAIN_BRANCH"

# Merge dev -> main with "prefer ours" on conflicts (keeps main side)
if git show-ref --verify --quiet "refs/remotes/$REMOTE/$DEV_BRANCH"; then
  log "🧬 Merge $REMOTE/$DEV_BRANCH → $MAIN_BRANCH (strategy: -X ours)"
  set +e
  git merge -X ours "$REMOTE/$DEV_BRANCH" -m "merge($DEV_BRANCH→$MAIN_BRANCH): prefer main on conflicts"
  MERGE_CODE=$?
  set -e
  if [ $MERGE_CODE -ne 0 ]; then
    log "⚠️ Merge non fast-forward mais géré. Tentative d’auto-résolution"
    # Si quelque chose reste en conflit, on force la préférence main
    git checkout --ours . || true
    git add -A || true
    git commit -m "fix: auto-resolve conflicts preferring main" || true
  fi
else
  log "ℹ️ Pas de branche distante $REMOTE/$DEV_BRANCH (rien à merger)"
fi

# --- Clean “agents” + Copilot traces ---
STAMP="$(date +%Y%m%d_%H%M%S)"
BACKUP_DIR=".patch-backups/$STAMP"
mkdir -p "$BACKUP_DIR"

backup_if_exists() {
  local f="$1"
  if [ -f "$f" ]; then
    mkdir -p "$BACKUP_DIR/$(dirname "$f")"
    cp "$f" "$BACKUP_DIR/$f.bak"
    log "🗂️ Backup: $BACKUP_DIR/$f.bak"
  fi
}

log "🧹 Suppression scripts/steps d’agents & Copilot"

# 1) Purger workflows plausibles d’agents/copilot (hors auto-improve.yml)
for wf in .github/workflows/*.yml .github/workflows/*.yaml; do
  [ -e "$wf" ] || continue
  base="$(basename "$wf")"
  if [[ "$base" != "auto-improve.yml" && "$base" != "auto-improve.yaml" ]]; then
    backup_if_exists "$wf"
    git rm -f "$wf" || true
    log "🗑️ Workflow supprimé: $wf"
  fi
done

# 2) Nettoyer le workflow auto-improve : retirer agent/env AGENT_* / appels IA
if [ -f "$WF_PATH" ]; then
  backup_if_exists "$WF_PATH"
  python3 - "$WF_PATH" <<'PY'
from __future__ import annotations
import re, sys
from pathlib import Path

p = Path(sys.argv[1])
y = p.read_text(encoding="utf-8")

# Minimal hourly workflow, with concurrency guard, no agent steps, no AGENT_* env, no IA scripts
minimal = """name: Auto-Improve

on:
  schedule:
    - cron: '0 * * * *'
  workflow_dispatch:

concurrency:
  group: auto-improve-global
  cancel-in-progress: true

permissions:
  contents: write

jobs:
  improve:
    runs-on: ubuntu-latest
    timeout-minutes: 30
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0

      - name: 🔄 Sync main
        run: |
          git config user.name "CI Bot"
          git config user.email "ci-bot@example"
          git fetch origin main
          git reset --hard origin/main

      - name: ✅ No-op
        run: echo "No agents. Clean, hourly, single-run with cancel-in-progress."
"""

# Si on veut juste forcer la version minimale:
y = minimal

p.write_text(y, encoding="utf-8")
print("OK")
PY
  log "🧩 Workflow auto-improve minimal appliqué"
else
  log "ℹ️ Pas de $WF_PATH, création d’un minimal"
  mkdir -p .github/workflows
  cat > "$WF_PATH" <<'YML'
name: Auto-Improve

on:
  schedule:
    - cron: '0 * * * *'
  workflow_dispatch:

concurrency:
  group: auto-improve-global
  cancel-in-progress: true

permissions:
  contents: write

jobs:
  improve:
    runs-on: ubuntu-latest
    timeout-minutes: 30
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0

      - name: 🔄 Sync main
        run: |
          git config user.name "CI Bot"
          git config user.email "ci-bot@example"
          git fetch origin main
          git reset --hard origin/main

      - name: ✅ No-op
        run: echo "No agents. Clean, hourly, single-run with cancel-in-progress."
YML
fi

# 3) Supprimer scripts IA/agents
rm -f .github/scripts/evoagentx_improve.py \
      .github/scripts/auto-improve-ai.py \
      .github/scripts/auto-improve.py \
      .github/scripts/track_costs.py \
      .github/scripts/generate-uml-diagrams.py \
      .github/scripts/generate_metrics.py 2>/dev/null || true

# 4) Nettoyage fichiers Copilot côté repo
rm -rf .github/copilot .copilot 2>/dev/null || true
rm -f .github/COPILOT.md .github/copilot-instructions.md 2>/dev/null || true

# Si un workflow Copilot aurait été présent sous un autre nom, il a été purgé à l’étape 1.

# --- Commit des nettoyages ---
log "💾 Commit des changements"
git add -A
git commit -m "ci: resolve conflicts preferring main + remove agents + disable copilot traces + minimal hourly workflow" || true

# --- Push main ---
log "⬆️ Push $MAIN_BRANCH → $REMOTE/$MAIN_BRANCH"
git push "$REMOTE" "$MAIN_BRANCH"

# --- Supprimer dev (distant & local) ---
if git show-ref --verify --quiet "refs/remotes/$REMOTE/$DEV_BRANCH"; then
  log "🧹 Suppression branche distante $REMOTE/$DEV_BRANCH"
  git push "$REMOTE" --delete "$DEV_BRANCH" || log "⚠️ Impossible de supprimer $REMOTE/$DEV_BRANCH (droits?)"
fi
if git show-ref --verify --quiet "refs/heads/$DEV_BRANCH"; then
  log "🧹 Suppression branche locale $DEV_BRANCH"
  git branch -D "$DEV_BRANCH" || true
fi

# --- Désactivation Copilot côté repo via gh (best-effort) ---
if command -v gh >/dev/null 2>&1; then
  REPO_FULL="$(gh repo view --json nameWithOwner -q .nameWithOwner 2>/dev/null || echo '')"
  if [ -n "$REPO_FULL" ]; then
    log "🛑 Tentative de retirer l’app GitHub Copilot de ce repo (si installée)…"
    # Récupérer les installations d’apps et retirer Copilot si présent
    # (best-effort; peut nécessiter permissions org)
    gh api -X GET repos/$REPO_FULL/installations --jq '.installations[]?|select(.app_slug=="github-copilot")?.id' 2>/dev/null \
      | while read -r INST_ID; do
          log "🔧 Uninstall Copilot installation_id=$INST_ID"
          gh api -X DELETE /user/installations/$INST_ID/repositories/"$(gh repo view --json id -q .id)" || true
        done
    # Poser une “policy locale off” (fichier info) pour marquer l’intention
    mkdir -p .github/policies
    echo '{ "copilot_allowed": false }' > .github/policies/copilot.json
    git add .github/policies/copilot.json
    git commit -m "policy: copilot off (repo-level marker)" || true
    git push "$REMOTE" "$MAIN_BRANCH" || true
  else
    log "ℹ️ gh OK mais repo non résolu. Ignore."
  fi
else
  log "ℹ️ gh CLI non présent — suppression côté GitHub à faire dans l’UI si nécessaire."
fi

log "✅ Terminé."
git status -sb || true
