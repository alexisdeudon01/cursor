#!/usr/bin/env bash
set -euo pipefail

REPO="${1:-/home/tor/wkspaces/mo2}"
WF="$REPO/.github/workflows/auto-improve.yml"
REMOTE="${REMOTE:-origin}"
DEV_BRANCH="${DEV_BRANCH:-dev}"
MAIN_BRANCH="${MAIN_BRANCH:-main}"

log(){ echo -e "[$(date '+%Y-%m-%d %H:%M:%S')] $*"; }
die(){ log "❌ $*"; exit 1; }

log "📌 Repo: $REPO"
cd "$REPO" || die "Repo introuvable: $REPO"

# Basic sanity
git rev-parse --is-inside-work-tree >/dev/null 2>&1 || die "Pas un dépôt git"
git remote get-url "$REMOTE" >/dev/null 2>&1 || die "Remote '$REMOTE' introuvable"

log "🔎 Remotes:"
git remote -v

log "⬇️ Fetch all"
git fetch --all --prune

# Ensure main exists remotely
if ! git show-ref --verify --quiet "refs/remotes/$REMOTE/$MAIN_BRANCH"; then
  die "Branche distante $REMOTE/$MAIN_BRANCH introuvable"
fi

# Ensure dev exists (local or remote) before trying merge
DEV_EXISTS_REMOTE=0
DEV_EXISTS_LOCAL=0
git show-ref --verify --quiet "refs/remotes/$REMOTE/$DEV_BRANCH" && DEV_EXISTS_REMOTE=1
git show-ref --verify --quiet "refs/heads/$DEV_BRANCH" && DEV_EXISTS_LOCAL=1

if [ "$DEV_EXISTS_REMOTE" -eq 0 ] && [ "$DEV_EXISTS_LOCAL" -eq 0 ]; then
  log "ℹ️ Branche '$DEV_BRANCH' absente (local+remote). Rien à merger. On continue quand même."
fi

log "✅ Checkout main (reset sur $REMOTE/main pour être clean)"
git checkout "$MAIN_BRANCH" 2>/dev/null || git checkout -b "$MAIN_BRANCH" "$REMOTE/$MAIN_BRANCH"
git reset --hard "$REMOTE/$MAIN_BRANCH"

if [ "$DEV_EXISTS_REMOTE" -eq 1 ]; then
  log "🧬 Merge $REMOTE/$DEV_BRANCH → $MAIN_BRANCH"
  # Try fast-forward
  if git merge --ff-only "$REMOTE/$DEV_BRANCH" >/dev/null 2>&1; then
    log "✅ Fast-forward OK"
  else
    log "⚠️ Pas de fast-forward possible → merge commit"
    git merge "$REMOTE/$DEV_BRANCH" -m "Merge $DEV_BRANCH into $MAIN_BRANCH"
  fi
elif [ "$DEV_EXISTS_LOCAL" -eq 1 ]; then
  log "🧬 Merge $DEV_BRANCH (local) → $MAIN_BRANCH"
  if git merge --ff-only "$DEV_BRANCH" >/dev/null 2>&1; then
    log "✅ Fast-forward OK"
  else
    log "⚠️ Pas de fast-forward possible → merge commit"
    git merge "$DEV_BRANCH" -m "Merge $DEV_BRANCH into $MAIN_BRANCH"
  fi
fi

# --- Add Agent reference into workflow (if workflow exists) ---
if [ -f "$WF" ]; then
  log "🧩 Patch workflow: ajout référence Agent si manquante"

  STAMP="$(date +%Y%m%d_%H%M%S)"
  mkdir -p "$REPO/.patch-backups/$STAMP"
  cp "$WF" "$REPO/.patch-backups/$STAMP/auto-improve.yml.bak"
  log "🗂️ Backup workflow: .patch-backups/$STAMP/auto-improve.yml.bak"

  python3 - "$WF" <<'PY'
from __future__ import annotations
import re, sys
from pathlib import Path

wf = Path(sys.argv[1])
y = wf.read_text(encoding="utf-8")

# Add a global env block if missing, otherwise ensure AGENT_NAME exists
def ensure_agent_env(y: str) -> str:
    if re.search(r'^\s*env:\s*$', y, re.M):
        # env exists somewhere, but might not be at top-level.
        # We'll add AGENT_NAME under the job env if not found.
        if "AGENT_NAME:" in y:
            return y
        # Add under jobs.improve if there is a job
        y2 = re.sub(
            r'(^jobs:\n(?:.*\n)*?\s+improve:\n(?:.*\n)*?\s+runs-on:.*\n)',
            r'\1' + "    env:\n      AGENT_NAME: EvoAgentX\n      AGENT_MODE: auto-improve\n",
            y,
            flags=re.M,
            count=1
        )
        return y2
    else:
        # Add a top-level env block just after permissions or concurrency if present.
        if "AGENT_NAME:" in y:
            return y
        insert = "\nenv:\n  AGENT_NAME: EvoAgentX\n  AGENT_MODE: auto-improve\n"
        if "permissions:" in y:
            y = re.sub(r'(^permissions:\n(?:\s+.*\n)+)', r'\1' + insert + "\n", y, flags=re.M, count=1)
            return y
        if "concurrency:" in y:
            y = re.sub(r'(^concurrency:\n(?:\s+.*\n)+)', r'\1' + insert + "\n", y, flags=re.M, count=1)
            return y
        # fallback: after on:
        y = re.sub(r'(^on:\n(?:.*\n)+?)\n', r'\1' + insert + "\n", y, flags=re.M, count=1)
        return y

y = ensure_agent_env(y)

# Add an explicit "Agent" step early (after checkout) if missing
if not re.search(r'^\s*-\s*name:\s*🧠\s*Agent\s*$', y, re.M):
    agent_step = (
        "\n      - name: 🧠 Agent\n"
        "        run: |\n"
        "          echo \"AGENT_NAME=${AGENT_NAME}\"\n"
        "          echo \"AGENT_MODE=${AGENT_MODE}\"\n"
    )
    # Insert after checkout step (best effort)
    y = re.sub(
        r'(\s*-\s*uses:\s*actions/checkout@v4[\s\S]*?\n\s*with:\n\s*fetch-depth:\s*0\s*\n)',
        r'\1' + agent_step,
        y,
        count=1
    )

wf.write_text(y, encoding="utf-8")
print("OK")
PY

  log "🔍 Vérif workflow (agent):"
  grep -nE "AGENT_NAME|AGENT_MODE|name:\s*🧠 Agent" "$WF" || true
else
  log "⚠️ Workflow introuvable: $WF (je n'ajoute pas l'agent)"
fi

# Push main
log "⬆️ Push $MAIN_BRANCH -> $REMOTE/$MAIN_BRANCH"
git push "$REMOTE" "$MAIN_BRANCH"

# Delete dev branch (remote + local)
if [ "$DEV_EXISTS_REMOTE" -eq 1 ]; then
  log "🧹 Suppression branche distante: $REMOTE/$DEV_BRANCH"
  git push "$REMOTE" --delete "$DEV_BRANCH" || log "⚠️ Impossible de supprimer $REMOTE/$DEV_BRANCH (droits?)"
fi

if [ "$DEV_EXISTS_LOCAL" -eq 1 ]; then
  log "🧹 Suppression branche locale: $DEV_BRANCH"
  git branch -D "$DEV_BRANCH" || log "⚠️ Impossible de supprimer la branche locale $DEV_BRANCH"
fi

# Optional: set default branch to main (needs gh + permission)
if command -v gh >/dev/null 2>&1; then
  log "🔧 Tentative: définir la branche par défaut sur '$MAIN_BRANCH' (via gh)"
  if gh repo set-default "$MAIN_BRANCH" >/dev/null 2>&1; then
    log "✅ Branche par défaut mise à '$MAIN_BRANCH'"
  else
    log "⚠️ gh n'a pas pu changer la branche par défaut (permissions ou repo?)"
    log "   Tu peux le faire dans GitHub: Settings → Branches → Default branch"
  fi
else
  log "ℹ️ gh CLI non trouvé. Si besoin, change la default branch dans GitHub UI."
fi

log "✅ Terminé."
log "🧾 Statut git:"
git status --porcelain || true
